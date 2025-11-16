# RePKG.Neo
<img src="RePKG.Neo/Resources/icon.png" width="162px" align="right"/>

一个功能增强的的 Wallpaper Engine PKG 解包器与 TEX 转换器，基于原版 [RePKG](https://github.com/notscuffed/repkg) 构建，**并配备现代图形界面**——支持提取图像、音频、视频等多种资源。

<br/>

## 新功能

> [!tip]
>
> 要使用 RePKG.Neo，你需要安装 [.NET 10 Desktop Runtime](https://get.dot.net/10)。

- **现代化的图形界面 (GUI)**：使用 WPF 构建，提供了直观易用的桌面应用程序体验。
- **升级至 .NET 10**：享受最新的性能优化和框架特性。
- **灵活的文件输入方式**：
  - 右键菜单选择“用 RePKG.Neo 打开”
  - 拖拽文件到程序图标
  - 在程序内直接选择文件
  - 将文件或文件夹直接拖入程序窗口
  - **智能文件夹识别**：拖入文件夹时自动识别其中的 `scene.pkg` 文件

> 下面的部分来自 RePKG 的原始 README。
>
> RePKG 的原始许可证请见 LICENSE-RePKG。

---

Wallpaper engine PKG unpacker/TEX converter, written in C#.

PKG and TEX formats reverse engineered by me.

Feel free to report errors.

## Features
- Extract PKG files
- Convert PKG into wallpaper engine project
- Convert TEX to image
- Dump PKG/TEX info
