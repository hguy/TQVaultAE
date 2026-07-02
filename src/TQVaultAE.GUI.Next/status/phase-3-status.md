# Phase 3: Main Window Shell — Progress

> Design authority: `src/TQVaultAE.GUI.Next/MIGRATION_PLAN.md` § Phase 3
> Effort: L (3-4 weeks)
> Complete when: All sub-ViewModels resolve, shell renders, menu/toolbar dispatch, integration scenarios pass
> ⚠️ Decompose further if any ViewModel exceeds ~500 lines

## 3.1 — Paint Handler Audit

- [ ] Audit `MainForm` for WinForms `Paint` event handlers
- [ ] Output feeds Phase 4 Controls rendering design

## 3.2 — ViewModel Decomposition

- [ ] `MainViewModel` (shell, navigation, DI orchestration)
- [ ] `PanelLayoutViewModel` (splitter positions, panel visibility)
- [ ] `PlayerPanelViewModel` (player list, character selection)
- [ ] `VaultPanelViewModel` (vault/stash inventory grid)
- [ ] `StashPanelViewModel` (stash management)
- [ ] `SearchViewModel` (search bar, results)
- [ ] `ForgeViewModel` (artifact forging)
- [ ] `ClipboardViewModel` (copy/paste operations)
- [ ] `ExportViewModel`
- [ ] `LoadingViewModel` (save/load orchestration)
- [ ] `MainForm.Lifecycle` → Avalonia window events (`OnLoaded`, `OnClosing`)

## Layout

- [ ] Design `MainView.axaml` (menu bar, toolbars, panels, status bar)
- [ ] Port menu structure and toolbar
- [ ] Port status bar — `IUIService` events → `INotifyPropertyChanged` on `StatusBarViewModel`

## Testing Gate

- [ ] Each sub-ViewModel resolves from DI and initializes
- [ ] Shell layout creates expected visual tree (E2E deferred to Phase 7)
- [ ] Menu and toolbar commands dispatch to correct ViewModels
- [ ] Player selection → `VaultPanelViewModel` refreshes
- [ ] Bag click dispatches item transfer to correct destination
- [ ] Search → results display correctly
- [ ] Drag from search results to stash completes
- [ ] Forge artifact workflow completes end-to-end

## Completed Items

| Date | Item |
|------|------|
|      |      |
