# RePKG.Neo
<img src="https://raw.githubusercontent.com/masterLazy/RePKG.Neo/refs/heads/master/RePKG.Neo/Res/icon.png" width="162px" align="right"/>

<p>
    <a href="https://github.com/masterLazy/RePKG.Neo/blob/master/LICENSE.txt"><img src="https://img.shields.io/github/license/masterLazy/RePKG.Neo"/></a>
  <a href="https://github.com/masterLazy/RePKG.Neo/releases"><img src="https://img.shields.io/github/downloads/masterLazy/RePKG.Neo/total" alt="Release Downloads"/></a>
  <a href="https://learn.microsoft.com/zh-cn/dotnet/core/whats-new/dotnet-10/overview/"><img src="https://img.shields.io/badge/dotnet-10.0-purple.svg?color=512bd4" alt="DotNet 10"/></a>
  <a href="#"><img src="https://img.shields.io/github/repo-size/masterLazy/RePKG.Neo" alt="GitHub Repo Size"/></a>
  <a href="https://github.com/masterLazy/RePKG.Neo/commits/"><img src="https://img.shields.io/github/last-commit/masterLazy/RePKG.Neo" alt="Last Commit"/></a>
  <a href="https://github.com/masterLazy/RePKG.Neo/issues"><img src="https://img.shields.io/github/issues/masterLazy/RePKG.Neo" alt="Issues"/></a>
  <a href="https://github.com/masterLazy/RePKG.Neo/releases"><img src="https://img.shields.io/github/v/release/masterLazy/RePKG.Neo" alt="Latest Version"/></a>
  <a href="https://github.com/masterLazy/RePKG.Neo/releases"><img src="https://img.shields.io/github/release-date/masterLazy/RePKG.Neo" alt="Release Date"/></a>
  <a href="https://github.com/masterLazy/RePKG.Neo/commits/"><img src="https://img.shields.io/github/commit-activity/m/masterLazy/RePKG.Neo" alt="Commit Activity"/></a>
  <a href="https://deepwiki.com/masterLazy/RePKG.Neo"><img src="https://deepwiki.com/badge.svg" alt="Ask DeepWiki"/></a>
</p>

一个功能增强的的 Wallpaper Engine PKG 解包器与 TEX 转换器，基于原版 [RePKG](https://github.com/notscuffed/repkg) 构建，**并配备现代图形界面**——支持提取图像、音频、视频等多种资源。

<br/>

## 新功能

> [!tip]
>
> 要使用 RePKG.Neo，你需要安装 [.NET 10 Desktop Runtime](https://get.dot.net/10)。

- **现代化的图形界面 (GUI)**：使用 WPF + MVVM 架构构建，提供了直观易用的桌面应用程序体验。
- **升级至 .NET 10**：享受最新的性能优化和框架特性。
- **批量处理**：支持同时添加多个文件/文件夹，一键按序批量提取。
- **逐项进度与状态**：每个文件独立显示进度条、状态（等待中/成功/失败）和预览缩略图。
- **灵活的文件输入方式**：
  - 右键菜单选择"用 RePKG.Neo 打开"
  - 拖拽文件到程序图标
  - 在程序内直接选择文件或文件夹（支持多选）
  - 将文件或文件夹直接拖入程序窗口
  - **智能文件夹扫描**：递归扫描文件夹中的所有支持文件 (.pkg, .mpkg, .tex)
- **文件操作**：可单独移除列表中的项目，或一键在文件资源管理器中定位提取结果。

## 屏幕截图

![screenshot](https://raw.githubusercontent.com/masterLazy/RePKG.Neo/refs/heads/master/img/screenshot_zh.webp)

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
