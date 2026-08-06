# RePKG.Neo
<img src="https://raw.githubusercontent.com/masterLazy/RePKG.Neo/refs/heads/master/RePKG.Neo/res/repkg-neo.png" width="162px" align="right"/>

[English](README.md) | 中文

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

一个功能增强的 Wallpaper Engine PKG 解包器与 TEX 转换器，基于原版 [RePKG](https://github.com/notscuffed/repkg) 构建，**并配备现代图形界面**——支持提取图像、音频、视频等多种资源。

<br/>

## 特性

> [!tip]
>
> 要使用 RePKG.Neo，你需要安装 [.NET 10 Desktop Runtime](https://get.dot.net/10)。
>
> 操作系统要求：>= Windows 10 1507

- **现代图形界面 (GUI)**：直观易用的操作界面，用户友好。
- **持续维护**：在 RePKG v0.4.0 的基础上持续修复漏洞。
- **批量处理**：支持同时添加多个文件/文件夹，一键按序批量提取。
- **多种文件输入方式**：
  - 双击 `.pkg` / `.mpkg` / `.tex` 文件打开
  - 右键菜单选择"用 RePKG.Neo 打开"
  - 拖拽文件到程序图标
  - 将文件或文件夹直接拖入程序窗口
  - 点击按钮选择文件或文件夹
  - **智能文件夹扫描**：递归扫描文件夹中的所有支持文件 (`.pkg`, `.mpkg`, `.tex`)
- **i18n**：支持英语、简体中文两种语言。

## 屏幕截图

![screenshot](https://raw.githubusercontent.com/masterLazy/RePKG.Neo/refs/heads/master/img/screenshot_zh.webp)

## 贡献指南

您可以[报告问题](https://github.com/masterLazy/RePKG.Neo/issues/new)或提交拉取请求，为本仓库提供支持。



> 下面的部分来自 RePKG 的原始 README。
>
> RePKG 的原始许可证请见 LICENSE-RePKG。
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
