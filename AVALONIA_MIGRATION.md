# RePKG.Neo - WPF to Avalonia Migration Guide

## Overview
This document details the migration of RePKG.Neo from WPF (Windows-only) to Avalonia (cross-platform). The migration enables the application to run on Windows, Linux, and macOS from a single codebase.

## Changes Made

### 1. Project File (RePKG.Neo.csproj)
**Changes:**
- Removed `<UseWPF>true</UseWPF>` - WPF is Windows-only
- Changed `TargetFramework` from `net10.0-windows` to `net10.0` - now platform-agnostic
- Added Avalonia NuGet packages:
  - `Avalonia` (11.0.0) - Core framework
  - `Avalonia.Desktop` (11.0.0) - Desktop support
  - `Avalonia.Themes.Fluent` (11.0.0) - Fluent theme (same visual style as before)
  - `Avalonia.Dialogs` (11.0.0) - Cross-platform file dialogs
- Changed resource type from `Resource` to `AvaloniaResource` for icon.png

**Why:** These changes are necessary because Avalonia is the framework replacement for WPF, and the new target framework removes Windows-specific dependencies.

---

### 2. App.xaml
**Changes:**
- Updated XML namespace from WPF to Avalonia: `https://github.com/avaloniaui`
- Replaced `ResourceDictionary.MergedDictionaries` with `FluentTheme`
- Changed brush definitions from `SolidColorBrush` to `Color` resources, then created brush wrappers
- Set `RequestedThemeVariant="Light"` for light theme support

**Why:** Avalonia uses different XAML namespaces and a different theming system. Avalonia's resource system is simpler but requires explicit brush definitions.

---

### 3. App.xaml.cs
**Changes:**
- Replaced `System.Windows.Application` with Avalonia's `Application` class
- Updated startup method from `OnStartup` to `OnFrameworkInitializationCompleted`
- Changed to `IClassicDesktopApplicationLifetime` for desktop app lifecycle
- Simplified window initialization:
  ```csharp
  // Old WPF
  protected override void OnStartup(StartupEventArgs e)
  
  // New Avalonia
  public override void OnFrameworkInitializationCompleted()
  ```
- Added `AvaloniaXamlLoader.Load(this)` for XAML loading

**Why:** Avalonia has a different application lifecycle and initialization pattern. The IClassicDesktopApplicationLifetime interface replaces WPF's startup event model.

---

### 4. MainWindow.xaml
**Major changes:**
- Updated XML namespace to Avalonia
- Removed WPF-specific attributes:
  - `ResizeMode="NoResize"` → `CanResize="False"`
  - `RenderOptions.BitmapScalingMode="HighQuality"` → Removed (handled automatically)
  - `WindowStartupLocation` → Kept but works the same in Avalonia
  - `DataContext="{Binding RelativeSource={RelativeSource Self}}"` → Set in code-behind instead
  - `AllowDrop="True" Drop="root_Drop"` → Uses Avalonia's DragDrop API

- Changed image path from `Source="Res/icon.png"` to `Source="avares://RePKG.Neo/Res/icon.png"` (Avalonia resource protocol)

- Simplified binding syntax (removed `Mode=TwoWay` - it's default for CheckBox)

- Replaced `ToolTipService.InitialShowDelay="100"` with `ToolTip.Tip` (simpler)

- Replaced `StackPanel Orientation="Horizontal"` content with emoji (⚙️) instead of Symbol font character

- Removed style references like `Style="{DynamicResource AccentButtonStyle}"` and added `Classes="accent"` instead

- Changed layout containers:
  - Wrapped content in `Grid` with `RowDefinitions="Auto,Auto,*"`
  - Used Avalonia's `DockPanel` with `LastChildFill` property

**Why:** Avalonia has subtle differences in XAML syntax, resource protocols, and binding conventions. These changes align with Avalonia's design patterns.

---

### 5. MainWindow.xaml.cs
**Major refactoring:**

**Namespace changes:**
```csharp
// Remove
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

// Add
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
```

**Event handling:**
- File dialogs are now **async** (Avalonia requirement):
  ```csharp
  // Old WPF (synchronous)
  bool? result = dialog.ShowDialog();
  
  // New Avalonia (async)
  var result = await dialog.ShowAsync(this);
  ```

- Drag & Drop now uses Avalonia's event system:
  ```csharp
  // Old WPF
  private void root_Drop(object sender, DragEventArgs e)
  
  // New Avalonia
  private void OnDrop(object? sender, DragEventArgs e)
  // Register with: AddHandler(DragDrop.DropEvent, OnDrop);
  ```

**Control access:**
- Instead of directly using `TbInput.Text`, we now use:
  ```csharp
  var tbInput = this.FindControl<TextBox>("TbInput");
  tbInput?.Text = "value";
  ```
  This is necessary because XAML controls aren't automatically available in code-behind in Avalonia.

**Message boxes:**
- Replaced `MessageBox.Show()` with Avalonia's `MessageBoxManager`:
  ```csharp
  private void ShowInfo(string content, string title) {
      var msgBox = MessageBoxManager.GetMessageBoxStandard(title, content);
      msgBox.ShowAsync();
  }
  ```

**Window lifecycle:**
- Replaced `Closed="root_Closed"` event with:
  ```csharp
  protected override void OnClosing(WindowClosingEventArgs e) {
      Options.Save();
      base.OnClosing(e);
  }
  ```

**Progress bar animation:**
- Removed WPF's `DoubleAnimation` with `EasingFunction`
- Simplified to direct value assignment (Avalonia handles smooth transitions):
  ```csharp
  // Old WPF
  _smoothAnimation.To = percent * 100;
  ProgressBar.BeginAnimation(ProgressBar.ValueProperty, _smoothAnimation);
  
  // New Avalonia
  if (progressBar != null) {
      progressBar.Value = percent * 100;
  }
  ```

**Why:** Avalonia's APIs are designed differently with async file dialogs, different event systems, and simpler animation handling. The `FindControl` approach is necessary because Avalonia doesn't expose XAML controls as code-behind members by default.

---

### 6. Program.cs (New File)
Created a new entry point for Avalonia:
```csharp
class Program {
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
```

**Why:** Avalonia requires an explicit entry point with `AppBuilder` configuration for platform detection and initialization.

---

## Cross-Platform File Dialogs

The migration now uses Avalonia's `OpenFileDialog` and `OpenFolderDialog` which are **automatically cross-platform**:

```csharp
// File open dialog - works on Windows, Linux, and macOS
var dialog = new OpenFileDialog {
    Title = "Select file",
    AllowMultiple = false,
    Filters = new[] { /* filter patterns */ }
};
var result = await dialog.ShowAsync(this);

// Folder dialog - works on Windows, Linux, and macOS
var folderDialog = new OpenFolderDialog { Title = "Select folder" };
var result = await folderDialog.ShowAsync(this);
```

No platform-specific code needed - Avalonia handles the platform differences internally.

---

## Remaining Tasks

### 1. Build and Test
- Run: `dotnet build` to verify compilation
- Test on Windows
- Test on Linux: `dotnet run`
- Test on macOS: `dotnet run`

### 2. Update Installers
- **Windows:** Update Inno Setup installer to use the new portable executable
- **Linux:** Create AppImage or snap package
- **macOS:** Create DMG installer with code signing

### 3. CI/CD Setup
Add GitHub Actions workflows for:
- Building on Windows (x64, ARM64)
- Building on Ubuntu (x64, ARM64)
- Building on macOS (x64, ARM64)
- Automated testing on all platforms

### 4. Code Signing (Optional)
- Windows: Signtool.exe
- macOS: Xcode signing certificate
- Linux: GPG signatures

---

## Key Differences: WPF vs Avalonia

| Feature | WPF | Avalonia |
|---------|-----|----------|
| **Platform** | Windows only | Cross-platform |
| **XAML** | `http://schemas.microsoft.com/winfx/2006/xaml/presentation` | `https://github.com/avaloniaui` |
| **Target Framework** | `net10.0-windows` | `net10.0` |
| **File Dialogs** | Synchronous, Windows-only | Async, cross-platform |
| **Message Boxes** | `MessageBox.Show()` | `MessageBoxManager` |
| **Resource Protocol** | `pack://application:,,,/` | `avares://AssemblyName/` |
| **Control Access** | Automatic in XAML | Via `FindControl<T>` |
| **Animations** | WPF Animation classes | Direct value changes (auto-smoothed) |
| **Drag & Drop** | `AllowDrop` property | `DragDrop.DropEvent` registration |

---

## Testing Checklist

- [ ] Application launches without errors
- [ ] File picker dialog opens and works correctly
- [ ] Folder picker dialog opens and works correctly
- [ ] Drag and drop files works
- [ ] Extraction process completes successfully
- [ ] Progress bar updates smoothly
- [ ] Settings are saved and loaded correctly
- [ ] Checkboxes state persists between sessions
- [ ] Error messages display correctly
- [ ] Application works on Windows 10+
- [ ] Application works on Ubuntu 20.04+
- [ ] Application works on macOS 12.0+
- [ ] Builds successfully with `dotnet publish -c Release`

---

## Troubleshooting

### Build Error: "Could not find required Avalonia packages"
```
dotnet nuget update source nuget.org
dotnet clean
dotnet restore
dotnet build
```

### Runtime Error: "Could not load icon resource"
- Ensure icon.png is marked as `AvaloniaResource` in the .csproj
- Use correct resource protocol: `avares://RePKG.Neo/Res/icon.png`

### File Dialog Not Working
- File dialogs are async - must use `await`
- Make sure method is marked as `async void` or `async Task`

### CheckBox Binding Issues
- Avalonia requires property change notifications
- Ensure `INotifyPropertyChanged` is implemented
- Use proper binding syntax (TwoWay is default for CheckBox)

---

## References

- Avalonia Documentation: https://docs.avaloniaui.net/
- Avalonia Migration Guide: https://docs.avaloniaui.net/docs/getting-started/wpf-developer-guide
- Avalonia File Dialogs: https://docs.avaloniaui.net/docs/controls/file-dialogs
- Avalonia Data Binding: https://docs.avaloniaui.net/docs/basics/data-binding
