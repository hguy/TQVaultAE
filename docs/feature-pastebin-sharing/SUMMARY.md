## Goal
- Implement item sharing between TQVaultAE users through clipboard, file, and PasteBin channels at item/tab/vault scopes.

## Constraints & Preferences
- Base64 only for clipboard/PasteBin transport; file export is raw .json
- Vaults have exactly 12 fixed tabs (18×20 grid)
- No partial merge for full vault import (only Replace / Create New Vault / Cancel)
- Import placement: try original position → FindOpenCells fallback → skip + report
- Items hydrated via `IItemProvider.GetDBData` (width/height come from DB, not export)
- `ExportScope` enum replaces magic strings; single `ImportFromJson` call (no `DetectScope`)
- Terminology: "Share" not "Export"; 🔗 U+1F517 emoji prefix on parent menu items
- Menu items must match existing `ToolStripMenuItem` styling pattern (BackColor/Font/ForeColor/DisplayStyle)
- Vault export button must use same game bitmaps (`HUDMENUSKILLBUTTON*`) as player combobox tools button
- Menu labels must use Resx resources, not hardcoded strings
- PasteBin `api_paste_name` property must be set: full item name (Item scope), VaultName (Vault scope), VaultName/TabName or VaultName/Tab # (Tab scope)

## Progress
### Done
- All 5 slices implemented on `optim-share-items` branch (#25→#29 closed)
- `IItemExchangeService` + `ItemExchangeService` with serialization, base64, import placement logic
- `PasteBinService` with HTTP upload/fetch, `SettingsDialog` API key + expiration config
- Export Format DTOs with `ExportScope` enum (Item/Tab/Vault)
- `ImportFromJson` deserializes `ItemDto` directly, hydrates via `_itemProvider.GetDBData`
- `BagButtonIconInfoDTO` eliminated (use domain `BagButtonIconInfo` directly)
- `ItemExportDTO` eliminated (use `Data.Dto.ItemDto` directly)
- `TabExportDTO`, `VaultExportDTO`, `ItemDtoExtensions` moved from Application to Services
- CSV items regrouped under "Export to CSV" submenu with proper styling
- `SackPanel` ExportItem context menu Click handlers wired (Clipboard + File were missing)
- All terminology renamed to "Share" with 🔗 emoji prefix on group items
- `sackType` property removed from `TabExportDTO` and `ImportResult` (unnecessary — target sack type always known from context)
- Vault export button now loads `HUDMENUSKILLBUTTON*` game bitmaps with `UseCustomGraphic = true`; vault combobox width adjusted for alignment; falls back to "..." text if bitmaps unavailable
- Menu labels in `SetupVaultExportButton()` changed from hardcoded strings to `Resources.PlayerPanelMenuExportTabFile`, `PlayerPanelMenuExportTabClipboard`, `PlayerPanelMenuExportTabPasteBin`
- `api_paste_name` added to PasteBin upload: Item name (Item scope), VaultName (Vault scope), VaultName/TabName or VaultName/Tab # (Tab scope)

### In Progress
- (none)

### Blocked
- (none)

## Key Decisions
- `ExportScope` enum replaces string scope (compile-time safety, case-insensitive `Enum.TryParse` for import)
- `DetectScope` removed: `ImportFromJson` parses once and returns `ImportResult` with `Scope` set
- `ItemExportDTO` eliminated: Export format serializes `Data.Dto.ItemDto` directly; Services references Data project
- Width/height excluded from export: hydrated via `IItemProvider.GetDBData` post-deserialization (mirrors `ParseJsonData` pattern)
- `TabExportDTO`/`VaultExportDTO` moved to Services project (Application cannot reference Data due to cycle)
- Menu terminology: "Share to Clipboard" / "Share to File..." / "Share to PasteBin" under "🔗 Share" parent
- `sackType` removed from export format — not consumed during import (target type known from context)
- Vault export button styled identically to player combobox tools button: same `HUDMENUSKILLBUTTON*` game bitmaps, `UseCustomGraphic = true`, combo box width shrunk to maintain total panel width
- Paste name format: Item uses `ToFriendlyNameResult.FullNameClean` (strips TQ tags); Vault uses `PlayerName`; Tab uses `VaultName/TabName` (from `BagButtonIconInfo.Label`) or `VaultName/Tab #` fallback
- Paste name is an optional parameter (`string pasteName = null`) threaded through `IItemExchangeService.ExportToPasteBinAsync` → `IPasteBinService.UploadAsync`

## Relevant Files
- `src/TQVaultAE.Services/ItemExchangeService.cs`: Core import/export orchestration
- `src/TQVaultAE.Services/PasteBinService.cs`: `UploadAsync` — accepts `pasteName` → sets `api_paste_name`
- `src/TQVaultAE.Application/Contracts/Services/IPasteBinService.cs`: Interface with `pasteName` optional param
- `src/TQVaultAE.Application/Contracts/Services/IItemExchangeService.cs`: Interface with `pasteName` optional param
- `src/TQVaultAE.GUI/MainForm.cs`: `ExportVaultToPasteBinClicked` — passes `vault.PlayerName`
- `src/TQVaultAE.GUI/Components/SackPanel.cs`: `ExportItemToPasteBinClicked` — passes `ItemProvider.GetFriendlyNames(focusedItem).FullNameClean`
- `src/TQVaultAE.GUI/Components/VaultPanel.cs`: `ExportTabToPasteBinClicked` — passes `VaultName/TabName` or `VaultName/Tab #`
- `docs/SUMMARY.md`: Progress summary
