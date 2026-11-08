# Logic trapped in the GUI: inventory and extraction plan

Inventory made on 2026-11-08 over `src/TQVaultAE.GUI` (Forms, Components, Dialogs, Tooltips, Models; Designer files and the
Debug-only `Automation` folder excluded). Line numbers are those of that date: check them before moving code.

The goal: the GUI only displays and takes input. Rules, state changes, validation and text built from game data move to
`Application` (contracts, DTOs, results, session state), `Services` (orchestration), `Domain` (pure rules on the entities) or
`Presentation` (text). Once moved, they can be tested without Windows Forms.

## What is already out of the GUI

`Domain/Actions/ItemActionRules`, `Domain/Forge/ForgeRules` and `ForgeMerge`, `Application/Results/CsvRow`,
`Services/ItemMovementService` (`FindItem`, `CanPlaceItem`, `IsItemValidForStashType`, `UpdateItemLocation`),
`IMessageBoxService` and `IUIService`.

## Cross-cutting findings

- `GuiServices`, the bundle every form and control receives, exposes four Data providers (`IDatabase`, `IItemProvider`,
  `IPlayerCollectionProvider`, `ITQDataService`); about fifteen GUI files use them without a service in between. Each lot below
  should remove some of these uses; the end state is a `GuiServices` without Data providers.
- Some Services code is written but unused while the GUI keeps its own copy: `ItemMovementService` (never injected),
  `VaultService.DeleteVault/RenameVault/GetVaultList`, `PlayerService.CreatePlayerFileWatcher`,
  `StashService.Create*FileWatcher`, `ItemDatabaseService.WarmUp` (copied by `SearchDialogAdvanced.InitItemDatabase`).
- Message boxes asked in the middle of a rule (`goto askAgain`, Abort/Retry/Ignore loops). The extracted code returns a
  result or takes a callback; the GUI asks.
- Session state kept in form fields or combo selections (`resourcesLoaded`, `loadingComplete`, `configChanged`, current vault
  and character) belongs to `SessionContext`.

## Bugs found during the inventory

| Where | Bug |
|---|---|
| `Dialogs/ItemSeedDialog.cs:104` | Accepts `> 0 && < 0x7fff`: 32767 is refused though the message says 1–32767; `Item.GenerateSeed` returns 0–32766, so the Random button can give 0, refused by OK. |
| `Components/Panels/VaultPanel.ContextMenu.cs:385-418` | `CopyBagClicked`: `if (SuppressWarnings \|\| !Confirm) return;` never copies onto a non-empty bag when warnings are suppressed (it should copy without asking). |
| `Components/Character/ComboBoxCharacter.cs` | Archive and unarchive are written three times each; the two "unarchive all" paths do not update the player file path (only `:813` does). |
| `Forms/MainForm.Loading.cs:30-134` | Progress totals disagree: `LoadAllFilesTotal` counts 2c+v-1, `total` c+v-1, and 2c+v steps are reported. |
| `Forms/MainForm.Lifecycle.cs`, `MainForm.Loading.cs:140-159` | The vault and stash save errors (`vaultOnError`, `stashOnError`) are discarded, never reported. |
| `Forms/MainForm.Vault.cs:97-98` | `break` in the vault list loop, probably meant as `continue`. |
| `Dialogs/SettingsDialog*` | Game paths are upper-cased on Browse, not when typed. |
| `Program.cs:278-285` | Dead duplicated `else if` in `SetUILanguage`. |

## Inventory by theme

Risk: low (pure code, few callers), med (state or threading), high (core drag and drop path).

### Items: moves, stacking, editing

| # | Where | What | Target | Risk |
|---|---|---|---|---|
| I1 | `SackPanel.DragDrop.cs:235-252`, `SackPanel.ContextMenuHandlers.cs:636-653, 673-692` | Weighted bonus roll from a loot table, copied 3 times | `LootTableCollection.PickWeighted` (Domain), random source injected | low |
| I2 | `SackPanel.DragDrop.cs:212-318` | Relic/charm and potion stacking, relic completion | `ItemStackingService` (Services) | med |
| I3 | `SackPanel.DragDrop.cs:407-422`, `SackPanel.Geometry.cs:69-186`, `MainForm.Panel.cs:429-459` | Place, find, update location: copies of `ItemMovementService` | `ItemMovementService` (settle the `Place.Name` difference first) | low |
| I4 | `SackPanel.SackManagement.cs:32-141`, `SackPanel.Selection.cs:96-151` | Autosort, moving a selection: cell maths only | `ItemMovementService` | med |
| I5 | `SackPanel.ContextMenuHandlers.cs:346-727` | Change affixes and bonuses, complete relic, craft artifact, split, copy, seed; dispatch on localized menu text | `ItemEditService` + dispatch on `ItemActions` | low-med |
| I6 | `SackPanel.ContextMenu.cs:208-497, 628-665` | Affix, bonus and set member catalogs (LINQ) | `AffixCatalogService` (Application/Services) | low |
| I7 | `SackPanel.ContextMenu.cs:22-81`, `ContextMenuHandlers.cs:114-201`, `SackManagement.cs:148-275` | Auto-move destinations and the "sort by size, move each" loop, written 3 times | `ItemActionRules.MoveDestinations`, `AutoMovePlanner` | low / med |
| I8 | `EquipmentPanel.DragDrop.cs:454-511` (slot rules), `:150-258` (2H/1H swap) | Equipment rules | `EquipmentRules` (Domain) | low / high |
| I9 | `SackPanel.DragDrop.cs:96-210` | Drop rules | `ItemMovementService.TryDrop` | high, last |
| I10 | `SackPanel.cs:568-605`, `EquipmentPanel.cs:59-85`, `StashPanel.PlayerInfo.cs:32-105` | Requirements met, gear and skill stat bonuses, player info lines | `PlayerService`, a Presentation builder | low |

### Vaults, characters, stashes

| # | Where | What | Target | Risk |
|---|---|---|---|---|
| V1 | `MainForm.Vault.cs:197-336`, `VaultMaintenanceDialog.cs:217-396` | New, copy, rename, delete vault and their validation (3 copies) | `VaultService` (`ValidateMaintenance`, `CopyVault`, fixed `Rename/Delete`) | low-med |
| V2 | `MainForm.Vault.cs:67-158` | Vault list: Main Vault first, the other panel's vault excluded | `VaultService.BuildVaultChoices` | low |
| V3 | `VaultPanel.ContextMenu.cs:30-84, 219-418` | Bag copy/move/merge, CSV rows (2 copies), icon info copy (2 copies) | `VaultService`, `BagButtonIconInfo.Clone` | low |
| V4 | `MainForm.Player.cs:59-87, 331-392` | Character list, new name validation, duplication with rollback | `PlayerService` | low-med |
| V5 | `ComboBoxCharacter.cs:617-867` | Archive/unarchive (6 copies), tag name validation | `GameFileService`/`PlayerService`, `TagService` | low |
| V6 | `MainForm.Player.cs:201-296`, `MainForm.Stash.cs:46-130` | Load error → message mapping, copied 5 times | a load facade returning results + messages | low |
| V7 | `StashPanel.cs:384-614`, `StashPanel.BagButtons.cs:113-206`, `BagButtonBase.cs:245-263` | Bag id clamping, tooltip settings switch (3 copies) | `StashService`, a tooltip policy over `UserSettings` | low |

### Startup, saving, settings

| # | Where | What | Target | Risk |
|---|---|---|---|---|
| S1 | `MainForm.Lifecycle.cs:54-89`, `MainForm.Loading.cs:140-159`, save wrappers in `MainForm.Player/Vault/Stash` | Save order (players, vaults, stashes, config, git), retry loop | `ISaveOrchestrator` returning a `SaveResult` | med |
| S2 | `MainForm.Loading.cs:371-517` | Configuration persistence, restart decision after settings | `SettingsPersistence`, pure `RestartReason Evaluate(...)` | low |
| S3 | `MainForm.Loading.cs:30-134, 280-362` | Load all, startup selection (last character, last vault) | `ILoadAllService`, `StartupSelection.Resolve` | med |
| S4 | `Program.cs:218-336`, `Models/CommandLineArgs.cs` | Game paths, UI culture, map name, command line parsed twice | `StartupService`; `CommandLineArgs(string[])` in Application | med |
| S5 | `SettingsDialog.cs`, `SettingsDialog.EventHandlers.cs` (~750 lines) | Change flags set by hand in 28 handlers; rules between settings (cheats off clears edits, item edit off clears epic affixes, git off clears legacy backup...) | `SettingsEditor` over a copy of `UserSettings`, flags from a comparison | med |
| S6 | `SettingsDialog.cs:296-307, 547`, `UserSettings.cs:170`, `PasteBinService.cs:30` | PasteBin default expiration written 3 times; git URL check | constants and validator in Services/Config | low |
| S7 | `MainForm.Export.cs:99-366`, `MainForm.Clipboard.cs:38-97` | Import placement, import as new vault, text import dispatch (3 copies) | `ItemExchangeService` | low |
| S8 | `CharacterEditDialog.cs:86-374` | Masteries by level, attribute and skill refunds, money floor; `int.Parse` throws on bad XP text | `CharacterEditService`, `PlayerLevel` helpers | med |

### Search and tooltips

| # | Where | What | Target | Risk |
|---|---|---|---|---|
| T1 | `SearchDialogAdvanced.Populate.cs:42-437` | 17 copies of the category builder (one uses `TQCleanup`, the others `RemoveAllTQTags`) | `SearchCategoryService` | med |
| T2 | `SearchDialogAdvanced.Filters.cs:195-222, 487-514` | AND/OR combination of filter results, min ≤ max | `ItemDatabaseService`, `AdvancedSearchRequest` | low |
| T3 | `SearchDialogAdvanced.Queries.cs`, `Models/Search/SearchQueries.cs` | Saved queries: validation, JSON storage in the GUI, `BoxItem` holding controls | `SearchQueryService` + DTO in Application | med |
| T4 | `ResultsDialog.cs:107-131`, `FoundResultsTooltip.cs:135-145`, `BagButtonTooltip.cs:128-140` | Container labels, "N x name" summary (2 copies) | Presentation helpers | low |
| T5 | `ItemTooltip.cs:17-452`, `ItemTooltip.GrantedSkills.cs:95-277` | Which lines the item tooltip shows and in what order | `ItemTooltipContentBuilder` (Presentation) returning lines; the GUI renders | med-high |
| T6 | `BaseTooltip.cs:92-124`, `ItemProperties.cs:89-178` | Color tag splitting (2 copies), property sections (3 copies) | `TQColorHelper.SplitColorSegments` (Domain) | low |
| T7 | `ForgePanel.cs:484-686` | Forge commit, consuming materials, reset on mode downgrade | `ForgeService`, `ForgeRules.IsDowngrade` | low-med |
| T8 | `HighlightFilters.cs:195-385` | Building the filter values, "any filter active" | `HighlightFilterValues.IsEmpty`, `HighlightService` | low |

## Order of the lots

Each lot is one commit (or a few), with unit tests on the moved code, and is checked in the running app through the
`tqvault` MCP.

1. **Bugs** found above, each with a test where the code allows it.
2. **Quick wins, low risk**: I1, I3, I6, T4, T6, V5, V6, S6.
3. **Vaults and characters**: V1, V2, V3, V4, V7.
4. **Item editing and moves**: I5, I7, I10, T7, T8, I2, I4.
5. **Import/export and startup**: S7, S2, S4, S3.
6. **Saving**: S1 (data safety: extra care, tests on the order and on the error reporting).
7. **Search**: T2, T1, T3.
8. **Settings and character edit**: S5, S8.
9. **Item tooltip content**: T5.
10. **Last**: I8 swap, I9 drop rules; then remove the Data providers from `GuiServices`.
