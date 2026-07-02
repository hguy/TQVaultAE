# Platform-Specific Implementation Guide

## Platform Detection

```csharp
public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
public static bool IsAndroid => OperatingSystem.IsAndroid();
public static bool IsIOS => OperatingSystem.IsIOS();
public static bool IsBrowser => OperatingSystem.IsBrowser();
```

## Multi-Platform Project Structure

```
MyAvaloniaApp/                    # Shared code (Views, ViewModels, Models, Services)
MyAvaloniaApp.Desktop/            # Desktop bootstrap + platform services
MyAvaloniaApp.Android/            # Android bootstrap + platform services
MyAvaloniaApp.iOS/                # iOS bootstrap + platform services
MyAvaloniaApp.Browser/            # WebAssembly bootstrap + platform services
```

## Service Interface Pattern

```csharp
// Shared: interface
public interface IFileService
{
    Task<string> ReadFileAsync(string path);
    Task<string> PickFileAsync();
}

// Desktop implementation
public class DesktopFileService : IFileService
{
    public async Task<string> PickFileAsync()
    {
        var dialog = new OpenFileDialog { Title = "Select File" };
        var result = await dialog.ShowAsync(mainWindow);
        return result?.FirstOrDefault();
    }
}
```

## Conditional XAML

```xml
<OnPlatform Default="16">
    <On Options="Windows" Content="14" />
    <On Options="macOS" Content="15" />
    <On Options="Android, iOS" Content="18" />
</OnPlatform>
```

## Window Setup Per Platform

```csharp
// Desktop
AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .With(new Win32PlatformOptions { UseWindowsUIComposition = true })
    .With(new X11PlatformOptions { UseDBusMenu = true })
    .With(new MacOSPlatformOptions { ShowInDock = true });

// Android
[Activity(Label = "MyApp", MainLauncher = true, ConfigurationChanges = ...)]
public class MainActivity : AvaloniaMainActivity<App> { }

// iOS
[Register("AppDelegate")]
public class AppDelegate : AvaloniaAppDelegate<App> { }
```

## Platform-Specific Code with `#if`

```csharp
#if WINDOWS
using Windows.Storage;
public async Task ShareAsync(string text) { ... }
#endif

#if MACOS
using AppKit;
public void SetupDockMenu() { ... }
#endif

#if LINUX
public void RegisterDBusService() { ... }
#endif

#if ANDROID
using Android.Content;
public void ShowToast(string message) { ... }
#endif

#if IOS
using UIKit;
public void ShowAlert(string title, string message) { ... }
#endif
```

## Touch vs Mouse

```csharp
if (e.Pointer.Type == PointerType.Touch)
    // Handle touch-specific behavior
else if (e.Pointer.Type == PointerType.Mouse)
    // Handle mouse-specific behavior
```

## Best Practices

1. Abstract platform differences behind interfaces + DI
2. Test on all target platforms regularly
3. Respect native UX patterns per platform
4. Use `OnPlatform` for adaptive XAML
5. Provide graceful fallbacks for missing features
6. Optimize UI complexity for mobile vs desktop
