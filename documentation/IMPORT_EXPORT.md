# Import / Export & PasteBin Sharing

Share items, vault tabs, and entire vaults between players — via clipboard, `.json` files, or PasteBin URL links.

---

## Table of contents

* [Export Scopes](#export-scopes)
* [Export Channels](#export-channels)
    + [Clipboard Export (Ctrl+C)](#clipboard)
    + [File Export (.json)](#file-export)
    + [PasteBin Export](#pastebin-export)
* [Import Channels](#import-channels)
    + [Clipboard Import (Ctrl+V)](#clipboard-import)
    + [File Import (.json)](#file-import)
    + [PasteBin Import](#pastebin-import)
* [Full Vault Import Strategy](#vault-import)
* [Item Placement Logic](#placement)
* [Settings](#settings)
    + [Obtaining a PasteBin API Key](#api-key)

---

### <a id="export-scopes"></a>Export Scopes

You can export items at three levels:

| Scope | How to trigger | Behavior |
|-------|---------------|----------|
| **Single item** | Right-click an item → `Export Item` | Exports the selected item with all its properties (seed, affixes, relic sockets, stack size, position, etc.). Use **Ctrl+LeftClick** to multi-select items; exporting a selection exports all selected items as a tab-level payload. |
| **Vault tab** | Click the **tab button** (BagButton) → `Export Tab` | Exports all items within that tab, preserving their positions and grid layout. |
| **Full vault** | Click the **export button** next to the vault combo box → `Export Vault` | Exports all 12 tabs and all their contents, including the vault name. |

---

### <a id="export-channels"></a>Export Channels

Every scope supports three transport channels.

#### <a id="clipboard"></a>Clipboard Export

- **Single / multi-select items:** Select items in the grid and press **Ctrl+C**.
- **Tab:** Open the tab menu → `Export Tab` → `Copy to Clipboard`.
- **Vault:** Click the vault export button → `Export Vault to Clipboard`.

The export data is serialized as JSON, base64-encoded, and placed on your clipboard. You can paste this directly into a text editor, chat, or another TQVaultAE instance.

#### <a id="file-export"></a>File Export (.json)

- **Single item:** Right-click an item → `Export Item` → `Save to File...`.
- **Tab:** Tab menu → `Export Tab` → `Save to File...`.
- **Vault:** Vault export button → `Export Vault to File`.

A Save File dialog opens. The file is saved as **raw JSON** (not base64-encoded), so you can inspect or edit it in any text editor before importing.

#### <a id="pastebin-export"></a>PasteBin Export

- **Single item:** Right-click an item → `Export Item` → `Export to PasteBin`.
- **Tab:** Tab menu → `Export Tab` → `Export to PasteBin`.
- **Vault:** Vault export button → `Export Vault to PasteBin`.

The data is base64-encoded and uploaded to PasteBin as an **Unlisted** paste (accessible via direct URL but not published on the public feed). The resulting PasteBin URL is automatically copied to your clipboard.

**Note:** PasteBin export menu items are **disabled (greyed out)** until you configure a PasteBin API key in Settings (see [below](#api-key)).

---

### <a id="import-channels"></a>Import Channels

#### <a id="clipboard-import"></a>Clipboard Import (Ctrl+V)

Press **Ctrl+V** from the vault panel. TQVaultAE automatically detects what's on your clipboard:

- If the clipboard contains a **PasteBin URL** (starting with `https://pastebin.com/`), it fetches the paste data from PasteBin.
- Otherwise, it attempts to **base64-decode** and parse the clipboard contents as export data.

On success, items are imported into the currently loaded vault. On failure, a clear error message is shown.

#### <a id="file-import"></a>File Import (.json)

Use this dialog accessible from the vault panel's import button. A file picker dialog lets you select a `.json` export file. The file is parsed and imported into the current vault.

#### <a id="pastebin-import"></a>PasteBin Import

**No API key is required** to import from a PasteBin link. Simply paste a PasteBin URL into your clipboard and press **Ctrl+V**, or use the file import dialog. TQVaultAE fetches the raw paste content, decodes it, and imports the items.

---

### <a id="vault-import"></a>Full Vault Import Strategy

When importing a **full vault**, a dialog presents three options:

| Option | Behavior |
|--------|----------|
| **Replace** (Yes) | Clears all 12 tabs in the current vault and imports the source data. If the target vault is **not empty**, you are shown a second confirmation warning: *"WARNING: This will erase all items in the current vault. Continue?"* |
| **Create New Vault** (No) | Creates a new vault file with the imported vault's name. If a vault with that name already exists, a numeric suffix is appended (e.g., `MyVault (2)`). The new vault is automatically loaded. |
| **Cancel** | Aborts the import. Nothing is changed. |

There is no partial merge of tabs into an existing vault — it's all-or-nothing.

---

### <a id="placement"></a>Item Placement Logic

When importing items into a target vault tab, TQVaultAE attempts placement in this order:

1. Try the item's **original position** (PositionX, PositionY from the export).
2. If the original cell is occupied or out of bounds, search for the **first available open cell** in the 18×20 grid.
3. If no open cell exists in that tab, the item is **skipped**.

After all items are processed, a result message is shown: *"Imported N of M items. X items could not be placed (no space)."*

---

### <a id="settings"></a>Settings

PasteBin settings are configured in **Settings** (gear icon in the toolbar):

| Setting | Description | Default |
|---------|-------------|---------|
| **PasteBin API Key** | Your PasteBin developer API key. Required to export pastes. | _(empty)_ |
| **PasteBin Expiration** | How long before the paste automatically expires. Options: 10 Minutes, 1 Hour, 1 Day, 1 Week, 2 Weeks, 1 Month, 6 Months, 1 Year, Never. | 1 Month |

#### <a id="api-key"></a>Obtaining a PasteBin API Key

1. Go to **[https://pastebin.com/doc_api](https://pastebin.com/doc_api)**.
2. Sign in or create a free PasteBin account.
3. Under the **"Your Unique Developer API Key"** section, copy the key shown.
4. Paste the key into TQVaultAE's **Settings → PasteBin API Key** field.

That's it — PasteBin export menu items will now be enabled.

---

**Version:** 1.0 — Feature implemented as per [Issue #29](https://github.com/hguy/TQVaultAE/issues/29), Slice 5.