# Phase 4: Controls — Progress

> Design authority: `src/TQVaultAE.GUI.Next/MIGRATION_PLAN.md` § Phase 4
> Effort: XL (4-6 weeks, SackPanel ~40%)
> Complete when: All controls render in both themes, SackPanel works with 500+ items, snapshots pass, DPI verified

## Priority ladder (strict order)
1. Compose from standard controls
2. Style existing controls
3. Custom `TemplatedControl` subclass only as last resort

## Controls

- [ ] Audit all 21+ WinForms controls against standard Avalonia equivalents
- [ ] **SackPanel** — `UniformGrid` + `ItemsRepeater` + `ScrollViewer` (START FIRST)
- [ ] **BagButtonBase / BagButtonSmall / BagButtonLarge** — styled `ToggleButton` + `ControlTemplate`
- [ ] **ScalingLabel** — `TextBlock` inside `Viewbox`
- [ ] **Tooltip controls (6 files)** — `ToolTip` attached property
- [ ] **SackPanel item rendering** — `Image` + `TextBlock` in `DataTemplate`
- [ ] Register all control styles in both theme dictionaries
- [ ] Enforce `AvaloniaUseCompiledBindingsByDefault` (all XAML bindings must compile)

## Custom GDI+ → Avalonia mapping

- [ ] `DrawString` → `Control.Render(DrawingContext)` / `TextBlock`
- [ ] `FillRectangle` → Composite XAML templates
- [ ] `DrawImage` → `Image` + `Border` + `TextBlock` composition
- [ ] Design done BEFORE implementation (depends on Phase 3.1 Paint Handler Audit)

## Testing Gate

- [ ] Each control renders in both themes without binding errors
- [ ] `Viewbox` + `TextBlock` sharp at 100%/125%/150% DPI (pHash comparison)
- [ ] Item `DataTemplate` renders correct icon, text, colors
- [ ] Snapshot baseline PNGs generated (100%/125%/150% DPI, both themes)
- [ ] CI snapshot comparison with pHash Hamming distance ≤5

## Completed Items

| Date | Item |
|------|------|
|      |      |
