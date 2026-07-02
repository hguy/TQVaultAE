# Phase 2: Startup, Configuration & Theming — Progress

> Design authority: `src/TQVaultAE.GUI.Next/MIGRATION_PLAN.md` § Phase 2
> Effort: S (3-5 days)
> Complete when: App starts with correct theme on both platforms, UserSettings round-trips, theme toggle works

## 2.1 — Entry Point

- [ ] Port entry point flow (database init, logging, splash)
- [ ] Port `UserSettings` + add `SelectedTheme` property
- [ ] Create Avalonia `SplashScreenWindow` (async init with `IProgress<T>`)
- [ ] Wire `IGamePathService` and validate game path
- [ ] Initialize database on startup

## 2.2 — Theming Infrastructure

- [ ] Define `Theme.*` and `Control.*` resource keys in `Styles/ThemeResources.axaml`
- [ ] Define `Theme.DefaultFontFamily` (Inter for Default, Liberation Sans → Albertus MT for Legacy)
- [ ] Create `Styles/LegacyTheme.axaml`
- [ ] Create `Styles/DefaultTheme.axaml`
- [ ] Extract `StreamGeometry` icons into `Styles/Icons.axaml`
- [ ] Wire theme selection from `UserSettings.SelectedTheme`
- [ ] Add runtime theme toggle in `MainViewModel`
- [ ] Implement `IStringLocalizer<T>` localization
- [ ] Create CI check for theme key completeness

## Testing Gate

- [ ] App starts (headless `AppBuilder` succeeds without exceptions)
- [ ] `UserSettings` round-trips on both platforms
- [ ] Theme resource key verification in CI
- [ ] Theme toggle switches all controls without binding errors
- [ ] E2E window-launch verification deferred to Phase 7

## Completed Items

| Date | Item |
|------|------|
|      |      |
