# Phase 8: Legacy Theme (WinForms Visual Parity) — Progress

> Design authority: `src/TQVaultAE.GUI.Next/MIGRATION_PLAN.md` § Phase 8
> Effort: M (1-2 weeks)
> Prerequisite: Phase 7 human validation complete
> Complete when: Legacy theme renders correctly, toggle works with 500+ items, snapshots pass, manual review approved

## 8.1 — Legacy Theme Resource Dictionary

- [ ] Create `Styles/LegacyTheme.axaml` — WinForms-matching colors, fonts, spacing, borders
- [ ] Register in `App.axaml` resource dictionaries
- [ ] Extend CI theme key check to BOTH dictionaries

## 8.2 — Control Styling

- [ ] SackPanel — WinForms-matching item slots, selection highlights, grid lines
- [ ] BagButtonBase / BagButtonSmall / BagButtonLarge — WinForms-matching appearance
- [ ] Tooltips — WinForms-matching background, font, border
- [ ] Item rendering — WinForms-matching icon sizing and text colorization

## 8.3 — Theme Toggle Polish

- [ ] Runtime theme toggle tested with 500+ visible items
- [ ] Loading overlay with `IProgress<T>` if toggle lags
- [ ] `SelectedTheme` persisted and applied on startup

## Testing Gate

- [ ] Legacy theme snapshots (100%/125%/150% DPI) pass pHash comparison
- [ ] Theme key completeness covers both dictionaries
- [ ] Theme toggle switches all controls without binding errors
- [ ] Visual comparison against WinForms screenshots — **manual review gate**
- [ ] Performance: theme toggle with 500+ items no visible lag

## Completed Items

| Date | Item |
|------|------|
|      |      |
