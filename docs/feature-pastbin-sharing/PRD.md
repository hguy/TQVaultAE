## Problem Statement

TQVaultAE users manage extensive item collections in their vaults, but there is no way to share items, vault tabs, or entire vaults between players. A player who wants to give an item to a friend must manually describe it; there is no copy-paste, no file exchange, and no online link sharing. The tool is isolated.

## Solution

Add import and export capabilities at three scopes — single item, vault tab, full vault — through three transport channels: clipboard copy/paste (Ctrl+C / Ctrl+V), file save/load (.json), and PasteBin link sharing. An export serializes the selection into a JSON envelope (base64-encoded for clipboard and PasteBin; raw JSON for files). On import, the tool detects the clipboard format (PasteBin URL vs base64 payload), validates the data, checks placement feasibility, and imports items into the target vault with a clear result report.

## User Stories

1. As a player, I want to copy a single item to my clipboard, so that I can paste it into another TQVaultAE instance or share the text with a friend.
2. As a player, I want to copy multiple selected items at once, so that I can share a set of items instead of one at a time.
3. As a player, I want to right-click an item and export it to a .json file, so that I can save it for later use or send it to someone.
4. As a player, I want to right-click an item and export it to PasteBin, so that I can share a link instead of a file.
5. As a player, I want to export an entire vault tab's contents, so that I can share a themed collection of items (e.g., all my warrior gear).
6. As a player, I want to export my entire vault (all 12 tabs), so that I can back up or share my complete collection.
7. As a player, I want to paste clipboard content (Ctrl+V) to import items, so that I can receive items from a friend's export.
8. As a player, I want to import from a .json file via a file dialog, so that I can load items someone sent me.
9. As a player, I want to import from a PasteBin link in my clipboard, so that I can retrieve items shared via a URL.
10. As a player, I want the tool to detect whether my clipboard contains a PasteBin URL or a base64 payload, so that I don't need separate import actions.
11. As a player, I want to see a clear error when I try to import invalid or corrupted data, so that I know the paste or file is bad.
12. As a player, I want imported items to restore to their original vault-tab positions when the target tab is empty, so that the layout is preserved.
13. As a player, I want imported items to fill the first available space when the target tab is not empty, so that nothing gets lost.
14. As a player, I want to see a result report after import (e.g., "Imported 5 of 7 items — 2 could not be placed"), so that I know what happened.
15. As a player, I want to import a full vault into an existing vault with a Replace option (clear all tabs first), so that I can overwrite my current vault with a shared one.
16. As a player, I want to import a full vault as a New Vault with auto-generated name (suffix on conflict), so that I can keep the shared vault separate.
17. As a player, I want a Cancel option when importing a full vault, so that I can back out if I change my mind.
18. As a player, I want a strong data-loss warning when choosing Replace on a non-empty vault, so that I don't accidentally erase my items.
19. As a player, I want the PasteBin export option to be greyed out until I configure my PasteBin API key, so that I understand the requirement.
20. As a player, I want to configure my PasteBin API key and paste expiration in the Settings dialog, so that I control how my shared items are stored.
21. As a player, I want to always be able to import from a PasteBin link, even without an API key, because PasteBin links are public.
22. As a player, I want to export a single item to PasteBin, so that I can quickly share a god-tier drop with the community.

## Implementation Decisions

### Transport Channels

- **Clipboard:** On Ctrl+C (when items are selected) serialize to Export Format, base64-encode, place on clipboard. On Ctrl+V, detect clipboard content: if it matches a PasteBin URL (`https://pastebin.com/*`), fetch from PasteBin; otherwise attempt base64 decode and parse. If neither works, show an error.
- **File:** Open/save dialog for `.json` files. Raw JSON — not base64-encoded — so users can inspect files in a text editor.
- **PasteBin:** Upload the base64-encoded Export Format text to PasteBin; place the returned URL on the user's clipboard. Download fetches the raw paste text, base64-decodes it, and parses as Export Format.

### Export Format Schema

A JSON envelope with a discriminator field:

```
{ "formatVersion": 1, "scope": "item",  "data": <serialized Item> }
{ "formatVersion": 1, "scope": "tab",   "data": { "sackNumber": int, "sackType": string, "iconInfo": <BagButtonIconInfo>, "items": [<Item>, ...] } }
{ "formatVersion": 1, "scope": "vault", "data": { "name": string, "sacks": [{ "sackNumber": int, "items": [<Item>, ...] }, ...] } }
```

The full Item is serialized, including position, seed, affixes, relic sockets, stack size, and DLC origin. ContainerPlace metadata is preserved in the export and updated on import.

### Import Placement Logic

When importing items into a target SackCollection:

1. Try placing the item at its original `(PositionX, PositionY)` from the export.
2. If the original position is already occupied OR out of bounds of the target grid, fall back to `ItemMovementService.FindOpenCells`.
3. If `FindOpenCells` also returns `(-1, -1)`, skip that item.
4. After all items are processed, report: "Imported N of M items. X items could not be placed (no space)."

No pre-import space check. Placement is attempted item-by-item and results are aggregated.

### Full Vault Import Strategy

When importing a full vault, present the user with a dialog with three options:

- **Replace:** Clear all 12 tabs in the target vault, then import the source data. Requires explicit confirmation with a strong warning about data loss.
- **Create New Vault:** Auto-generate a new vault file with a unique name (suffix the vault name on conflict) and import into it.
- **Cancel:** Abort the import.

No partial merge of tabs into an existing vault.

### Vault Format Constraints

- Vaults have exactly **12 tabs** (SackCollections). This is fixed and invariable.
- Each vault tab has a grid of **18 columns × 20 rows** (cells).

### New Module: ItemExchangeService

Orchestrates all import and export operations:

- Serializes domain entities (Item, SackCollection, PlayerCollection) to/from the Export Format JSON.
- Base64-encodes/decodes payloads for clipboard and PasteBin.
- Detects clipboard format (PasteBin URL vs base64 vs invalid).
- Coordinates with `ItemMovementService` for item placement (FindOpenCells, CanPlaceItem, UpdateItemLocation).
- Coordinates with `VaultService` for vault-level operations (CreateVault, LoadVault, creating empty sacks).
- Returns structured import result reports (success count, skipped count, reasons).

Interface contract (not implementation):

```
// Serialization
string SerializeItem(Item item) → Export Format JSON
string SerializeSackCollection(SackCollection sack) → Export Format JSON
string SerializePlayerCollection(PlayerCollection vault) → Export Format JSON

// Deserialization  
ImportResult ImportFromJson(string json) → parses scope, validates, returns structured result

// Clipboard
string EncodeToClipboardPayload(string json) → base64
bool IsPasteBinUrl(string clipboardText)
```

### New Module: PasteBinService

Encapsulates PasteBin API HTTP interaction:

- `Task<string> UploadAsync(string text)` — POSTs base64-encoded export data to PasteBin API, returns the paste URL. Uses the API key from `UserSettings` and expiration from `UserSettings`.
- `Task<string> FetchPasteAsync(string pasteUrl)` — GETs the raw paste content from a PasteBin URL.

Paste visibility is always `Unlisted` (not on public feed, but accessible via direct URL). No API key is needed for fetch.

### Modified: UserSettings

Add two new XML-serialized properties:

- `PasteBinApiKey` (string) — the user's PasteBin developer API key. Empty by default.
- `PasteBinExpiration` (string) — paste expiration duration. Default `"1M"`. Valid values: `N` (never), `10M`, `1H`, `1D`, `1W`, `2W`, `1M`, `6M`, `1Y`.

### Modified: SettingsDialog

Add:
- A text input field for `PasteBinApiKey` (label: "PasteBin API Key").
- A dropdown/combo box for `PasteBinExpiration` with human-readable labels mapped to API values (e.g., "10 Minutes" → `10M`, "1 Month" → `1M`).

### Modified: UI (GUI Layer)

- **BagButton (tab)** context menu: add "Export Tab" → sub-items for File, Clipboard, PasteBin.
- **Item right-click** context menu: add "Export Item" → sub-items for File, Clipboard, PasteBin.
- **Vault combo box area** (right-side button, mirroring the player combo pattern): add "Export Vault" → sub-items for File, Clipboard, PasteBin.
- **Ctrl+C** when items are selected: triggers export to clipboard (single or multi-select items as a tab-level payload). Multi-select is achieved via the existing Ctrl+LeftClick behavior.
- **Ctrl+V** from vault panel: triggers import from clipboard.
- PasteBin export menu items are disabled (greyed out) when `PasteBinApiKey` is not configured.

### User Notification

- Simple status messages (import result counts): via `IUIService.NotifyUser` (status bar notification).
- Error/warning dialogs (invalid data, no space, data loss warning): via `IUIService.ShowWarning` / `IUIService.ShowError` with appropriate `ShowMessageButtons`.

## Testing Decisions

Tests verify observable behavior, not implementation. Target the new modules in isolation with mocked dependencies.

### What makes a good test

- Verifies a business rule or edge case, not a property assignment.
- Uses Arrange-Act-Assert and mocked dependencies.
- Tests the public interface of the module.
- Follows existing xUnit + AwesomeAssertions + Moq patterns (see `VaultServiceTests`, `GameFileServiceTests` for prior art).

### Test Plan (simplest to most complex)

1. **Export Format serialization/deserialization** — Round-trip: domain entity → JSON → domain entity. Assert all fields survive. Test each scope (item, tab, vault). No mocking needed.
2. **PasteBinService** — Mock `HttpClient`. Test upload success returns URL, upload failure (API error) throws, fetch success returns text, fetch failure (invalid URL, network error) throws.
3. **ItemExchangeService — placement logic** — Mock `IItemMovementService`. Assert that on empty sack, items are placed at original position. Assert fallback to FindOpenCells when original slot is occupied. Assert skip-and-report when FindOpenCells returns (-1, -1). Assert correct result counts (imported N of M).
4. **ItemExchangeService — full vault import** — Mock `IVaultService`. Assert Replace clears tabs then imports. Assert Create New calls CreateVault with a suffixed name on conflict. Assert Cancel does nothing. Assert data-loss warning is raised for non-empty targets.
5. **Clipboard format detection** — Assert PasteBin URL detection (`https://pastebin.com/abc` → true, random text → false). Assert base64 decode and validation of Export Format envelope.
6. **PasteBin export gating** — Assert PasteBin export menu is disabled when API key is empty; enabled when configured.

### Prior art for tests

- `VaultServiceTests` — tests vault CRUD with mocked file/game services.
- `GameFileServiceTests` — tests backup logic with `ShowMessageUserEventHandlerEventArgs` for user dialogs.
- `SackCollectionProviderTests` — tests serialization round-trips.

## Out of Scope

- Partial merge of imported tabs into an existing vault (only Replace or New Vault).
- Importing items into the player inventory or stash (vaults only for this feature).
- Encryption or password protection of exports.
- PasteBin account management beyond API key entry.
- Batch or bulk PasteBin management (delete, update, list pastes).
- Import/export of player characters or stash files.
- Any server-side relay other than PasteBin.

## Further Notes

- All work should be committed to the `optim-share-items` branch.
- Documentation file should be created at `./documentation/IMPORT_EXPORT.md` describing the feature, including step-by-step instructions and a guide on how to obtain a PasteBin API key (with a link to https://pastebin.com/doc_api).
- Commit at each logical step for easy tracking and revert.
