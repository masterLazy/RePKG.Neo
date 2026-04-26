# RePKG.Neo
<img src="https://raw.githubusercontent.com/masterLazy/RePKG.Neo/refs/heads/master/RePKG.Neo/Res/icon.png" width="162px" align="right"/>

此自述文件还有 [中文版](README_zh.md)。

An enhanced Wallpaper Engine PKG extractor and TEX converter, built upon the original [RePKG](https://github.com/notscuffed/repkg), **featuring a modern GUI** – supporting extracting images, sounds, videos, and more.

**Now cross-platform!** Built with [Avalonia](https://www.avaloniaui.net/) for Windows, macOS, and Linux.

<br/>

## New Features

> [!tip]
>
> To use RePKG.Neo, you need to install [.NET 10 Desktop Runtime](https://get.dot.net/10).

- **Cross-Platform Support**: Built with Avalonia, runs on Windows, macOS, and Linux with a single codebase.
- **Modern Graphical Interface (GUI)**: Clean and intuitive desktop application experience.
- **Upgraded to .NET 10**: Leverage the latest performance optimizations and framework features.
- **Flexible File Input Methods**:
  - Right-click and select "Open with RePKG.Neo"
  - Drag and drop files onto the program icon
  - Select files directly within the application
  - Drag and drop files or folders directly into the program window
  - **Smart Folder Recognition**: Automatically detects the `scene.pkg` file when a folder is dragged in

## Building from Source

### Prerequisites
- .NET 10.0 SDK or later: [Download from dot.net](https://get.dot.net/10)
- Git

### Clone and Build

```bash
git clone https://github.com/masterLazy/RePKG.Neo.git
cd RePKG.Neo

# Build
dotnet build -c Release

# Run
dotnet run --project RePKG.Neo/RePKG.Neo.csproj
```

### Platform-Specific Build Notes

**Windows:**
```bash
dotnet publish -c Release -r win-x64
# Output: RePKG.Neo/bin/Release/net10.0/win-x64/publish/
```

**macOS:**
```bash
dotnet publish -c Release -r osx-x64
# Output: RePKG.Neo/bin/Release/net10.0/osx-x64/publish/
```

**Linux:**
```bash
dotnet publish -c Release -r linux-x64
# Output: RePKG.Neo/bin/Release/net10.0/linux-x64/publish/
```

## Screenshot

![screenshot](https://raw.githubusercontent.com/masterLazy/RePKG.Neo/refs/heads/master/img/screenshot.webp)

> The following parts are from the original README of RePKG.
>
> The original license file of RePKG see LICENSE-RePKG.

---

Wallpaper engine PKG unpacker/TEX converter, written in C#.

PKG and TEX formats reverse engineered by me.

Feel free to report errors.

## Features
- Extract PKG files
- Convert PKG into wallpaper engine project
- Convert TEX to image
- Dump PKG/TEX info
