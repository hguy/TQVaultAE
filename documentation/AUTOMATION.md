# Automation endpoint (Debug builds only)

Debug builds of TQVault host an [MCP](https://modelcontextprotocol.io) server inside the application.
An AI agent (Claude Code, or any MCP client) can use it to inspect and drive the running UI
without screenshots or real mouse clicks.

The item grids (`SackPanel`, `EquipmentPanel`) are owner-drawn, so UI Automation sees each one as a single pane.
This endpoint reads the app's own models instead, and drives drags through the panels' own mouse handlers.

**Release builds contain none of this.** The `Automation/` folder, the `ModelContextProtocol.AspNetCore` package
and the ASP.NET Core framework reference are only part of the `Debug` configuration, and every hook
in existing code is inside `#if DEBUG`.

---

## Transport and address

- Transport: MCP streamable HTTP, through the official C# SDK (`ModelContextProtocol.AspNetCore`) on Kestrel.
- Address: `http://127.0.0.1:47017/mcp`. It listens on loopback only.
- It starts with the application, before the first dialog, so startup message boxes can already be answered.
- The address is written to the log at startup:
  `Automation MCP endpoint listening on http://127.0.0.1:47017/mcp`

| Environment variable | Effect |
|---|---|
| `TQVAULT_MCP_PORT` | Listen on another port. |
| `TQVAULT_MCP_DISABLED=1` | Do not start the endpoint. |
| `TQVAULT_MCP_ALLOW_SAVES=1` | Turn the save guard off by default (see below). |

## Register it in Claude Code

The repository's `.mcp.json` already registers it as `tqvault`. Follow these steps:

1. Start a Debug build: run `dotnet build TQVaultAE.slnx`, then `src/TQVaultAE.GUI/bin/Debug/net10.0-windows/TQVaultAE.exe`.
2. Open Claude Code at the repository root, and approve the project server when you are asked.

The tools show up as `mcp__tqvault__*` in sessions started at the repository root. A local registration counts only for the folder the command runs in, not the whole repository. To register the server by hand from another folder, use:

```bash
claude mcp add --transport http tqvault http://127.0.0.1:47017/mcp
```

If you use another port (`TQVAULT_MCP_PORT`), update the URL in `.mcp.json` too.

## Tools

All UI access is marshalled to the UI thread with `Control.Invoke`.
Panel ids are `vault`, `secondaryVault`, `player` (main inventory), `playerBag`, `stash` and `equipment`.
Cells are grid coordinates, as reported by `get_state`.

| Tool | What it does |
|---|---|
| `list_windows` | The process' visible top-level windows: WinForms forms and native message boxes, with title, class, modal state, message text and buttons. |
| `close_dialog(title, button)` | Presses a dialog button by its text, for example `("Default Vault Path", "OK")`. It works for message boxes and WinForms forms, even when they are not in the foreground. |
| `list_forms` | The form and dialog types registered in the container. |
| `open_form(name, show)` | Creates a form or dialog from the container, as `FormFactory` does, so its `InitializeComponent` and resources run. It reports the title, size and control count. With `show=false` (the default) the form is then disposed; with `show=true` it opens modeless over the main window, for `capture` and `close_dialog`. Singletons (`MainForm`, `BagButtonSettings`) are only reported. Dialogs that need data first may fail when shown; the error goes to `recent_log`. |
| `open_bag_settings(panel)` | Opens the bag button settings (icon and label picker) for the current bag of a panel, like its "Change icon" menu entry. It waits for the game icons and reports how many loaded. The dialog is modal: capture it with `capture("Bag Button Settings")` and close it with `close_dialog`, using the Cancel button's text in the game language (for example "Annuler"). |
| `highlight_filters(check?, uncheck?, apply?)` | Opens the highlight filters panel of the main window (the "Highlight items having" link), ticks and unticks its check boxes by text or name (`"Having Skill,Set Item"`), and applies the filters like its Apply button when `apply` is true. Returns the check boxes and how many items are highlighted. Untick them and apply again, or use the panel's Reset, to clear the highlight. |
| `get_state(panel?)` | The selected vault and character, the available characters and vaults, `PlayerReadonly`, the save guard, the drag state, every panel (shown, sack type, bag, item count, modified). With `panel`, it adds the items of that panel's current sack: name, base name, record id, cell location and size. For `equipment` it also lists the slot rectangles. |
| `owned_skill_items` | The owned items that grant a skill, from the search database (vaults, characters and stashes when *Load all files* is on): record ids, seed, vars, stack size, size in cells, location, and the skills with their level and the record that grants them. It writes nothing; its output is enough to write a vault file. |
| `select_character(name)` / `select_vault(name)` | Select an entry in the drop-downs, as listed by `get_state`. |
| `select_bag(panel, bag)` | Clicks a bag button. `stash` bag 0 shows the equipment panel. |
| `pick_item(panel, x, y)` | Picks up the item at a cell, like a left click there. |
| `move_drag(panel, x, y)` | Hovers the dragged item so that its top-left corner lands on cell (x, y). The result reports the cells under the item. |
| `drop_item(panel, x, y)` | Drops the dragged item with its top-left corner on cell (x, y). It reports whether the panel accepted the drop. |
| `cancel_drag` | Returns the dragged item to where it was picked up, like a right click. |
| `create_item(panel, baseItem, prefix?, suffix?, relic?, relicBonus?, relic2?, relicBonus2?, seed?)` | Builds an item in memory from database record ids and drags it, like the Duplicate menu entry; place it with `drop_item` (a created item cannot be cancelled back). It tests combinations no vault holds, for example a relic whose completion bonus grants a skill, or any record in any slot as the Forge allows in God mode. The save guard keeps it from being written. |
| `hover_item(panel, x, y)` | Rests the virtual cursor on a cell without dragging. TQVault shows the item's tooltip and writes its name in the notification bar. The result gives the item, the notification text, the tooltips shown, and a PNG of the tooltip taken at once, since a tooltip can close by itself. |
| `hover_end` | Moves the virtual cursor off the panel, which closes the tooltip. |
| `context_menu(panel, x, y)` | Right-clicks a cell through the panel's mouse handlers and returns the context menu built for it: entry texts, `-` for separators, sub menus with their entries. The menu is closed without choosing anything. The current selection counts, as for a real right click. |
| `zoom(step)` | Zooms the main window like its shortcuts: `in` (Ctrl +), `out` (Ctrl -), `reset` (Ctrl Home). Returns the zoom, the DPI factor, the scale (their product) and the client size. The zoom is written to `UserConfig.xml` like a user zoom (the save guard does not cover it), so end with `reset`. To compare with the 96 DPI layout on a high DPI screen, start the app with the environment variable `__COMPAT_LAYER=DPIUNAWARE`. |
| `control_tree(form, depth)` | Lists the controls of an open form (the main window by default) with type, bounds, font and visibility, plus the form's DPI and the scale, zoom and DPI factor, to check layout and scaling. |
| `capture(control)` | A PNG made with `Control.DrawToBitmap`, returned as an image and also saved under `%TEMP%`. Targets: a panel id, `<panel>.container` (the panel with its bag buttons), `main`, `tooltip` (the last tooltip shown), or part of a form title. It works when the window is behind other windows. |
| `recent_log(n)` | The last `n` log lines, kept in memory. Every message box the GUI shows through `IMessageBoxService` is logged with its caption, buttons and text, and so is the answer to a question. |
| `recent_sounds(n)` | The last `n` sounds played. For a named sound, `loaded` and `wavBytes` tell whether its data could be loaded. The level-up MP3 is converted to WAV by NAudio first. |
| `forge_show(show)` | Shows or hides the forge, like the Forge button. Items for the forge are picked from the `vault` panel with `pick_item`. |
| `forge_state` | Mode, hardcore, the item in each slot (`Base`, `Prefix`, `Suffix`, `Relic1`, `Relic2`) with the parts it offers in God mode, the preview of the forged item, and the messages shown to the user. |
| `forge_set_mode(mode)` / `forge_set_hardcore(enabled)` | Like the mode radio buttons (`Game`, `Strict`, `Relax`, `God`) and the Hardcore box. A stricter mode clears the slots, as in the app. |
| `forge_drop(slot)` | Drops the dragged item on a forge slot. The rules of the current mode decide. A refusal is reported in `newMessages`, and the item stays dragged. |
| `forge_select_part(slot, part)` | God mode: chooses which part of a material item is merged, like its combo box. |
| `forge_reset` / `forge_cancel` / `forge_forge` | Like the Reset, Cancel and Forge buttons. `forge_forge` merges in memory only, since the save guard keeps it from being written, and reports the forged item and the sounds played. |
| `set_save_guard(enabled)` | Turns the save guard on or off. |

Every forge tool returns the forge state, with the `newMessages` and `sounds` that the call produced.

### How drags are simulated

The real cursor is never moved. The tools call the panels' own `MouseMove` / `MouseDown` / `MouseLeave` handlers
with synthetic `MouseEventArgs`, and a Debug-only `VirtualCursor` replaces `Cursor.Position` in
`SackPanel.PaintCallback`. After each move, the panel is painted synchronously, so the drop target cells
(`CellsUnderDragItem`) are computed by the regular code.

All the app's rules still apply: item requirements, slot types, stacking and swapping.

### Example: forge in God mode

```text
forge_show()                                 -> forge shown, mode Game
forge_set_mode("God")
pick_item("vault", 0, 3); forge_drop("Base")  -> base item in the Base slot
pick_item("vault", 0, 1); forge_drop("Prefix")
forge_select_part("Prefix", "Prefix")        -> preview: base item with the other item's prefix
forge_forge()                                -> forgedItem, sounds: PlayLevelUp UI_LEVELUP.MP3 loaded, wavBytes 626732
```

In Debug builds, `ISoundService` is wrapped by `RecordingSoundService`, which records every sound for `recent_sounds`.

## Safety

- **`PlayerReadonly`**: when it is on (the default), `pick_item` and `drop_item` refuse the equipment panel,
  just as the panel ignores real clicks. Hovering it with `move_drag` is allowed.
- **Save guard**: once any tool has driven the UI (selection, drag, dialog answer), TQVault refuses to:
  - save players, vaults and stashes, including on exit;
  - save its configuration on exit;
  - duplicate a character or import a vault;
  - git commit and push the vault folder on exit.

  Each refused write is logged as `Save refused while automation is driving TQVault: ...`.
  To allow saves, start the app with `TQVAULT_MCP_ALLOW_SAVES=1`, or call `set_save_guard(false)`.
  Small UI preferences (window position, tooltip toggles) are still written to `UserConfig.xml`.

## Example session

```text
get_state()                                  -> characters, vaults, panels
select_character("Warfare, Guerrier - Niveau : 67")
select_bag("stash", 0)                       -> equipment panel shown
get_state("player")                          -> helmet at x=0, y=0, 2x2
pick_item("player", 0, 0)                    -> dragging, cellsUnderItem (0,0 2x2)
move_drag("equipment", 4, 0)                 -> hoveredPanel equipment, cellsUnderItem (4,0 2x2)
capture("equipment.container")               -> helmet drawn over the head slot, green highlight
drop_item("player", 0, 0)                    -> accepted, dragging false
capture("player")
```

Dropping an item back on its own cells re-places it and marks the sack modified, exactly like a real drop.
Use `cancel_drag` to put it back untouched.

## Limitations

- Task dialogs (`TaskDialog`) draw their buttons without child windows. For those, `close_dialog` only knows the
  standard buttons (OK, Cancel, Yes, No, Retry, Close...), through their command ids.
- The tools that need the main window answer "not ready" until it has been created.
  `list_windows` and `close_dialog` work from the start.
- `capture("equipment")` shows the item grid only. Use `equipment.container` to include the stash tabs.
- The real mouse still works while automation drives the app. Moving it over the panels during a drag
  repaints them, but the virtual cursor keeps deciding where the dragged item is drawn and dropped.
