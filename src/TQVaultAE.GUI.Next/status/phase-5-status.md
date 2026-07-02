# Phase 5: Dialog Forms — Progress

> Design authority: `src/TQVaultAE.GUI.Next/MIGRATION_PLAN.md` § Phase 5
> Effort: M (2-3 weeks)
> Can run in parallel with Phase 4 (styling only, not rendering)

## 5.1 — IUIService Redesign

> ⚠️ This split happens in Phase 0 scope (before signature changes). Documented here for reference.

- [ ] `IUIService` — shared contract, event-free
- [ ] `IWinFormsUIService : IUIService` — adds events (lives in `TQVaultAE.GUI`)
- [ ] `IAvaloniaUIService : IUIService` — adds `StatusBarViewModel`
- [ ] Remove `NotifyUser(string, Color)`; keep `NotifyUser(string, TQColor)`

## 5.2 — Dialog Implementation

- [ ] `AboutBox`
- [ ] `BagButtonSettings`
- [ ] `CharacterEditDialog`
- [ ] `ItemProperties`
- [ ] `ItemSeedDialog`
- [ ] `ResultsDialog`
- [ ] `SearchDialogAdvanced`
- [ ] `SettingsDialog`
- [ ] `VaultMaintenanceDialog` ⚠️ Verify save-file integrity after every operation

## Testing Gate

- [ ] Each dialog ViewModel state transitions correctly (open → edit → save → close)
- [ ] Data binding round-trip verified for each dialog
- [ ] Localized strings resolve on both platforms

## Completed Items

| Date | Item |
|------|------|
|      |      |
