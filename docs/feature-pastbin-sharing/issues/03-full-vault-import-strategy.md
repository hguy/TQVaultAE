# Slice 3: Full Vault Export + Import Strategy

- **Type:** AFK
- **Blocked by:** Slice 1
- **User stories:** #6, #15, #16, #17, #18

## What to build

Add vault-level export/import (`scope: "vault"`) with the full-vault import strategy dialog.

When exporting a full vault, all 12 SackCollections plus the vault name are serialized. A button added to the right of the vault combo box (mirroring the existing player combo pattern) provides export options.

When importing a full vault, a dialog presents three choices:
- **Replace** — clear all 12 tabs in the target vault, then import with a data-loss warning (uses ShowWarning with OKCancel)
- **Create New Vault** — auto-generate a new vault file with a suffixed name on conflict, then import into it
- **Cancel** — abort

## Acceptance criteria

- [ ] Vault combo box area has a new button with context menu "Export Vault" → File, Clipboard
- [ ] Exporting a vault serializes the name and all 12 SackCollections in the Export Format
- [ ] Vault-scope import triggers a dialog with Replace / Create New Vault / Cancel
- [ ] Replace: if target vault is non-empty, ShowWarning dialog warns about data loss with OKCancel; if confirmed, clears all tabs before importing
- [ ] Replace: if target vault is empty, imports directly without confirmation
- [ ] Create New Vault: invokes VaultService.CreateVault with a unique name (suffixed on conflict) and imports into it
- [ ] Cancel: no action taken
- [ ] All new functionality has passing unit tests
