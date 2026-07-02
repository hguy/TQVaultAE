# Phase 0: Prerequisites & Architecture Decisions — Progress

> Design authority: `src/TQVaultAE.GUI.Next/MIGRATION_PLAN.md` § Phase 0
> Effort: M (1-2 weeks)

## 0.0 — Behavioral Regression Inventory

- [ ] Enumerate 30-50 key user workflows from existing WinForms app
- [ ] Create traceable checklist mapping workflows to headless integration tests (`test/Regression/`)
- [ ] Each test passes on BOTH platforms before Phase 7 sign-off

## 0.1 — System.Drawing Elimination

### IUIService
- [ ] `DefaultBitmap` → `TextureData`
- [ ] `GetBitmap(Item)` → `TextureData`
- [ ] `LoadBitmap(RecordId)` (3 overloads) → `TextureData`
- [ ] `LoadRelicOverlayBitmap()` → `TextureData`
- [ ] Keep `NotifyUser(string, TQColor)` as-is
- [ ] Remove `NotifyUser(string, Color)` overload
- [ ] Replace `NotifyUserEvent` / `ShowMessageUserEvent` with `StatusBarViewModel` (see Phase 5.1)

### IBitmapService
- [ ] `LoadFromTexMemory()` → `(int w, int h, byte[] rgba)` tuple
- [ ] `ResizeImage()` → `TextureData` with `ResizeQuality` enum + `maintainAspectRatio`

### IAddFontToOS
- [ ] Not registered in Avalonia DI graph

### IFontService
- [ ] Move to `TQVaultAE.Presentation` (from Application contracts layer)
- [ ] Redesign: return `SKTypeface` (~4 overloads)

### Additional cleanup
- [ ] Remove `LicenseManager.UsageMode` checks from `FontService`, `UIService`
- [ ] Replace `UIService.CreateDefaultBitmap()` with embedded PNG
- [ ] Split `Resources.resx` (2,177 lines): strings / fonts (keep `.resx` byte[]) / images
- [ ] Audit `Resources.AlbertusMT`, `Resources.AlbertusMTLight` callers before splitting

## 0.1a — Font Self-Sufficiency

- [ ] Add Liberation Sans, Liberation Serif, DejaVu Sans `.ttf` to `Resources.resx`
- [ ] Add `LiberationSans`, `DejaVuSans`, `LiberationSerif` to `FontFamilyList` enum
- [ ] Rename old `Arial`/`Verdana`/`TimesNewRoman` concept callers
- [ ] Implement `EmbeddedResourceFontCollection` (custom `IFontCollection`) for FontManager registration
- [ ] Add Albertus MT entry to `THIRD_PARTY_NOTICES.md`
- [ ] Remove `System.Drawing.Common` from `Presentation.csproj`
- [ ] Move WinForms font compat shim to `TQVaultAE.GUI`

## 0.2 — Image Pipeline & Caching

- [ ] Define `TextureData` struct in `TQVaultAE.Domain`
- [ ] Update `IUIService.GetBitmap()` / `LoadBitmap()` / `LoadRelicOverlayBitmap()` / `DefaultBitmap` → `TextureData`
- [ ] Update `_Bitmaps` cache to `LazyConcurrentDictionary<RecordId, TextureData>`
- [ ] Update `AddBitmap()` for dimensions from `LoadFromTexMemory()`
- [ ] Define `IBitmapFactory<TNative>` in `TQVaultAE.Application/Contracts/Services/`
- [ ] Update `ResizeImage()` signature: `(byte[], int, int, int, int, ResizeQuality, bool)` → `TextureData`
- [ ] `LoadRelicOverlayBitmap` sentinel comparison uses reference equality on `TextureData`
- [ ] Audit Magick.NET DDS format support (DXT1/DXT3/DXT5/uncompressed)
- [ ] Interface split early: `IUIService` / `IWinFormsUIService` / `IAvaloniaUIService`
- [ ] Remove `System.Drawing.Common` from Presentation; move compat code to `TQVaultAE.GUI`
- [ ] Build WinForms `TQVaultAE.GUI` in CI after contract changes

## 0.3 — Linux Service Stubs

- [ ] Create `TQVaultAE.Services.Linux` project
- [ ] Implement `SoundServiceLinux` (OpenTK.OpenAL + MP3Sharp)
- [ ] Implement `GamePathServiceLinux` (Proton prefix + XDG paths)
- [ ] Implement `GameFileServiceLinux` (POSIX symlinks)

## 0.3a — Localization Strategy

- [ ] Verify `Thread.CurrentUICulture` switching resolves all 800+ resource keys (EN/FR/DE/RU)
- [ ] Verify `IStringLocalizer<T>` resolves for each ViewModel type

## 0.4 — Test Project & CI

- [ ] Create `src/TQVaultAE.GUI.Next.Tests/` project
- [ ] Audit `Directory.Packages.props` (CPM scope — dual-scope conflict)
- [ ] Add `Avalonia.Headless.XUnit.v3`, `Avalonia.Headless` to packages
- [ ] Configure headless `AppBuilder`
- [ ] Add Linux CI apt dependencies
- [ ] Create `ViewModelTestBase` fixture
- [ ] Create `TestDataFactory` / `ViewModelBuilder`
- [ ] Add `ubuntu-latest` to CI matrix
- [ ] Add Linux CI job with `AVALONIA_HEADLESS` env var
- [ ] Create `TQVaultAE-CrossPlatform.slnf`
- [ ] Add `ValidateOnBuild = true`, `ValidateScopes = true`
- [ ] Measure coverage baseline: `Services = ___%`, `Data = ___%`, `Domain = ___%`
- [ ] Configure per-project `<CoverletThreshold>`
- [ ] Configure coverlet exclusions (`*.g.cs`, `*.axaml.cs`)
- [ ] Add coverage collection to CI
- [ ] Add `BindingErrorAssertionHelper`

## 0.4a — Test Fixture Data

- [ ] Create `test/Fixtures/` with player, vault, stash, and DDS fixtures
- [ ] Create `FixtureManager` class
- [ ] Commit fixtures to git
- [ ] Verify DDS fixtures decode on both platforms

## Completed Items

| Date | Item |
|------|------|
|      |      |
