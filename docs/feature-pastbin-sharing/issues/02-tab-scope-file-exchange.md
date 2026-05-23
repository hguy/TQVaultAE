# Slice 2: Tab Scope + File Export/Import

- **Type:** AFK
- **Blocked by:** Slice 1
- **User stories:** #2, #3, #5, #8

## What to build

Extend the export/import system to handle tab-level scope (`scope: "tab"`) and add file-based transport (.json files via save/open dialogs).

When multiple items are selected, the export produces a tab-scope payload. The BagButton (tab button) context menu gets export items. File save/open dialogs allow exporting/importing as raw JSON (not base64-encoded).

Multi-select items export as a tab-level payload. BagButton context menu includes Export to File and Export to Clipboard.

## Acceptance criteria

- [ ] Selecting multiple items and pressing Ctrl+C exports a tab-scope payload with all selected items
- [ ] BagButton context menu has "Export Tab" with sub-items for File and Clipboard
- [ ] Item right-click context menu has "Export Item" with sub-items for File and Clipboard
- [ ] File save dialog writes raw JSON (not base64) for exported items/tabs
- [ ] File open dialog reads raw JSON and imports successfully
- [ ] Importing from file validates the Export Format envelope and rejects invalid data with ShowWarning
- [ ] Tab-scope import places each item using Import Placement Logic
- [ ] Tab-scope serialization preserves BagButtonIconInfo and SackType
- [ ] All new functionality has passing unit tests
