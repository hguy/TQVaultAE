# TQVaultAE Avalonia Migration Plan

## Status: EARLY SCAFFOLDING

The Avalonia project skeleton exists with stub views. No real application logic, DI wiring, or UI controls have been ported. The old WinForms `TQVaultAE.GUI` remains the active, production application.

> **Plan Maintenance:** Check completed items with `[x]`. Add completion dates. Update dependency diagram if phases are reordered. Review this plan at the end of each phase.

> **Progress Tracking:** Per-phase status files live in `status/phase-N-status.md`. These mirror the checklist items here with completion dates. Design authority stays in this file.

> **Quick Start (Phase 0 Step 0):** 1. Create `TQVaultAE.GUI.Next.Tests` project and verify it compiles. 2. Run existing `TQVaultAE.Tests` to establish coverage baseline. 3. Begin Phase 0.1 by replacing `System.Drawing.Bitmap` → `TextureData` in `IUIService`. Commit after each interface change.

> **Effort Estimates:** Phase 0: M (1-2 weeks) | Phase 1: S (3-5 days) | Phase 2: S (3-5 days) | Phase 3: L (3-4 weeks) | Phase 4: XL (4-6 weeks, SackPanel ~40%) | Phase 5: M (2-3 weeks) | Phase 6: M (1-2 weeks) | Phase 7: L (2-3 weeks) | Phase 8: M (1-2 weeks)

---

## Scope & Boundaries

| Area | In Scope | Out of Scope |
|------|----------|--------------|
| **Main app** | `TQVaultAE.GUI` → `TQVaultAE.GUI.Next` | — |
| **Companion tools** | — | `ARZExplorer` and `TQSaveFilesExplorer` (remain WinForms; not migrated) |
| **Platforms** | Windows + Linux | macOS, mobile, WebAssembly |
| **Themes** | Default (Avalonia Fluent) + Legacy (WinForms look) | Additional custom themes |

---

## Cross-Cutting Goals

### 1. Cross-Platform: Windows + Linux

The Avalonia project MUST run on both Windows and Linux. The existing service layer already has platform abstractions (`IFileIO`, `IPathIO`). Any Win32-specific code currently in `TQVaultAE.Services.Win32` must be gated behind interfaces with cross-platform implementations. The `TQVaultAE.Services.Win32` project targets `net10.0-windows` with `<UseWindowsForms>true` — it **cannot** be referenced directly by the cross-platform `GUI.Next` project. Instead, use runtime `OperatingSystem.IsWindows()` checks for DI registration (simpler build, no multi-targeting overhead).

**Linux service stubs required** (all live in a new `TQVaultAE.Services.Linux` project, mirroring the existing `TQVaultAE.Services.Win32` structure):
- `SoundServiceLinux` — OpenTK (`OpenTK.Audio.OpenAL`) for playback + MP3Sharp (pure .NET MP3 decoder) for MP3 → WAV/WAV-stream conversion; both are NuGet packages with zero native dependencies. Must implement the full `ISoundService` interface: `PlaySound(RecordId)`, `InitAllPlayers()`, `SetSoundResource(RecordId, byte[])`, `GetSoundResource(RecordId)`, `PlayRandomItemDrop()`, `PlayRandomRelicDrop()`, `PlayRandomVoice()`, `PlayRandomCancel()`, `PlayRandomMetalHit()`, `PlayLevelUp()`, `ConvertMp3ToWav(Stream, Stream)`.
- `GamePathServiceLinux` — Implements all methods in `IGamePathService` (~234-line interface): `ResolveGamePath()`, `ResolveTQVaultPath()`, `DetectGameType()` (Proton-specific executable detection), all path properties. XDG paths + manual-only entry.
- `GameFileServiceLinux` — POSIX `ln -s` for symlinks instead of `MKLINK`, no CMD/fsutil dependency

Each stub must implement the existing interface so the DI container resolves on Linux without errors. `IAddFontToOS` is obsolete for Avalonia — SkiaSharp loads fonts from memory directly, no OS font installation needed on any platform.

### 2. UI Theming — Default First, Legacy Later

| Theme | Description | Timing |
|-------|-------------|--------|
| **Default** | Vanilla Avalonia look and feel — clean, modern, using Avalonia's built-in theme system (Fluent) | Phases 2–7 |
| **Legacy** | Visual match to the current WinForms UI — colors, borders, fonts, spacing, control styling that replicates the original TQVaultAE appearance | Phase 8 (dedicated, after human validation) |

**Rationale:** Ship the working Avalonia app first with the Default theme. Once the tool is validated by human testing, add the Legacy theme in a dedicated phase. This de-risks the schedule by separating visual parity from functional parity.

Implementation approach:
- Use Avalonia's `ThemeVariant` / resource dictionary system — never hard-code colors, brushes, or dimensions
- Define all visual properties as `StaticResource` or `DynamicResource` lookups in `Styles/`
- **Resource key naming convention:**
  - `Theme.*` for semantic roles: `Theme.AccentBrush`, `Theme.ControlBackgroundBrush`, `Theme.ControlForegroundBrush`
  - `Control.*` for control-specific: `Control.BagButton.SmallWidth`, `Control.SackPanel.ItemMargin`
- Theme switching (Default ↔ Legacy) is a runtime toggle via `RequestedThemeVariant` on the `App` or `Window` level, with user preference persisted in `UserSettings.SelectedTheme`
- Extract inline `StreamGeometry` icons from `App.axaml` into `Styles/Icons.axaml` resource dictionary
- The `Styles/LegacyTheme.axaml` resource dictionary is added in Phase 8

### 3. AI Agents: Best Practices First, One-to-One Port Last

AI agents working on this migration MUST follow this priority:

1. **Avalonia/WPF/XAML best practices first** — before writing any code, research (websearch if needed) the idiomatic Avalonia approach for the feature being implemented. Prefer MVVM data binding, `INotifyPropertyChanged` / CommunityToolkit source generators, `DataTemplates`, `ControlTemplates`, styles, attached properties, and composition over code-behind.
2. **Adapt don't transplant** — when porting a WinForms control, design an Avalonia-native equivalent using the framework's primitives. Do NOT recreate the WinForms event model or Paint handler logic inside Avalonia unless absolutely necessary.
3. **One-to-one conversion is the last resort** — a direct pixel-for-pixel port is acceptable only when there is no idiomatic Avalonia equivalent and a custom control from scratch would be impractical. When this happens, document the reason in a `/// <summary>` on the control.

---

## Architecture

### Layer Dependency Diagram

```
GUI → Presentation → Services → Domain → Data
     ↓
Application (DTOs, Results, Search)
```

### Target Framework

`net10.0` (cross-platform), Avalonia 12.0.4, CommunityToolkit.Mvvm 8.4.0

| Component | Version | Notes |
|-----------|---------|-------|
| .NET SDK | 10.0.100+ | Verify with `dotnet --version` |
| Avalonia | 12.0.4 | Fluent theme included via NuGet |
| CommunityToolkit.Mvvm | 8.4.0 | Source generators for `[ObservableProperty]`, `[RelayCommand]` |
| SkiaSharp | via Avalonia.Skia | Hardware-accelerated rendering |
| Magick.NET | Q8-AnyCPU | DDS/TGA decoding (already in Presentation) |

### Migration Strategy

The WinForms `TQVaultAE.GUI` and Avalonia `TQVaultAE.GUI.Next` coexist during migration. Both UIs share the same service layer and consume the same `TextureData` cache. WinForms GUI builds alongside the new project until all critical workflows are verified in Avalonia. The switch to Avalonia as default happens when Phase 7 testing passes on both platforms. Rollback: if Avalonia has a critical post-release bug, users run the old WinForms binary.

### Projects (Current + Target)

| Project | Status | Purpose |
|---------|--------|---------|
| `TQVaultAE.GUI` | **Active (old)** | Windows Forms UI — being migrated FROM |
| `TQVaultAE.GUI.Next` | **In progress** | Avalonia UI — migration target |
| `TQVaultAE.GUI.Next.Tests` | **Not yet created** | Avalonia ViewModel/control/integration tests |
| `TQVaultAE.Services` | Complete | Business logic |
| `TQVaultAE.Application` | Complete | DTOs, Results, Contracts |
| `TQVaultAE.Data` | Complete | Data access |
| `TQVaultAE.Domain` | Complete | Entities |
| `TQVaultAE.Presentation` | Complete | Shared presentation services |
| `TQVaultAE.Config` | Complete | UserSettings, TagConfig |
| `TQVaultAE.Logs` | Complete | Logging |
| `TQVaultAE.Services.Win32` | Complete | Win32-specific services (cannot be referenced by cross-platform project) |
| `TQVaultAE.Services.Linux` | **Not yet created** | Linux-specific services — `SoundServiceLinux`, `GamePathServiceLinux`, `GameFileServiceLinux` |
| `TQVaultAE.Tests` | Complete | xUnit tests (service/domain/data layer) |

---

## Phase 0: Prerequisites & Architecture Decisions

**Goal:** Resolve blocking cross-platform decisions before any code is written.

### 0.0 — Behavioral Regression Inventory

Before modifying any code, catalog the behaviors the migration must preserve:

- [ ] Enumerate 30-50 key user workflows from the existing WinForms app:
  - Open vault → browse items → mouse-over tooltips display correctly
  - Move item between bags within a character
  - Transfer item from vault to player character
  - Search by name → results filtered correctly
  - Craft artifact via forge
  - Sort vault by name/level/type
  - Export vault to text file
  - Save/reload vault → all items in same positions
  - Double-click item → Item Properties dialog
  - Right-click context menu actions (copy, delete, move to)
- [ ] Create traceable checklist mapping each workflow to a headless integration test (`test/Regression/`)
- [ ] Each test must pass on BOTH platforms before Phase 7 sign-off

### 0.1 — System.Drawing Elimination

`System.Drawing` leaks through service contracts and must be replaced before DI wiring. **Every affected method/property is listed:**

#### IUIService — 6 leak points

| Member | Current Signature | Replacement |
|--------|------------------|-------------|
| `DefaultBitmap` (property) | Returns `System.Drawing.Bitmap` | Returns `TextureData` |
| `GetBitmap(Item)` | Returns `System.Drawing.Bitmap` | Returns `TextureData` |
| `LoadBitmap(RecordId)` (3 overloads) | Returns `System.Drawing.Bitmap` | Returns `TextureData` |
| `LoadRelicOverlayBitmap()` | Returns `System.Drawing.Bitmap` | Returns `TextureData` |
| `NotifyUser(string, TQColor)` | Takes `TQColor` enum | **Keep as-is** — already platform-neutral (exists at `src/TQVaultAE.Domain/Enums/TQColor.cs`) |
| `NotifyUser(string, Color)` | Takes `System.Drawing.Color` | **Remove this overload.** Callers use `NotifyUser(string, TQColor)` instead. |
| `NotifyUserEvent` / `ShowMessageUserEvent` | .NET events with WinForms delegate types | Replaced by `INotifyPropertyChanged` on `StatusBarViewModel` (see Phase 5.1) |

#### IBitmapService — 2 leak points

| Member | Current Signature | Replacement |
|--------|------------------|-------------|
| `LoadFromTexMemory(byte[], int, int)` | Returns `System.Drawing.Bitmap` | Returns `(int width, int height, byte[] rgbaPixels)` tuple — dimensions required for `TextureData` construction |
| `ResizeImage(Image, int, int, InterpolationMode, bool)` | Returns `System.Drawing.Image`, takes `System.Drawing.Drawing2D.InterpolationMode` + `bool maintainAspectRatio` | Replace `InterpolationMode` with platform-neutral `ResizeQuality` enum (`Low|Medium|High|NearestNeighbor`); return `TextureData` (see Phase 0.2 for full signature). Keep `maintainAspectRatio` parameter — it centers images for TQ item rendering with varying aspect ratios. |

#### IAddFontToOS

| Member | Current Signature | Replacement |
|--------|------------------|-------------|
| `AddFontToOS(string, byte[])` | Returns `System.Drawing.FontFamily` | **Unused in Avalonia** — SkiaSharp loads fonts from memory directly. Not registered in DI. |

#### IFontService — 12 overloads

| Member | Current Return | Replacement |
|--------|---------------|-------------|
| `GetFont(...)` (6 overloads) | Returns `System.Drawing.Font` | See Phase 0.1a redesign — returns `SKTypeface` |
| `GetFontLight(...)` (6 overloads) | Returns `System.Drawing.Font` | See Phase 0.1a redesign — returns `SKTypeface` |
| `FontLoader` (property) | Exposes `IAddFontToOS` | Removed — Avalonia DI tree does not register `IAddFontToOS` |

See Phase 0.1a for `IFontService` redesign.

#### Additional cleanup

- [ ] Remove all `LicenseManager.UsageMode` checks from `FontService` and `UIService` — WinForms designer guards are dead weight in Avalonia; use `#if DEBUG` Avalonia previewer detection if needed
- [ ] Replace `UIService.CreateDefaultBitmap()` — currently renders an orange question-mark bitmap via `System.Drawing.Graphics.DrawString`. Replace with an embedded PNG resource (`Resources/DefaultItemIcon.png`)
- [ ] Split monolithic `Resources.resx` (2,177 lines) into:
  - String resources → keep in `.resx` for `IStringLocalizer`
  - Font `.ttf` files → keep in `.resx` as `byte[]` (single embedding path)
  - Image assets → `Assets/Images/` with `<AvaloniaResource>` build action in `TQVaultAE.GUI.Next.csproj`
- [ ] Audit all callers of `Resources.AlbertusMT`, `Resources.AlbertusMTLight` before splitting

### 0.1a — Font Self-Sufficiency

**Goal:** Zero OS font dependencies. All fonts load from embedded resources — no `apt install` required on Linux, no font fallback to the OS.

**Embedded fonts (already in Resources.resx):**

| Font | Resource Name | Embedded `.ttf` | Size |
|------|--------------|-----------------|------|
| Albertus MT | `Resources.AlbertusMT` | `albertus mt.ttf` | 26 KB |
| Albertus MT Light | `Resources.AlbertusMTLight` | `albertus mt light.ttf` | 39 KB |

**Fonts to embed (free metric-compatible replacements):**

| System Font | Free Replacement | License | Resource Name | `.ttf` File |
|------------|-----------------|---------|---------------|-------------|
| Arial | **Liberation Sans** (metric-compatible — same glyph widths) | SIL Open Font | `Resources.LiberationSans` | `LiberationSans-Regular.ttf` |
| Times New Roman | **Liberation Serif** (metric-compatible) | SIL Open Font | `Resources.LiberationSerif` | `LiberationSerif-Regular.ttf` |
| Verdana | **DejaVu Sans** (visually similar, widely used on Linux) | Free license | `Resources.DejaVuSans` | `DejaVuSans.ttf` |

All `.ttf` files go into `src/TQVaultAE.Presentation/Resources/Fonts/` and are registered in `Resources.resx` as `byte[]` (same pattern as Albertus MT).

**Updated `FontFamilyList` enum:**

| Enum Value | Resource | Description |
|-----------|----------|-------------|
| `AlbertusMT` | `Resources.AlbertusMT` + `Resources.AlbertusMTLight` | Default — embedded proprietary `.ttf` |
| `LiberationSans` | `Resources.LiberationSans` | Arial-compatible free replacement |
| `DejaVuSans` | `Resources.DejaVuSans` | Verdana-like free replacement |
| `LiberationSerif` | `Resources.LiberationSerif` | Times New Roman-compatible free replacement |

- [ ] Add `LiberationSans`, `DejaVuSans`, `LiberationSerif` members to `FontFamilyList` enum. Rename existing callers that used the old `Arial`/`Verdana`/`TimesNewRoman` concept names.
- [ ] Create `EmbeddedResourceFontCollection` custom `IFontCollection` implementation that wraps `SKTypeface.FromData()` for `FontManager.Current.AddFontCollection()`

**Migration approach for Avalonia:**
- Fonts are embedded as `.resx` `byte[]` only. At startup, register embedded fonts with Avalonia's `FontManager` via a custom `IFontCollection` implementation that reads `.resx` font bytes through `SKTypeface.FromData()`:
  ```csharp
  var fontBytes = Resources.AlbertusMT; // .resx byte[]
  var typeface = SKTypeface.FromData(fontBytes);
  FontManager.Current.AddFontCollection(new EmbeddedResourceFontCollection(typeface));
  ```
  `EmbeddedResourceFontCollection` is a custom `IFontCollection` implementation — wraps `SKTypeface.FromData()` output. No `avares://` needed. No `.resx` duplication. Makes Albertus MT, Liberation Sans, Liberation Serif, and DejaVu Sans available to both custom `Render(DrawingContext)` and XAML `TextBlock.FontFamily` bindings.
- **Albertus MT kept** — same proprietary `.ttf` loaded via SkiaSharp instead of Win32. No visual change. Add entry to `THIRD_PARTY_NOTICES.md` documenting the Monotype license.
- **`IFontService` refactored to be platform-neutral** — new contract returns `SKTypeface` (from `SKTypeface.FromData(fontBytes)`). **Move the interface to `TQVaultAE.Presentation`** (where `FontService.cs` and `FontFamilyList.cs` already live) to avoid SkiaSharp leaking into the `TQVaultAE.Application` contracts layer. For XAML-side font usage, fonts registered via `FontManager.Current.AddFontCollection()` are resolved by name.
- **Neutral contract trims to ~4 overloads** (size-only, size+style, size+style+unit, light-variant). `GdiCharSet` and `GraphicsUnit` stay in the WinForms compat shim only — NOT in the neutral interface.
  - **WinForms compat shim:** lives in `TQVaultAE.GUI` (not `Presentation`), wraps the new `IFontService`, produces `System.Drawing.Font` on demand. Maps `GraphicsUnit` → points internally. `GdiCharSet` overload logs a warning and forwards without the charset byte.
- **`Avalonia.Fonts.Inter` kept for Default theme** — clean, modern look. Liberation Sans + Albertus MT are available for Legacy theme in Phase 8.
- `IAddFontToOS` interface removed entirely — no OS font installation on any platform
- `System.Drawing.Common` **removed** from `Presentation.csproj` — all `System.Drawing` compat code moves to `TQVaultAE.GUI` compat shim

### 0.2 — Image Pipeline & Caching Design

Design the new bitmap pipeline before Phase 1 DI wiring:

**Data flow:**
```
ArcFile/Database → TEX byte[] → (BitmapService decodes DDS/TGA) → RGBA byte[] → (UI layer) → Avalonia Bitmap
```

**Layer responsibilities:**
| Layer | Responsibility |
|-------|---------------|
| `IBitmapService` | Load raw TEX data from ArcFile/DB, decode DDS/TGA → RGBA `byte[]`. Return `(int width, int height, byte[] rgbaPixels)` tuple. Returns `byte[]` (not `System.Drawing.Bitmap`). The DDS decoding pipeline (`Magick.NET → raw RGBA`) is extracted once — both UIs share the same decoded pixel buffer. |
| `IUIService.GetBitmap()` | Calls `IBitmapService`, returns a `TextureData` struct `(int Width, int Height, byte[] Rgba)`. **Owns the texture cache:** `_Bitmaps` becomes `LazyConcurrentDictionary<RecordId, TextureData>` (already defined in `TQVaultAE.Domain`) — same unbounded cache as today, with same thread-safe get-or-add pattern. |
| UI layer (`IBitmapFactory<TNative>`) | Injectable generic interface (`IBitmapFactory<TNative>`) with one implementation per UI project. `IBitmapFactory<TNative>.Create(TextureData data)` → platform-native Bitmap (e.g. `IBitmapFactory<AvaloniaBitmap>` or `IBitmapFactory<SystemDrawingBitmap>`). NOT a cache — just a stateless converter. Both WinForms and Avalonia consume the same `TextureData` from the cache — zero duplication. Testable via interface mocking. |

**Why `IUIService` keeps the cache:** The new cache stores `TextureData` (framework-agnostic struct). Both WinForms and Avalonia consume the same cached `TextureData` — each project's `BitmapFactory.Create(data)` wraps it into a native Bitmap. No per-project cache needed.

**Data flow:**
```
ArcFile/Database → TEX byte[] → Magick.NET decodes DDS → TextureData(W, H, RGBA) → IUIService._Bitmaps caches → BitmapFactory.Create(data) → WinForms/Avalonia Bitmap
```

**Cache eviction:** Not needed. The existing `UIService._Bitmaps` has no eviction and works fine (500 items × ~256KB = 128MB, trivial for desktop). Cache clears on player/vault switch. Same `LazyConcurrentDictionary` pattern, no added complexity. Thread-safe (atomic get-or-add).

#### 0.2 Checklist

- [ ] Define `TextureData` struct `(int Width, int Height, byte[] Rgba)` in `TQVaultAE.Domain` (alongside `LazyConcurrentDictionary` which also lives in Domain)
- [ ] Update `IUIService.GetBitmap()` / `LoadBitmap()` / `LoadRelicOverlayBitmap()` / `DefaultBitmap` return types to `TextureData`
- [ ] Update `_Bitmaps` cache type to `LazyConcurrentDictionary<RecordId, TextureData>`
- [ ] Update `AddBitmap()` to capture dimensions from `IBitmapService.LoadFromTexMemory()` and cache them in `TextureData`
- [ ] Define `IBitmapFactory<TNative>` generic interface in `TQVaultAE.Application/Contracts/Services/` — each UI project implements with its native bitmap type (e.g. `IBitmapFactory<Avalonia.Media.Imaging.Bitmap>`). Registered as singleton in DI per UI project.
- [ ] Update `IBitmapService.ResizeImage()` signature: input becomes `(byte[] rgba, int w, int h, int newW, int newH, ResizeQuality quality, bool maintainAspectRatio = false)`, returns `TextureData`
- [ ] Ensure `LoadRelicOverlayBitmap` sentinel comparison (`if (result == this.DefaultBitmap) return null`) uses reference equality on the single cached `DefaultBitmap` `TextureData` instance
- [ ] Audit Magick.NET DDS format support against TQ's actual DDS corpus (DXT1/DXT3/DXT5/uncompressed)
- [ ] Adopt interface split early (see Phase 5.1 design): shared `IUIService` (event-free, all methods), `IWinFormsUIService : IUIService` (adds events, lives in `TQVaultAE.GUI`), `IAvaloniaUIService : IUIService` (adds `StatusBarViewModel`). This split happens BEFORE signature changes.
- [ ] **Transitional state:** `System.Drawing.Common` is removed from `Presentation.csproj`. All WinForms `System.Drawing` compat code (Font creation, bitmap conversion) moves to `TQVaultAE.GUI` compat shim. No conditional compilation needed in Presentation.
- [ ] Build `TQVaultAE.GUI` (WinForms) in CI after all Phase 0 service contract changes — compile-only verification that WinForms consumers still build

### 0.3 — Linux Service Stubs

Define concrete implementations for the services that need Linux alternatives:

| Interface | Win32 Implementation | Linux Implementation |
|-----------|---------------------|---------------------|
| `ISoundService` | `SoundServiceWin` (NAudio + SoundPlayer) | `SoundServiceLinux` (OpenTK.OpenAL + MP3Sharp) — cross-platform, no native deps. See details below. |
| `IGamePathService` | `GamePathServiceWin` (registry + Steam/GOG) | `GamePathServiceLinux` (Proton prefix probe + manual path) |
| `IGameFileService` | `GameFileServiceWin` (CMD/MKLINK/fsutil) | `GameFileServiceLinux` (POSIX symlinks, no shell commands) |

All Linux implementations live in a new `TQVaultAE.Services.Linux` project (mirrors `TQVaultAE.Services.Win32` structure). Add project reference from `TQVaultAE.GUI.Next.csproj`.

**`IAddFontToOS` is NOT registered in the Avalonia DI graph** — SkiaSharp loads embedded `.ttf` bytes directly via `SKTypeface.FromData(fontBytes)`. No OS font installation needed on any platform.

**`IFontService` redesign** — follows the decision in Phase 0.1a: neutral `IFontService` returning `SKTypeface` (~4 overloads); WinForms compat shim in `TQVaultAE.GUI`; interface moved to `TQVaultAE.Presentation`.

#### SoundServiceLinux Implementation Details

(`TQVaultAE.Services.Linux/SoundServiceLinux.cs`)

- **Playback:** `ALC.OpenDevice(null)` → `ALC.CreateContext()` → `ALC.MakeContextCurrent()` at startup.
- **⚠️ OpenAL context is thread-local.** `ALC.MakeContextCurrent()` only applies to the calling thread. If `PlaySound()` is called from the UI dispatcher thread, that thread has no context and playback will silently fail. **Fix:** marshal all `PlaySound()` calls to a dedicated audio thread via `ConcurrentQueue<RecordId>` + `AutoResetEvent` + background worker thread.
- **Shutdown:** `CancellationTokenSource` signals worker thread to exit. Drain remaining queue items or discard after a 2s deadline. `Dispose()` waits for thread join. OpenAL cleanup (delete sources, buffers, close context, close device) runs after worker exits.
- **Lazy-initialize** on first `PlaySound()` call — don't block startup on audio device discovery.
- **Graceful failure:** If `OpenDevice` returns null (headless CI, no audio hardware), `PlaySound()` is a no-op — never throw. Also wrap with try/catch for device unavailability after initialization (e.g. USB audio unplug).
- **Resource management:** Pre-load OpenAL buffers from `byte[]`; pool `Source` objects. Dispose releases all buffers + sources + context + device.
- **MP3 conversion:** `MP3Sharp` accepts `Stream` input — matches existing `ConvertMp3ToWav(Stream inMp3, Stream outWav)` contract. `MP3Sharp.MP3Stream` decodes MP3 → WAV (RIFF header + PCM) into output `Stream`. Load WAV data into OpenAL buffer. Test with a known 100ms sine-wave fixture to verify RIFF header validity.
- **Dispose leak check:** Create → play → dispose → verify no leaked OpenAL contexts.

#### Magick.NET Cross-Platform Note

`BitmapService.LoadFromTexMemory()` uses `Magick.NET-Q8-AnyCPU` for DDS → PNG conversion (not `System.Drawing`). This is already cross-platform but requires:

- Windows: `winget install ImageMagick.Q8` (existing dependency)
- Linux: `apt install libMagickWand-7.Q8 libMagickCore-7.Q8`
- CI: add Linux apt packages to `build-and-test.yml`

### 0.3a — Localization Strategy

**Chosen: `IStringLocalizer<T>` + ViewModel binding**

Strings loaded from `.resx` via `IStringLocalizer`, exposed as ViewModel properties, bound in XAML via `{Binding}`. Clean MVVM, fully testable, no custom XAML markup extensions needed. Ensure `Thread.CurrentUICulture`-based switching works for all 800+ resource strings.

- [ ] Verify `Thread.CurrentUICulture` switching resolves all 800+ resource keys across EN/FR/DE/RU
- [ ] Verify `IStringLocalizer<T>` resolves correctly for each ViewModel type

### 0.4 — Test Project & CI

- [ ] Create `src/TQVaultAE.GUI.Next.Tests/` project (xUnit.v3, AwesomeAssertions, Moq)
- [ ] **Pre-flight:** Audit `Directory.Packages.props` for existing Avalonia/CommunityToolkit/Microsoft.Extensions versions before adding new packages. Verify no version conflicts. Note: `TQVaultAE.GUI.Next.Tests` needs packages from both CPM scopes (Avalonia.Headless from `GUI.Next/Directory.Packages.props` + xUnit/Moq from root `Directory.Packages.props`). Consolidate into one `Directory.Packages.props` or duplicate test-framework versions into GUI.Next scope.
- [ ] Add to `Directory.Packages.props`: `Avalonia.Headless.XUnit.v3` v12.0.4, `Avalonia.Headless` v12.0.4 (note: `.v3` suffix required — existing tests use xunit.v3)
- [ ] Configure headless `AppBuilder` for tests (no display server required)
- [ ] Add Linux CI apt dependencies to `build-and-test.yml`: `apt install libfontconfig1 libfreetype6 libMagickWand-7.Q8 libMagickCore-7.Q8`
- [ ] Create `ViewModelTestBase` fixture that initializes headless Avalonia context once per assembly
- [ ] Create `TestDataFactory` / `ViewModelBuilder` with pre-built mocks:
  - `MockPlayerProvider` returning known player data
  - `MockVaultProvider` returning a sack with 50 items
  - `MockUIService` returning known RGBA `byte[]`
  - `MockGamePathService` pointing to a temp directory
- [ ] Add `ubuntu-latest` to CI build matrix in `.github/workflows/build-and-test.yml`
- [ ] Add Linux CI job with dependencies:
  ```yaml
  test-linux:
    runs-on: ubuntu-latest
    env:
      AVALONIA_HEADLESS: "1"  # Required for headless rendering
    steps:
      - run: sudo apt-get install -y libfontconfig1 libfreetype6 libMagickWand-7.Q8 libMagickCore-7.Q8
      - run: dotnet test --blame-crash --blame-hang-timeout 60s
      # Run TQVaultAE.Tests and TQVaultAE.GUI.Next.Tests in separate CI jobs
  ```
- [ ] Create `TQVaultAE-CrossPlatform.slnf` solution filter excluding `TQVaultAE.GUI` (WinForms) and `TQVaultAE.Services.Win32` for Linux CI; includes new `TQVaultAE.Services.Linux` project
- [ ] Add `ValidateOnBuild = true` and `ValidateScopes = true` to DI container in tests
- [ ] **Measure coverage baseline NOW:** run `dotnet test --collect:"XPlat Code Coverage;Format=opencover"` on existing `TQVaultAE.Tests`. Record:
  - `TQVaultAE.Services = ___%`
  - `TQVaultAE.Data = ___%`
  - `TQVaultAE.Domain = ___%`
  - Update these numbers in this plan after measurement.
- [ ] Configure per-project `<CoverletThreshold>` in csproj test configurations (coverage on manually authored code only, excluding `*.g.cs` and `*.axaml.cs`):
  - `TQVaultAE.GUI.Next`: ≥70% (≥80% ViewModels — focus on RelayCommand paths, OnXxxChanged side effects, error branches)
  - `TQVaultAE.Services`: ≥ recorded baseline + 2%
- [ ] Configure coverlet exclusions: `*.g.cs` (source-generated), `*.axaml.cs` (XAML code-behind)
- [ ] Add coverage collection to CI: `dotnet test --collect:"XPlat Code Coverage;Format=opencover"` + `reportgenerator` step comparing against per-project `<CoverletThreshold>` values
- [ ] Add `BindingErrorAssertionHelper` — subscribes `Avalonia.Logging.Logger.Sink` at `LogEventLevel.Debug` to catch all binding error levels. Register sink BEFORE headless `AppBuilder` starts. Buffer events in `ConcurrentQueue`, assert empty after test concludes. (See Phase 0.5 standards for implementation example.)

#### Testing Gate

- [ ] Test project builds on both Windows and Linux
- [ ] `ViewModelTestBase` initializes headless Avalonia session
- [ ] A ViewModel resolves from `ServiceCollection` built by `ServiceCollectionExtensions`
- [ ] Round-trip `INotifyPropertyChanged` assertion passes
- [ ] CI pipeline passes on both `windows-latest` and `ubuntu-latest`
- [ ] Coverage baseline recorded in plan

### 0.4a — Test Fixture Data

- [ ] Create `test/Fixtures/` directory for shared test data:
  - `minimal-player.chr` — a minimal TQ player character file
  - `minimal-vault.vault` — a vault with 10-20 items covering different item types
  - `minimal-stash.stash` — a stash file
  - `dds-fixtures/` — 5 DDS files covering DXT1, DXT3, DXT5, uncompressed, and edge cases
- [ ] Create `FixtureManager` class that copies fixtures to `Path.GetTempPath()` before each test collection (`[CollectionDefinition]`)
- [ ] Fixtures committed to git (small binary files) — not generated at test time
- [ ] Verify DDS fixtures decode correctly via `IBitmapService.LoadFromTexMemory()` on both platforms

### 0.4b — ViewModel Test Patterns

Reference patterns for ViewModel testing across all phases:

```csharp
// Testing [RelayCommand]
vm.MyCommand.CanExecute(null).Should().BeTrue();
vm.MyCommand.Execute(null);
vm.SomeProperty.Should().Be(expectedValue);

// Testing [ObservableProperty] side effects
bool wasCalled = false;
vm.PropertyChanged += (_, e) => { if (e.PropertyName == "SelectedPlayer") wasCalled = true; };
vm.SelectedPlayer = new PlayerInfo();
wasCalled.Should().BeTrue();

// ViewModel integration
playerPanel.SelectPlayer("PlayerName");
vaultPanel.Items.Should().NotBeEmpty();
```

### 0.5 — Testing Standards (Applies to All Phases)

These verification patterns are referenced by all per-phase Testing Gates:

| Standard | How to Verify |
|----------|---------------|
| **Binding errors** | `BindingErrorAssertionHelper` subscribes `Avalonia.Logging.Logger.Sink` (not `ILogSink`) — the static callback on `Avalonia.Logging.Logger`. Subscribe at `LogEventLevel.Debug` to catch all diagnostic levels. Register sink BEFORE headless `AppBuilder` starts. Buffer events in `ConcurrentQueue`, assert empty after test concludes. (Internally uses a `BindingErrorSink` class implementing the `ILogSink`-compatible delegate, wrapped by `BindingErrorAssertionHelper` for lifecycle management.) Example:
```csharp
var errors = new ConcurrentQueue<string>();
Logger.Sink = new BindingErrorSink(errors);
// Verify Avalonia 12.0.4 binding error log level is Debug/Warning/Error
``` |
| **Font loading** | Iterate each `Resources.*` font `byte[]`, call `SKTypeface.FromData(fontBytes)`, assert non-null — on BOTH platforms. Do NOT use `SKFontManager.Default.CreateTypeface()` (requires populated fontconfig cache; fails on headless Linux). Also register each font via `FontManager.Current.AddFontCollection()` and verify `FontFamily` resolves in XAML (render `TextBlock` with bound `FontFamily`, assert glyphs render without tofu). |
| **Glyph coverage** | Render longest translated item name in each locale (EN/FR/DE/RU) through each font; verify no missing-glyph boxes. Define "tofu signature" by rendering a CJK character in Liberation Sans, capturing pixel pattern, then asserting that signature does NOT appear in locale test strings. |
| **Font metrics** | Render same string in Liberation Sans vs Arial (Windows) and Liberation Serif vs Times New Roman; measure rendered width; assert within 2% tolerance. |
| **Bitmap cache concurrency** | 16 threads call `IUIService.GetBitmap(sameKey)` simultaneously; assert `IBitmapService.LoadFromTexMemory` invoked exactly once; all threads receive same `TextureData`; no deadlock after 5s. |
| **Full bitmap pipeline** | `IBitmapService.LoadFromTexMemory()` → `byte[]` → `IUIService._Bitmaps` caches → `BitmapFactory.Create(bytes, w, h)` → native Bitmap. Verify RGBA values unchanged end-to-end. |
| **DI correctness** | Resolve two instances per service; singletons return same instance; transients return different instances. |
| **Localization regression** | Iterate all resource keys across all supported cultures; assert non-null non-empty value; format placeholder counts (`{0}`, `{1}`) match across locales. |
| **SoundServiceLinux** | OpenAL: mock device that returns null → `PlaySound()` is no-op (no crash). MP3Sharp: feed `Stream` containing known 100ms sine-wave MP3 → decode via `ConvertMp3ToWav(Stream, Stream)` → verify RIFF/WAV header valid + PCM samples correct. Dispose → verify no leaked OpenAL contexts. Manual gate: test with real audio hardware on a Linux desktop. |
| **ViewLocator completeness** | For each registered ViewModel in the factory dictionary (post-Phase 1.4), assert the corresponding factory delegate returns non-null Control. Catches "added ViewModel, forgot to add View" bugs. Also verify reverse: each ViewModel's public bindable properties have at least one binding reference in the corresponding `.axaml` file. |
| **Memory leaks** | `[Collection("MemoryTests")]` isolation to prevent parallel test interference. Create ViewModel → resolve full View tree → dispose → double `GC.Collect(2, GCCollectionMode.Forced, true)` + `GC.WaitForPendingFinalizers()` cycle → assert `WeakReference` to ViewModel is dead. Flaky failures >5% investigated as real leaks but may be Avalonia internal false positives. |
| **Save-file integrity** | Load known-good player/vault file → modify item position → save → reload → assert item position matches. Verify file hash unchanged for no-op save. Most dangerous regression vector for TQVaultAE. |
| **Theme key completeness** | Recurring CI check: reflect all `StaticResource`/`DynamicResource` keys in XAML; assert every key exists in `DefaultTheme.axaml`. Run on every commit. Extended to `LegacyTheme.axaml` in Phase 8. |
| **Snapshot tolerance** | Perceptual hash (pHash) with Hamming distance ≤5 on `Control.RenderToBitmap()` output. Store per-platform baselines in `test/Snapshots/win-x64/` and `test/Snapshots/linux-x64/`. Different platforms get different baselines (GPU/OS rendering differences are expected). CI compares pHash of current render vs stored baseline. |

---

## Phase 1: Project References, DI & Platform Foundation

**Goal:** Wire the Avalonia project into the existing service layer with correct platform gating.

> **Phase 1 Complete When:** CI passes on both Windows and Linux, DI container validates without errors, ViewLocator resolves all ViewModels, platform-specific services resolve correctly on each OS.

### 1.1 — Project References

- [ ] Add project references to `TQVaultAE.GUI.Next.csproj`:
  - `TQVaultAE.Application`
  - `TQVaultAE.Config`
  - `TQVaultAE.Data`
  - `TQVaultAE.Domain`
  - `TQVaultAE.Logs`
  - `TQVaultAE.Presentation`
  - `TQVaultAE.Services`
  - `TQVaultAE.Services.Linux` — cross-platform Linux service implementations
  - *(NOT `TQVaultAE.Services.Win32` — it targets `net10.0-windows` and cannot be referenced)*
- [ ] Change `OutputType` in `TQVaultAE.GUI.Next.Desktop.csproj` from `WinExe` to `Exe` (cross-platform compatible)
- [ ] Desktop csproj stays single `net10.0` — platform gating uses runtime `OperatingSystem.IsWindows()` checks (simpler build, no multi-targeting overhead)

### 1.2 — DI Composition Root

- [ ] Add NuGet packages: `Microsoft.Extensions.Hosting` (provides `Host.CreateApplicationBuilder()`), `Microsoft.Extensions.Logging`
- [ ] Make `IServiceProvider` available to Avalonia — **bind BEFORE `AppBuilder.Configure`** to avoid race conditions with early ViewModel resolution:
  ```csharp
  var sp = host.Services;
  AvaloniaLocator.CurrentMutable.Bind<IServiceProvider>().ToConstant(sp);
  AppBuilder.Configure<App>()
      .StartWithClassicDesktopLifetime(args);
  ```
  (Do NOT use `AfterSetup` — ViewModels may resolve from `AvaloniaLocator` before `AfterSetup` fires.)
- [ ] Create platform-specific DI registration via runtime checks in `Program.cs`:
  ```csharp
  if (OperatingSystem.IsWindows())
  {
      services.AddSingleton<ISoundService, SoundServiceWin>();
      services.AddSingleton<IGamePathService, GamePathServiceWin>();
      services.AddSingleton<IGameFileService, GameFileServiceWin>();
      // NO IAddFontToOS — obsolete for Avalonia
  }
  else
  {
      services.AddSingleton<ISoundService, SoundServiceLinux>();
      services.AddSingleton<IGamePathService, GamePathServiceLinux>();
      services.AddSingleton<IGameFileService, GameFileServiceLinux>();
      // NO IAddFontToOS — obsolete for Avalonia
  }
  ```
- [ ] Extract shared service registrations into `ServiceCollectionExtensions.cs` (reusable in tests)
- [ ] **DO NOT copy the static `ServiceProvider` field** from old `Program.cs` — all ViewModels and services must be constructor-injected via DI
- [ ] Pass `IServiceProvider` to `App` class via a non-static `Initialize(IServiceProvider)` method
- [ ] Remove stub `MainViewModel` instantiation; resolve from DI
- [ ] Wire `log4net` + `Microsoft.Extensions.Logging` in DI (existing `LoggerFactory.AddLog4Net()`)
- [ ] Wire global exception handling:
  - `Dispatcher.UIThread.UnhandledException` → log + error dialog
  - `AppDomain.CurrentDomain.UnhandledException` → log + graceful shutdown

### 1.3 — Platform-Specific Fixes

- [ ] Replace `Assembly.GetExecutingAssembly().EscapedCodeBase` with `AppContext.BaseDirectory` in:
  - `GamePathServiceWin.ResolveTQVaultPath()`
  - `UserSettings.ResolveUserSettingsFilePath()`
- [ ] Refactor `UserSettings` path resolution:
  - Use `Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)` on ALL platforms — it maps to `%APPDATA%` on Windows and `~/.config` on Linux automatically
  - Only add `$XDG_CONFIG_HOME` as an explicit override if the default is unsuitable
- [ ] Audit service layer async patterns — in Avalonia, the primary risks are:
  - `async void` event handlers in ViewModels (unobserved exceptions crash the process)
  - Fire-and-forget `Task.Run` in ViewModel constructors
  - Long-running sync-over-async blocking the UI thread
  - Add `ConfigureAwait(false)` to library-code async methods where appropriate for consistency

### 1.4 — ViewLocator

- [ ] Replace reflection-based `ViewLocator` with explicit View-ViewModel registration via `Func<ViewModelBase, Control>` factory delegates
- [ ] This avoids trimming issues (`Type.GetType` with string manipulation breaks under NativeAOT/trimming)

#### Testing Gate

- [ ] DI container resolves all registered services without exceptions (`ValidateOnBuild = true`)
- [ ] Platform-specific services resolve correctly on Windows and Linux
- [ ] `ViewLocator` resolves every registered ViewModel to a non-null View
- [ ] DI correctness: singletons return same instance on second resolve; transients return different instances
- [ ] Build succeeds on both Windows and Linux
- [ ] CI pipeline passes on both `windows-latest` and `ubuntu-latest`

---

## Phase 2: Startup, Configuration & Theming

**Goal:** Application boots, loads config, navigates to main view, and applies user-selected theme.

> **Phase 2 Complete When:** App starts with correct (Default) theme on both platforms, UserSettings round-trips, theme infrastructure in place, all resource keys exist in DefaultTheme.axaml.

### 2.1 — Entry Point

- [ ] Port entry point flow from old `Program.cs` (database init, logging, splash)
- [ ] Port `UserSettings` loading (from `TQVaultAE.Config`) — add `SelectedTheme` property
- [ ] Create Avalonia `SplashScreenWindow` (replacement for `SplashScreenForm`)
  - Use async initialization with `IProgress<T>` for DB load progress — do not block UI thread
- [ ] Wire `IGamePathService` and validate game path on startup
- [ ] Initialize database on startup (if applicable)

### 2.2 — Theming Infrastructure

- [ ] Define shared resource keys in `Styles/ThemeResources.axaml`:
  - `Theme.AccentBrush`, `Theme.ControlBackgroundBrush`, `Theme.ControlForegroundBrush`
  - `Theme.ControlBorderBrush`, `Theme.ItemBorderBrush`, `Theme.ContainerMargin`
  - `Control.*` keys for control-specific overrides
1. **Define `Theme.DefaultFontFamily` resource key:**
  - **Default theme:** Inter (from `Avalonia.Fonts.Inter` NuGet package — already referenced)
  - **Legacy theme:** Liberation Sans → Albertus MT as primary font (loaded via `SKTypeface.FromData` at runtime — implemented in Phase 8)
- [ ] Create `Styles/DefaultTheme.axaml` as the vanilla Avalonia/Fluent baseline
- [ ] Extract inline `StreamGeometry` icons from `App.axaml` into `Styles/Icons.axaml`
- [ ] Wire theme selection in `App.axaml.cs` from `UserSettings.SelectedTheme` on startup (defaults to "Default")
- [ ] Add runtime theme toggle infrastructure in `MainViewModel` — toggles Default ↔ Legacy once Legacy is available (Phase 8)
- [ ] Implement localization per strategy chosen in Phase 0.3a
- [ ] Create CI check for theme key completeness: parse all `.axaml` files for `StaticResource`/`DynamicResource` keys; verify each key exists in `DefaultTheme.axaml`. Run on every commit per Phase 0.5 standard. Extended to both dictionaries in Phase 8.

#### Testing Gate

- [ ] App starts visually with splash screen and main window on both platforms **(E2E — deferred to Phase 7; verify in headless CI now that `AppBuilder` succeeds without exceptions)**
- [ ] `UserSettings` round-trips (serialize/deserialize) on both platforms
- [ ] Theme resource keys resolve against `DefaultTheme.axaml` without binding errors
- [ ] `SelectedTheme` preference persisted and applied on startup

---

## Phase 3: Main Window Shell

**Goal:** Replace `MainForm` with Avalonia equivalent, decomposed into composable ViewModels.

> **Phase 3 Complete When:** All sub-ViewModels resolve from DI, shell layout renders with panel placeholders, menu/toolbar dispatch to correct ViewModels, sub-ViewModel integration scenarios pass. Decompose further if any ViewModel exceeds ~500 lines.

**⚠️ Risk:** `MainForm` is a God class (~10K+ lines, 10 partial files, 21 `#region` blocks). Do NOT create a single `MainViewModel` with the same complexity. Decompose.

**⚠️ Highest-complexity component:** `SackPanel` (2,800+ lines, 6 partial class files, `SackPanel.Rendering.cs` with custom Paint handlers). This is the single riskiest control to port. Start with SackPanel early in Phase 4 to derisk the schedule.

### 3.1 — Paint Handler Audit

Before writing any Avalonia code, audit `MainForm` for WinForms `Paint` event handlers. These are **fundamentally incompatible** with Avalonia's retained-mode rendering and must be re-implemented using Avalonia primitives (shapes, `DrawingContext`, `ControlTemplate`).

### 3.2 — ViewModel Decomposition

Map each `MainForm` partial file to a dedicated ViewModel:

| Old Partial | New ViewModel |
|-------------|---------------|
| `MainForm.cs` (core) | `MainViewModel` (shell, navigation, DI orchestration) |
| `MainForm.Panel.cs` | `PanelLayoutViewModel` (splitter positions, panel visibility) |
| `MainForm.Player.cs` | `PlayerPanelViewModel` (player list, character selection) |
| `MainForm.Vault.cs` | `VaultPanelViewModel` (vault/stash inventory grid) |
| `MainForm.Stash.cs` | `StashPanelViewModel` (stash management) |
| `MainForm.Search.cs` | `SearchViewModel` (search bar, results) |
| `MainForm.Forge.cs` | `ForgeViewModel` (artifact forging) |
| `MainForm.Clipboard.cs` | `ClipboardViewModel` (copy/paste operations) |
| `MainForm.Export.cs` | `ExportViewModel` |
| `MainForm.Loading.cs` | `LoadingViewModel` (save/load orchestration) |
| `MainForm.Lifecycle.cs` | Handled by Avalonia window events (`OnLoaded`, `OnClosing`) |

- [ ] Design `MainView.axaml` layout (menu bar, toolbars, panels, status bar)
- [ ] Create `MainViewModel` as a shell orchestrator, not a God class
- [ ] Create each sub-ViewModel with `[ObservableProperty]` and `[RelayCommand]` from CommunityToolkit.Mvvm
- [ ] Port menu structure and toolbar
- [ ] Port status bar — `IUIService` events replaced with `INotifyPropertyChanged` properties bound to status bar `TextBlock`

#### Testing Gate

- [ ] Each sub-ViewModel resolves from DI and initializes without errors
- [ ] Main window shell layout creates expected visual tree with all panel placeholders **(E2E — deferred to Phase 7)**
- [ ] Menu and toolbar commands dispatch to correct ViewModels
- [ ] Sub-ViewModel integration — **each scenario independently verifiable:**
  - [ ] Player selection → `VaultPanelViewModel` refreshes with correct data
  - [ ] Bag click dispatches item transfer to correct destination
  - [ ] Search → results display correctly
  - [ ] Drag item from search results to stash completes without errors
  - [ ] Forge artifact workflow completes end-to-end

---

## Phase 4: Controls

**Goal:** Reimplement all WinForms UI controls using standard Avalonia controls. Create custom Avalonia controls only when the standard feature set is exhausted.

> **Phase 4 Complete When:** All controls render without binding errors, SackPanel works with 500+ items, snapshot tests generate baselines and pass on both platforms, DPI scaling verified at 100%/125%/150%.

**Principle:** Many WinForms custom controls exist solely for dynamic resizing with aspect ratio preservation — a feature built into Avalonia by default via layout containers (`Grid`, `DockPanel`, `Viewbox`, `StackPanel`, `RelativePanel`).

**Priority ladder (strict order):**
1. **Compose from standard controls** — `Button`, `ToggleButton`, `Border`, `Grid`, `Image`, `TextBlock`, `ToolTip`, `ScrollViewer`, `ItemsRepeater`, `UniformGrid`, `Viewbox`, etc.
2. **Style existing controls** — use Avalonia styles, `ControlTemplate`, and `DataTemplate` to achieve the desired appearance without subclassing
3. **Custom control** — create a `TemplatedControl` subclass only when composition and styling cannot achieve the required behavior

**Old controls mapped to likely modern equivalents:**

| WinForms Control | Avalonia First Choice | Fallback |
|-----------------|----------------------|----------|
| `SackPanel` | `UniformGrid` (tiled item grid) + `ItemsRepeater` + `ScrollViewer` | Custom `TemplatedControl` for sack-specific layout logic |
| `BagButtonBase` / `BagButtonSmall` / `BagButtonLarge` | `ToggleButton` styled with `ControlTemplate` + different `ThemeVariant` per size | Custom `BagButton` if interaction logic demands it |
| `ScalingLabel` | `TextBlock` inside `Viewbox` (built-in aspect-ratio scaling) | — (unlikely needed; test on 125%/150% DPI for font blur) |
| Tooltip controls (6 files) | `ToolTip` attached property + `ToolTip.Tip` styled per theme | Custom `ToolTip` if WinForms-specific placement/delay logic is required |
| `SackPanel` item rendering | `Image` + `TextBlock` inside `DataTemplate` with binding to item model | Custom `ItemControl` for complex multi-part item rendering |

- [ ] Audit all 21+ WinForms controls against standard Avalonia equivalents
- [ ] Implement `SackPanel` — start with `UniformGrid` + `ItemsRepeater`; only subclass if needed
- [ ] Implement bag buttons — start with styled `ToggleButton`; only subclass if needed
- [ ] Verify `Viewbox` + `TextBlock` handles all `ScalingLabel` cases (test on 125%/150% DPI)
- [ ] Port tooltip behavior via styled `ToolTip` attached property
- [ ] Register all control styles in theme resource dictionaries (`DefaultTheme.axaml` / `LegacyTheme.axaml`)
- [ ] Enforce `AvaloniaUseCompiledBindingsByDefault` (already `true` in csproj) — all XAML bindings must compile
- [ ] Start with `SackPanel` first — it is the highest-risk control (2,800+ lines, custom Paint rendering) and derisks the schedule if tackled early

**⚠️ Custom GDI+ drawing (SackPanel, BagButtonBase, etc.) has no abstraction layer today.** These controls use `PaintEventArgs.Graphics` directly — `DrawString`, `FillRectangle`, `DrawImage`. In Avalonia, this becomes:
- `Control.Render(DrawingContext)` overrides (for custom-draw areas)
- Composite XAML templates with `DataTemplate` (for item slots)
- `Image` + `Border` + `TextBlock` composition (for simple decorations)

Audit these controls in Phase 3.1 Paint Handler Audit and design their Avalonia equivalents before starting Phase 4 controls work.

#### Testing Gate

- [ ] Each control renders without binding errors (verified via `BindingErrorAssertionHelper`)
- [ ] `Viewbox` + `TextBlock` renders sharp text at 100%, 125%, and 150% DPI (pHash comparison)
- [ ] `Viewbox` + `TextBlock` renders sharp text at 100%, 125%, and 150% DPI (verify via pHash comparison against per-DPI baselines — same method as Phase 0.5 snapshot tolerance standard)
- [ ] Item `DataTemplate` renders with correct icon, text, and colors
- [ ] **Snapshot tests start here:** generate baseline PNGs for each custom control at 100%/125%/150% DPI via `Control.RenderToBitmap()`. Compare on each CI run with pHash Hamming distance ≤5 (per Phase 0.5 snapshot tolerance standard). Legacy theme baselines added in Phase 8.

---

## Phase 5: Dialog Forms

**Goal:** Migrate all dialog windows to MVVM patterns.

**Old dialogs:**
- `AboutBox`
- `BagButtonSettings`
- `CharacterEditDialog`
- `ItemProperties`
- `ItemSeedDialog`
- `ResultsDialog`
- `SearchDialogAdvanced`
- `SettingsDialog`
- `VaultMaintenanceDialog`

### 5.1 — IUIService Redesign

**⚠️ This interface split MUST happen in Phase 0 scope (before signature changes).** It is documented here for complete design reference.

Before porting dialogs, refactor `IUIService` to replace WinForms event patterns with Avalonia-native patterns. **Use a clean interface split to avoid dead event contracts:**

```csharp
// Shared contract — event-free, used by BOTH frameworks
public interface IUIService { /* all current methods EXCEPT events */ }

// WinForms-specific — adds events for legacy subscribers (lives in TQVaultAE.GUI)
public interface IWinFormsUIService : IUIService {
    event NotifyUserEventHandler NotifyUserEvent;
    event ShowMessageUserEventHandler ShowMessageUserEvent;
}

// Avalonia-specific — adds StatusBarViewModel
public interface IAvaloniaUIService : IUIService {
    StatusBarViewModel StatusBar { get; }
}
```

Do NOT use `IAvaloniaUIService : IUIService` with inherited-but-never-fired events — that creates dead code consumers can accidentally subscribe to with no compile error.

- `IAvaloniaUIService` becomes a mediator: updates `StatusBarViewModel` properties via `INotifyPropertyChanged` instead of firing events
- Avalonia status bar `TextBlock` binds to `StatusBarViewModel` properties — no code-behind event handlers
- Remove `NotifyUser(string, Color)` overload (System.Drawing leak); keep `NotifyUser(string, TQColor)` (already neutral)

### 5.2 — Dialog Implementation

**⚠️ Safety:** `VaultMaintenanceDialog` modifies save files. Verify save-file integrity after every maintenance operation (see Phase 0.5 save-file integrity test standard).

- [ ] Create Avalonia `Window` subclass for each dialog (9 dialogs: `AboutBox`, `BagButtonSettings`, `CharacterEditDialog`, `ItemProperties`, `ItemSeedDialog`, `ResultsDialog`, `SearchDialogAdvanced`, `SettingsDialog`, `VaultMaintenanceDialog`)
- [ ] Create corresponding ViewModels with `[ObservableProperty]` and `[RelayCommand]` from CommunityToolkit.Mvvm
- [ ] Wire up data binding — no code-behind event handlers
- [ ] Port localized resources from `TQVaultAE.Presentation/Resources.resx`

#### Testing Gate

- [ ] Each dialog ViewModel state transitions correctly (open → edit → save → close)
- [ ] Data binding round-trip: set values in ViewModel → verify XAML displays them → user edits → verify ViewModel receives them
- [ ] Localized strings resolve correctly on both platforms

---

## Phase 6: Search & Item UI

**Goal:** Port search experience and item display.

- [ ] Port `SearchDialogAdvanced` → Avalonia advanced search window
- [ ] Port `ItemProperties` dialog
- [ ] Port `ResultsDialog`
- [ ] Implement item rendering (icons, tooltips, stats)
- [ ] Map `ItemStyle` and `Rarity` enums to Avalonia `IBrush`/`Color` resources in theme dictionaries for item background/font colorization

#### Testing Gate

- [ ] Search ViewModel returns correct results against known test data
- [ ] Item properties dialog displays all item stats correctly
- [ ] Item colorization matches old WinForms item colors (Legacy theme) and is readable (Default theme)

---

## Phase 7: Polish & Performance

Testing is a **per-phase gate** (see Testing Gates under each phase). All Phase 2-6 gates are **headless** (run in CI via `Avalonia.Headless.XUnit.v3`). E2E window-launch verification is deferred to Phase 7's accessibility audit.

### 7.0 — Polish Tasks

- [ ] Keyboard shortcuts and navigation — verify keyboard-only navigation for all workflows
- [ ] Add `AutomationProperties.Name` to key controls for screen reader support:
  - [ ] Every `Button`/`MenuItem`/`TextBox` has `AutomationProperties.Name`
  - [ ] Color contrast verifier: `Theme.ForegroundBrush` vs `Theme.ControlBackgroundBrush` against WCAG AA thresholds
- [ ] Drag-and-drop between sacks/bags
- [ ] Clipboard integration
- [ ] Sound effects (if any) — verify on Windows, graceful no-op on Linux
- [ ] Theme toggle infrastructure tested — switches to Legacy once Phase 8 completes

### 7.1 — Performance

- [ ] Add `BenchmarkDotNet` to `Directory.Packages.props`
- [ ] Benchmark tests for: item loading, search filtering, sack rendering, vault open (cold), vault open (warm)
- [ ] Performance budget: loading a 500-item vault must complete in <2 seconds (absolute cap)
- [ ] **Gate interaction:** whichever is stricter applies — if either absolute 2s cap or 15% regression vs baseline fails, the CI gate fails
- [ ] **Stress scenarios:** Transfer 500 items between vault and player → assert all arrive correctly. Open 10 different vaults in sequence → assert memory returns to baseline (no leak).
- [ ] **Baseline files** stored as JSON in `test/BenchmarkBaselines/{platform}/` per platform (Windows/Linux differ)
- [ ] **Metric:** `Mean` (average execution time). CI compares current `Mean` to stored baseline.
- [ ] **First run:** no baseline exists → store baseline, always pass
- [ ] **Subsequent runs:** CI gate fails if any benchmark `Mean` regresses >15% vs stored baseline
- [ ] Profile and optimize on both platforms

### 7.2 — Code Coverage

- [ ] Coverage thresholds consolidated in Phase 0.4 — see per-project `<CoverletThreshold>` values there

#### Testing Gate

- [ ] Visual regression snapshots match expected output (Default theme)
- [ ] Performance benchmarks pass on both platforms; CI gate fails if any benchmark `Mean` regresses >15% vs stored baseline in `test/BenchmarkBaselines/{platform}/`
- [ ] Accessibility audit **(E2E):** all `Button`/`ToggleButton`/`MenuItem` reachable via Tab; all dialogs close on Escape; Enter/Space activates focused control; verify via headless `InputElement.Focus()` + key simulation + command invocation assertion
- [ ] Regression tests: all 30-50 workflows from Phase 0.0 pass on both platforms

---

## Phase 8: Legacy Theme (WinForms Visual Parity)

**Goal:** Add the Legacy theme resource dictionary, matching the current WinForms UI appearance.

> **Phase 8 Complete When:** All controls render correctly in Legacy theme, theme toggle switches reliably with 500+ items, snapshot baselines pass for both themes, visual comparison against old WinForms screenshots passes manual review.

> **Effort:** M (1-2 weeks) — resource dictionary overrides + per-control styling. No new controls or logic.

### 8.1 — Legacy Theme Resource Dictionary

- [ ] Create `Styles/LegacyTheme.axaml` overriding `Theme.*` and `Control.*` resource keys with WinForms-matching values:
  - Colors: match WinForms `BackColor`, `ForeColor`, `BorderColor` equivalents
  - Fonts: Liberation Sans → Albertus MT as primary font (registered via `EmbeddedResourceFontCollection`)
  - Spacing: match WinForms control margins and padding
  - Borders: match WinForms border styles
- [ ] Register `LegacyTheme.axaml` in `App.axaml` resource dictionaries
- [ ] Extend CI theme key check to verify keys in BOTH `DefaultTheme.axaml` and `LegacyTheme.axaml`

### 8.2 — Control Styling

- [ ] Per-control `ControlTemplate` or style overrides for Legacy visual parity:
  - `SackPanel` — WinForms-matching item slots, selection highlights, grid lines
  - `BagButtonBase` / `BagButtonSmall` / `BagButtonLarge` — WinForms-matching button appearance per size
  - Tooltips — WinForms-matching tooltip background, font, and border
  - Item rendering — WinForms-matching icon sizing and text colorization

### 8.3 — Theme Toggle Polish

- [ ] Runtime theme toggle (Default ↔ Legacy) fully tested with 500+ visible items
- [ ] Theme switch performance: add loading overlay with `IProgress<T>` if toggle lags with large vaults
- [ ] `SelectedTheme` preference saved to `UserSettings` and applied on next startup

### Testing Gate

- [ ] Legacy theme snapshots generated (100%/125%/150% DPI) and pass pHash comparison
- [ ] Theme key completeness check extended to both dictionaries
- [ ] Theme toggle switches all controls without binding errors
- [ ] Visual comparison against WinForms screenshots — manual review gate
- [ ] Performance: theme toggle with 500+ items completes without visible lag

---

## Phase Dependencies

```
Phase 0 (Prerequisites) ──► Phase 1 (DI & Platform) ──► Phase 2 (Startup & Theming)
                                                               │
                                                               ▼
                                                      Phase 3 (Main Window Shell)
                                                          │   3.1 Paint Handler Audit
                                                          │        │
                                          ┌───────────────┤        ▼
                                          ▼               │  Phase 4 (Controls)
                                  Phase 5 (Dialogs)       │  (depends on 3.1 output)
                                          │               │
                                          ▼               ▼
                                  Phase 6 (Search & Items) ◄── Phase 4 (Controls)
                                          │
                                          ▼
                                   Phase 7 (Polish & Performance)
                                           │
                                           ▼
                                   Phase 8 (Legacy Theme)
```

> **Parallelization note:** Phase 4 control **styling** (resource-key-dependent work like `ControlTemplate` and theme dictionaries) can run in parallel with Phase 5 once Phase 2 theming is complete. Phase 4 custom control **rendering** (SackPanel, custom-draw areas) requires Phase 3.1 Paint Handler Audit output. Phase 5 dialogs and Phase 4 styling share only theme resource key names from Phase 2.2.

---

## Release & Deployment

**Development:** Framework-dependent (`dotnet build`). Run with native host on Windows or `dotnet` on Linux. Fast iteration, small output.

**Release:** Self-contained per platform. Users need no .NET SDK installed. Single download, double-click to run.

### CI Release Workflow

Produces three artifacts — the existing WinForms binary (unchanged) plus the new Avalonia self-contained outputs:

```yaml
# .github/workflows/release.yml
publish-winforms:
  runs-on: windows-latest
  steps:
    # Existing WinForms build — preserved as fallback
    - run: dotnet publish src/TQVaultAE.GUI/TQVaultAE.GUI.csproj
            -r win-x64 --self-contained -c Release -o publish/winforms

publish-avalonia-win:
  runs-on: windows-latest
  steps:
    - run: dotnet publish src/TQVaultAE.GUI.Next/TQVaultAE.GUI.Next.Desktop/TQVaultAE.GUI.Next.Desktop.csproj
            -r win-x64 --self-contained -c Release -o publish/avalonia-win-x64

publish-avalonia-linux:
  runs-on: ubuntu-latest
  steps:
    - run: dotnet publish src/TQVaultAE.GUI.Next/TQVaultAE.GUI.Next.Desktop/TQVaultAE.GUI.Next.Desktop.csproj
            -r linux-x64 --self-contained -c Release -o publish/avalonia-linux-x64
```

| Artifact | Target | Invocation |
|----------|--------|-----------|
| `publish/winforms/` | Windows | `TQVaultAE.exe` (existing WinForms — production) |
| `publish/avalonia-win-x64/` | Windows | `TQVaultAE.GUI.Next.Desktop.exe` (Avalonia — new) |
| `publish/avalonia-linux-x64/` | Linux | `./TQVaultAE.GUI.Next.Desktop` (Avalonia — new) |

No `dotnet` prefix required for release builds. All archives are self-contained — bundle the .NET runtime.

- [ ] Create `.github/workflows/release.yml` (new workflow file; triggers on tag push)

---

## Tools & Resources

### Skills
- **`avalonia`** — `skill({ name: "avalonia" })` — MVVM, XAML, data binding, styling, theming, custom controls

### MCP Servers
- **`codebase-memory-mcp`** — code intelligence (search_graph, trace_path, get_architecture)
- **`avalonia-ui-mcp`** — ValidateXaml, ConvertWpfXamlToAvalonia, GenerateTheme, GenerateCustomControl
- **`avalonia-mcp`** — runtime debugger (visual tree, binding errors, screenshots)

### Reference
- Old WinForms code: `src/TQVaultAE.GUI/`
- DI setup: `src/TQVaultAE.GUI/Program.cs`
- Central packages: `src/TQVaultAE.GUI.Next/Directory.Packages.props`
- CI workflow: `.github/workflows/build-and-test.yml`
- Release workflow: `.github/workflows/release.yml`
- Progress tracking: `src/TQVaultAE.GUI.Next/status/phase-N-status.md`