# Phase 1: Project References, DI & Platform Foundation — Progress

> Design authority: `src/TQVaultAE.GUI.Next/MIGRATION_PLAN.md` § Phase 1
> Effort: S (3-5 days)
> Complete when: CI passes on Windows + Linux, DI validates, ViewLocator resolves all ViewModels

## 1.1 — Project References

- [ ] Add project references to `TQVaultAE.GUI.Next.csproj`
- [ ] `OutputType`: `WinExe` → `Exe`
- [ ] Desktop csproj stays single `net10.0`

## 1.2 — DI Composition Root

- [ ] Add `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.Logging` NuGet
- [ ] Bind `IServiceProvider` to `AvaloniaLocator` BEFORE `AppBuilder.Configure`
- [ ] Platform-specific DI via `OperatingSystem.IsWindows()` runtime checks
- [ ] Extract `ServiceCollectionExtensions.cs`
- [ ] Remove static `ServiceProvider` from old `Program.cs`
- [ ] Pass `IServiceProvider` to `App` via `Initialize(IServiceProvider)`
- [ ] Resolve `MainViewModel` from DI
- [ ] Wire `log4net` + `Microsoft.Extensions.Logging`
- [ ] Wire global exception handling (`Dispatcher.UIThread`, `AppDomain.UnhandledException`)

## 1.3 — Platform-Specific Fixes

- [ ] Replace `EscapedCodeBase` → `AppContext.BaseDirectory`
- [ ] Refactor `UserSettings` to `Environment.SpecialFolder.ApplicationData`
- [ ] Audit async patterns (async void, fire-and-forget, sync-over-async)

## 1.4 — ViewLocator

- [ ] Replace reflection-based ViewLocator with explicit factory delegates
- [ ] Test: `ViewLocator` resolves every registered ViewModel

## Testing Gate

- [ ] DI validates without exceptions (`ValidateOnBuild = true`)
- [ ] Platform-specific services resolve on Windows and Linux
- [ ] ViewLocator resolves all ViewModels to non-null Views
- [ ] DI correctness: singletons same instance, transients different
- [ ] Build succeeds on Windows and Linux
- [ ] CI passes on `windows-latest` and `ubuntu-latest`

## Completed Items

| Date | Item |
|------|------|
|      |      |
