# RePKG.Neo
<img src="https://raw.githubusercontent.com/masterLazy/RePKG.Neo/refs/heads/master/RePKG.Neo/res/repkg-neo.png" width="162px" align="right"/>

English | [中文](README_zh.md)

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

An enhanced Wallpaper Engine PKG unpacker and TEX converter, built upon the original [RePKG](https://github.com/notscuffed/repkg), **featuring a modern graphical interface** — supports extracting images, audio, videos, and other resources.

<br/>

## Features

> [!tip]
>
> To use RePKG.Neo, you need to install [.NET 10 Desktop Runtime](https://get.dot.net/10).
>
> OS requirements: >= Windows 10 1507

- **Modern Graphical Interface (GUI)**: Intuitive and user-friendly interface.
- **Actively Maintained**: Continuous bug fixes based on RePKG v0.4.0.
- **Batch Processing**: Supports adding multiple files/folders at once, with one-click sequential batch extraction.
- **Multiple File Input Methods**:
  - Double-click `.pkg` / `.mpkg` / `.tex` files to open
  - Right-click menu "Open with RePKG.Neo"
  - Drag and drop files onto the program icon
  - Drag files or folders directly into the program window
  - Click buttons to select files or folders
  - **Smart Folder Scanning**: Recursively scans folders for all supported files (`.pkg`, `.mpkg`, `.tex`)
- **i18n**: Supports both English and Simplified Chinese.

## Screenshots

![screenshot](https://raw.githubusercontent.com/masterLazy/RePKG.Neo/refs/heads/master/img/screenshot.webp)

## Contributing

You can [report a bug](https://github.com/masterLazy/RePKG.Neo/issues/new) or open a pull request to support this repo.



> The section below is from the original README of RePKG.
>
> See LICENSE-RePKG for the original RePKG license.
> 
> ---
> 
> Wallpaper engine PKG unpacker/TEX converter, written in C#.
> 
> PKG and TEX formats reverse engineered by me.
> 
> Feel free to report errors.
> 
> ## Features
> - Extract PKG files
> - Convert PKG into wallpaper engine project
> - Convert TEX to image
> - Dump PKG/TEX info