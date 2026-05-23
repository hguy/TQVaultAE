# Slice 1: Ctrl+C / Ctrl+V Item Clipboard Exchange

- **Type:** AFK
- **Blocked by:** None — can start immediately
- **User stories:** #1, #11, #12, #13, #14

## What to build

Implement single-item clipboard export and import for TQVaultAE vaults.

When a user selects one or more items (existing Ctrl+LeftClick multi-select) and presses Ctrl+C, the tool serializes the item(s) into the Export Format with `scope: "item"`, base64-encodes the result, and places it on the system clipboard.

When a user presses Ctrl+V while viewing a vault tab, the tool reads the clipboard, attempts to base64-decode the content, validates it against the Export Format, deserializes the item(s), and imports them into the currently-visible SackCollection using the Import Placement Logic. A result report is shown via NotifyUser.

This slice includes:
- The `IItemExchangeService` interface and implementation (item-scope serialization/deserialization)
- Export Format DTOs with scope discriminator
- Base64 encode/decode
- Clipboard read/write integration via MainForm key handlers
- Import Placement Logic: try original position → fallback to FindOpenCells → skip if no space
- Result reporting via IUIService.NotifyUser
- Item right-click context menu: "Export Item → Clipboard"

## Acceptance criteria

- [ ] Selecting an item and pressing Ctrl+C places a base64-encoded Export Format payload on the system clipboard
- [ ] Pressing Ctrl+V in a vault tab panel detects a valid clipboard payload and imports the item
- [ ] On an empty sack, the imported item is placed at its original (PositionX, PositionY)
- [ ] If original position is occupied or out of bounds, the item falls back to first available space via ItemMovementService.FindOpenCells
- [ ] If no space exists (FindOpenCells returns -1,-1), the item is skipped and reported
- [ ] A NotifyUser message reports "Imported N of M items" (or "X items could not be placed")
- [ ] Invalid clipboard data shows a ShowWarning error dialog
- [ ] Item right-click context menu shows "Export Item → Clipboard" option
- [ ] All new service logic has passing unit tests
