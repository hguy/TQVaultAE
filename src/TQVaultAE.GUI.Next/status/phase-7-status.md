# Phase 7: Polish & Performance — Progress

> Design authority: `src/TQVaultAE.GUI.Next/MIGRATION_PLAN.md` § Phase 7
> Effort: L (2-3 weeks)
> All prior phase gates are headless (`Avalonia.Headless.XUnit.v3`). E2E window-launch deferred here.

## 7.0 — Polish Tasks

- [ ] Keyboard shortcuts and navigation — keyboard-only navigation for all workflows
- [ ] `AutomationProperties.Name` on every `Button`/`MenuItem`/`TextBox`
- [ ] Color contrast verifier (WCAG AA thresholds)
- [ ] Drag-and-drop between sacks/bags
- [ ] Clipboard integration
- [ ] Sound effects — verify Windows, graceful no-op on Linux
- [ ] Theme toggle — both themes test all controls
- [ ] Theme switch performance — test with 500+ items; add loading overlay if laggy

## 7.1 — Performance

- [ ] Add `BenchmarkDotNet`
- [ ] Benchmarks: item loading, search filtering, sack rendering, vault open (cold/warm)
- [ ] Performance budget: 500-item vault <2s (absolute cap)
- [ ] Stress: transfer 500 items vault↔player; open 10 vaults sequentially (no memory leak)
- [ ] Baseline JSON stored in `test/BenchmarkBaselines/{platform}/`
- [ ] First run: store baseline, always pass
- [ ] Subsequent: gate fails if `Mean` regresses >15% vs baseline
- [ ] Gate: whichever is stricter — absolute 2s cap OR 15% regression
- [ ] Profile and optimize both platforms

## 7.2 — Code Coverage

- [ ] Coverage thresholds per Phase 0.4 `<CoverletThreshold>` configuration

## Testing Gate

- [ ] Visual regression snapshots match (Default and Legacy)
- [ ] Performance benchmarks pass both platforms
- [ ] Accessibility audit (Tab/Enter/Space/Escape) via headless key simulation
- [ ] Regression: all 30-50 workflows from Phase 0.0 pass on both platforms

## Completed Items

| Date | Item |
|------|------|
|      |      |
