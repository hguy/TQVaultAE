# TQVaultAE Domain Glossary

## Core Entities

- **Item** — a single Titan Quest item (gear, relic, scroll, potion, etc.) with position, seed, affixes, relic sockets, stack size, and DLC origin
- **SackCollection** — a single tab/bag within a vault or player file; a grid inventory of Items with a SackType
- **PlayerCollection** — a vault or player file on disk (.json for vaults, .chr for players); contains an array of SackCollections and equipment data
- **ContainerPlace** — record of where an item lives: file path, sack number, sack type, stash type
- **Vault** — a .json file flagged `IsVault=true`; a named collection of SackCollections (tabs) stored in the vault save folder
- **Stash** — the in-game stash file, keyed by player name, with a single SackCollection

## Transport Formats

- **Export Format** — a JSON structure representing Items, SackCollections, or whole Vaults for interchange
- **Clipboard Payload** — the Export Format encoded as base64, for copy/paste between tool instances
- **PasteBin Payload** — the Export Format encoded as base64, uploaded to PasteBin for link sharing
- **File Payload** — the Export Format as raw JSON written to a .json file (not base64)

## PasteBin Integration

- **API Key** (`PasteBinApiKey`): stored in `UserSettings`, serialized to `UserConfig.xml`. Entered in SettingsDialog.
- **Expiration** (`PasteBinExpiration`): stored in `UserSettings`, defaults to `"1M"`. Configurable in SettingsDialog with values matching PasteBin API (`N`, `10M`, `1H`, `1D`, `1W`, `2W`, `1M`, `6M`, `1Y`).
- **Visibility**: `Unlisted` (fixed — accessible by URL, not on public feed).
- **Export to PasteBin**: greyed out / disabled when `PasteBinApiKey` is empty.
- **Import from PasteBin**: always allowed (public URLs require no API key).
- **Upload flow**: serialize to Export Format JSON → base64-encode → POST to PasteBin API → place returned URL on user's clipboard.
- **Download flow**: detect PasteBin URL in clipboard → GET raw paste → base64-decode → parse as Export Format.

## Vault

- A vault has exactly 12 tabs (SackCollections). This is fixed and invariable.
- Tabs within a vault have a fixed grid size (width × height).

## Full-Vault Import Strategy

Dialog with three choices:
1. **Replace** — clear all 12 tabs in target vault, then import source data (strong data-loss warning required)
2. **Create new vault** — auto-generate unique filename with a suffix if name conflicts
3. **Cancel** — no action taken

No partial merge is supported.

## Export Scope Selection

- **Item**: Ctrl+LeftClick (multi-select), then Ctrl-C copies to clipboard. Right-click context menu for export to File or PasteBin.
- **Multi-selected items**: exported as a tab-level payload via the BagButton menu.
- **Tab**: each tab's BagButton has a context menu with export options (File, PasteBin, Clipboard).
- **Vault**: a button on the right of the vault combo box (mirroring the player combo pattern) opens a context menu with export options.

## Clipboard Transport

On Ctrl-V (import):
1. If clipboard text matches `https://pastebin.com/*` → fetch from PasteBin, decode base64, parse as Export Format
2. Else → attempt base64 decode, then parse as Export Format
3. If neither succeeds → show "invalid data" error

On Ctrl-C (export): serialize the Export Format, base64-encode it, place on clipboard.

## Import Result Reporting

After an import operation, report to the user: "Imported N of M items. X items could not be placed (no space)." No pre-import space check is performed.

## Import Logic

When placing items into a target sack:

1. Try the item's original (PositionX, PositionY) from export
2. If that slot is occupied OR out of the target grid's bounds → fall back to first-available-space via `ItemMovementService.FindOpenCells`
3. If `FindOpenCells` also fails → skip the item and report which items couldn't fit

## Services

### IItemExchangeService (new)
Orchestrates import and export operations:
- Serialize Items/SackCollections/PlayerCollections to/from the Export Format JSON
- Base64 encode/decode for clipboard and PasteBin
- Coordinate with `IItemMovementService` for item placement
- Coordinate with `IVaultService` for vault-level create/load
- HTTP interaction with PasteBin API (upload/download)

Clipboard read/write and file dialogs remain in the GUI layer.

## Export Format

A JSON structure with a `formatVersion`, `scope` discriminator, and `data` payload:

- `formatVersion: 1` — schema version for forward compatibility
- `scope: "item"` — data is a single serialized Item
- `scope: "tab"` — data is a `{ sackNumber, sackType, iconInfo, items: [...] }`
- `scope: "vault"` — data is a `{ name, sacks: [{ sackNumber, items: [...] }] }`
