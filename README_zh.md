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

---

Wallpaper engine PKG unpacker/TEX converter, written in C#.

PKG and TEX formats reverse engineered by me.

Feel free to report errors.

## Features
- Extract PKG files
- Convert PKG into wallpaper engine project
- Convert TEX to image
- Dump PKG/TEX info

### Commands
- help - shows those commands, use `help "extract"` and `help "info"` to see options for them
- extract - extracts specified PKG/TEX file, or files from folder
```
-o, --output          (Default: ./output) Output directory
-i, --ignoreexts      Don't extract files with specified extensions (delimited by comma ",")
-e, --onlyexts        Only extract files with specified extensions (delimited by comma ",")
-d, --debuginfo       Print debug info while extracting/decompiling
-t, --tex             Convert all TEX files into images from specified directory in input
-s, --singledir       Should all extracted files be put in one directory instead of their entry path
-r, --recursive       Recursive search in all subfolders of specified directory
-c, --copyproject     Copy project.json and preview.jpg from beside PKG into output directory
-n, --usename         Use name from project.json as project subfolder name instead of id
--no-tex-convert      Don't convert TEX files into images while extracting PKG
--overwrite           Overwrite all existing files
```
- info - Dumps PKG/TEX info
```
-s, --sort             Sort entries a-z
-b, --sortby           (Default: name) Sort by ... (available options: name, extension, size)
-t, --tex              Dump info about all TEX files from specified directory
-p, --projectinfo      Keys to dump from project.json (delimit using comma) (* for all)
-e, --printentries     Print entries in packages
--title-filter         Title filter
```

### Examples
Simply extract PKG and convert TEX entries into images to output folder created in current directory
```
repkg extract E:\Games\steamapps\workshop\content\123\scene.pkg
```
Find PKG files in subfolders of specified directory and make wallpaper engine projects out of them in output directory
```
repkg extract -c E:\Games\steamapps\workshop\content\123
```
Find PKG files in subfolders of specified directory and only convert TEX entries to png then put them in ./output omitting their paths from PKG:
```
repkg extract -e tex -s -o ./output E:\Games\steamapps\workshop\content\123
```
Convert all TEX files to images from specific folder
```
repkg extract -t -s E:\path\to\dir\with\tex\files
```