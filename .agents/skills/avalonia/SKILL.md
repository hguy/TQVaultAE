---
name: avalonia
description: Expert guidance for developing cross-platform desktop applications with Avalonia UI framework. Covers MVVM architecture, XAML design, data binding, styling, theming, custom controls, and cross-platform deployment for Windows, macOS, Linux, iOS, Android, and WebAssembly.
license: MIT
compatibility: opencode, claude, cline
---

# Avalonia UI Framework - Orchestration Hub

Modular guidance for cross-platform desktop and mobile development using Avalonia, a WPF-inspired XAML-based framework for .NET.

## Quick Reference: When to Load Which Resource

| Task/Goal | Load Resource |
|-----------|---------------|
| MVVM patterns, data binding, dependency injection, value converters | `resources/mvvm-databinding.md` |
| UI controls reference (layouts, inputs, collections, menus) | `resources/controls-reference.md` |
| Custom controls, advanced layouts, performance optimization, virtualization | `resources/custom-controls-advanced.md` |
| Styling, themes, animations, control templates | `resources/styling-guide.md` |
| Reactive patterns, commands, observables, animations | `resources/reactive-animations.md` |
| Windows, macOS, Linux, iOS, Android implementation details | `resources/platform-specific.md` |

## Framework Overview

**Avalonia** is a cross-platform XAML framework supporting:
- **Platforms**: Windows, macOS, Linux, iOS, Android, WebAssembly
- **Architecture**: MVVM with ReactiveUI support
- **Styling**: CSS-like selectors with Fluent/Simple themes
- **Features**: Data binding, reactive commands, observable collections, custom controls
- **Modern .NET**: .NET 6+ and .NET Standard 2.0

## Getting Started

### Minimal Setup

```csharp
// Program.cs
public static void Main(string[] args)
{
    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
}

public static AppBuilder BuildAvaloniaApp() =>
    AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .LogToTrace();
```

### Standard Project Structure

```
MyAvaloniaApp/
├── MyAvaloniaApp/                  # Shared code
│   ├── App.axaml
│   ├── Views/                      # XAML views
│   ├── ViewModels/                 # Business logic + state
│   ├── Models/                     # Data models
│   ├── Services/                   # Application services
│   ├── Converters/                 # Value converters
│   ├── Assets/                     # Images, fonts
│   └── Styles/                     # Style resources
├── MyAvaloniaApp.Desktop/          # Desktop-specific (Win/Mac/Linux)
├── MyAvaloniaApp.Android/          # Android-specific (optional)
├── MyAvaloniaApp.iOS/              # iOS-specific (optional)
└── MyAvaloniaApp.Browser/          # WebAssembly (optional)
```

## Core Patterns

### MVVM Architecture
1. **View** (XAML): UI presentation with data bindings
2. **ViewModel** (C#): State management and commands
3. **Model** (C#): Business logic and data access
4. **Service**: Cross-cutting concerns (DI/IoC)

### Reactive Programming with ReactiveUI
```csharp
this.WhenAnyValue(x => x.SearchText)
    .Debounce(TimeSpan.FromMilliseconds(300))
    .Subscribe(text => PerformSearch(text));
```

### Platform-Adaptive UI
```xml
<OnPlatform Default="16">
    <On Options="Windows" Content="14" />
    <On Options="macOS" Content="15" />
</OnPlatform>
```

## Navigation by Task

### "I need to build a form with validation"
1. Load `resources/mvvm-databinding.md` → ViewModel with validation
2. Load `resources/controls-reference.md` → TextBox, ComboBox, Button
3. Load `resources/reactive-animations.md` → Debounced validation

### "I'm seeing poor performance with large lists"
1. Load `resources/custom-controls-advanced.md` → Enable virtualization
2. Load `resources/mvvm-databinding.md` → Compiled bindings
3. Load `resources/reactive-animations.md` → Debounce/throttle

### "I need platform-specific behavior"
1. Load `resources/platform-specific.md` → Service interfaces
2. Load `resources/mvvm-databinding.md` → DI registration
3. Platform-specific implementations

### "I want custom styling and animations"
1. Load `resources/styling-guide.md` → Styles and themes
2. Load `resources/reactive-animations.md` → Animations
3. Load `resources/custom-controls-advanced.md` → Control templates

### "I'm building a complex control"
1. Load `resources/custom-controls-advanced.md` → TemplatedControl
2. Load `resources/mvvm-databinding.md` → Attached properties
3. Load `resources/styling-guide.md` → Control templates

## Best Practices

**Architecture**: Strict MVVM separation, DI for testability, business logic in ViewModels

**Performance**: Compiled bindings with `x:DataType`, virtualize collections, debounce updates

**Styling**: Resource dictionaries for consistency, support light/dark themes, test all platforms

**Reactive**: Use observables for events, debounce/throttle input, handle ThrownExceptions

**Testing**: Unit test ViewModels in isolation, use Avalonia.Headless for UI testing, provide design-time DataContext
