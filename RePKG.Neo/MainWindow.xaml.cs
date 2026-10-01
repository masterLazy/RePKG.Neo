/*
  Copyright 2025 masterLazy

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0
*/

using System.IO;
using System.Text.Json;
using LazyWpf;
using RePKG.Neo.res;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;

namespace RePKG.Neo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    private MainWindowVm DataCtx => (MainWindowVm)DataContext;
    private readonly MbService _mbService;
    private readonly WallpaperViewer _wallpaperViewer;

    private bool _isPopupAnimating = false;

    public MainWindow() {
        _mbService = new MbService(this);
        DataContext = new MainWindowVm(_mbService);

        InitializeComponent();

        _wallpaperViewer = new WallpaperViewer(WebView);
        _wallpaperViewer.Initialize();

        // Handle dropped files 
        if (App.DroppedFiles.Length <= 0) return;
        DataCtx.AddPath(App.DroppedFiles);
        if (DataCtx.Options.AutoExtract) DataCtx.StartExtract();
    }

    private async void Window_OnLoaded(object sender, RoutedEventArgs e) {
        // The right timing to add lister
        App.SingleInstance.IncomingFiles += paths => {
            DataCtx.AddPath(paths);
            if (DataCtx.Options.AutoExtract && DataCtx.CanStart) DataCtx.StartExtract();
        };
        // Show error msg
        if (App.ErrorMessage != null) {
            _mbService.ShowDialog($"{Lang.Msg_ErrorStartUp}\n\n{App.ErrorMessage}", Lang.Msg_Error, MbOpt.OK,
                MbBtn.None, MbIco.Error);
        }
        // Initialize webview
        int original = TabControl.SelectedIndex;
        TabControl.SelectedItem = TabViewer;
        TabControl.UpdateLayout();
        await Dispatcher.Yield(DispatcherPriority.Loaded);
        TabControl.SelectedIndex = original;
        WebView.CoreWebView2InitializationCompleted += async (_, e) => {
            if (!e.IsSuccess) return;
            DataCtx.IsWebviewReady = true;

            var core = WebView.CoreWebView2;
            await core.CallDevToolsProtocolMethodAsync("Runtime.enable", "{}");
            var runtimeReceiver = core.GetDevToolsProtocolEventReceiver("Runtime.consoleAPICalled");
            runtimeReceiver.DevToolsProtocolEventReceived += (_,  e) => {
                if (e.ParameterObjectAsJson == null) return;
                using var doc = JsonDocument.Parse(e.ParameterObjectAsJson);
                var root = doc.RootElement;
                string type = root.TryGetProperty("type", out var t) ? t.GetString() ?? "log" : "log";
                var parts = new List<string>();
                if (!root.TryGetProperty("args", out var args) || args.ValueKind != JsonValueKind.Array) return;
                foreach (var arg in args.EnumerateArray()) {
                    if (arg.TryGetProperty("value", out var v)) {
                        string part = (v.ValueKind == JsonValueKind.String ? v.GetString()! : v.GetRawText()).Trim();
                        parts.Add(part);
                    }
                }
                if (type == "error") Log.Error($"[WebView2] {string.Join("\n", parts)}");
                else if (type == "warning") Log.Warn($"[WebView2] {string.Join("\n", parts)}");
                else Log.Info($"[WebView2] {string.Join("\n", parts)}");
            };
        };
    }

    private void Window_StateChanged(object sender, EventArgs e) {
        switch (WindowState) {
            case WindowState.Maximized:
                double left = SystemParameters.ResizeFrameVerticalBorderWidth +
                              SystemParameters.FixedFrameVerticalBorderWidth + SystemParameters.BorderWidth;
                double top = SystemParameters.ResizeFrameHorizontalBorderHeight +
                             SystemParameters.FixedFrameHorizontalBorderHeight + SystemParameters.BorderWidth;
                LayoutRoot.Margin = new Thickness(left, top, left, top);
                break;
            default:
                LayoutRoot.Margin = new Thickness(0);
                break;
        }
    }

    private void Window_Closed(object sender, EventArgs e) {
        DataCtx.Options.Save();
    }

    private void BtnAddFile_Click(object sender, RoutedEventArgs e) {
        var dialog = new Microsoft.Win32.OpenFileDialog {
            DefaultExt = ".pkg",
            Filter = Lang.Msg_FileFilter,
            Multiselect = true
        };
        bool? result = dialog.ShowDialog();
        if (result != true) return;
        DataCtx.AddPath(dialog.FileNames);
        if (DataCtx.Options.AutoExtract) DataCtx.StartExtract();
    }

    private void BtnAddFolder_Click(object sender, RoutedEventArgs e) {
        Microsoft.Win32.OpenFolderDialog dialog = new() {
            Title = Lang.FolderDialog_Title,
            Multiselect = true
        };
        bool? result = dialog.ShowDialog();
        if (result != true) return;
        DataCtx.AddPath(dialog.FolderNames);
        if (DataCtx.Options.AutoExtract) DataCtx.StartExtract();
    }

    private void Border_Drop(object sender, DragEventArgs e) {
        if (DataCtx.IsRunning) {
            _mbService.ShowDialog(Lang.Msg_DropWhenRunning, Lang.Msg_Info, MbOpt.OK, icon: MbIco.Info);
            return;
        }
        if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (files == null) return;
            DataCtx.AddPath(files);
            if (DataCtx.Options.AutoExtract) DataCtx.StartExtract();
        } else {
            _mbService.ShowDialog(Lang.Msg_InvalidDrop, Lang.Msg_Info, MbOpt.OK, icon: MbIco.Info);
        }
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e) {
        DataCtx.ClearItems();
    }

    private void BtnStart_Click(object sender, RoutedEventArgs e) {
        DataCtx.StartExtract();
    }

    private void BtnStop_Click(object sender, RoutedEventArgs e) {
        DataCtx.Cancel();
    }

    private void BtnRemove_Click(object sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        if (btn.DataContext is not Item item) return;
        DataCtx.RemoveItem(item);
    }

    private void BtnReveal_Click(object sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        if (btn.DataContext is not Item item) return;
        if (item.SavePath != null) {
            System.Diagnostics.Process.Start("explorer.exe", $"\"{item.SavePath}\"");
        } else {
            System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{item.FilePath}\"");
        }
    }

    private void Button_OnClick(object sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        if (btn.DataContext is not Item item) return;
        _wallpaperViewer.View(item.FilePath);
        TabControl.SelectedItem = TabViewer;
    }

    private void BtnOptions_Click(object sender, RoutedEventArgs e) {
        if (_isPopupAnimating) return;
        if (PopupOptions.IsOpen) {
            ClosePopupWithAnimation();
        } else {
            OpenPopupWithAnimation();
        }
    }

    private void PopupOptions_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
        if (PopupOptions.IsOpen && !PopupOptions.IsMouseOver) {
            ClosePopupWithAnimation();
        }
    }

    private void OpenPopupWithAnimation() {
        if (_isPopupAnimating) return;
        _isPopupAnimating = true;
        PopupBorder.Opacity = 0;
        PopupOptions.IsOpen = true;
        var fadeIn = (Storyboard?)PopupOptions.Resources["FadeInStoryboard"];

        fadeIn?.Completed += Handler;
        fadeIn?.Begin(PopupBorder);
        return;

        void Handler(object? s, EventArgs _) {
            _isPopupAnimating = false;
            fadeIn.Completed -= Handler;
        }
    }

    private void ClosePopupWithAnimation() {
        if (_isPopupAnimating || !PopupOptions.IsOpen) return;
        _isPopupAnimating = true;
        var fadeOut = (Storyboard?)PopupOptions.Resources["FadeOutStoryboard"];

        fadeOut?.Completed += Handler;
        fadeOut?.Begin(PopupBorder);
        return;

        void Handler(object? s, EventArgs _) {
            PopupOptions.IsOpen = false;
            _isPopupAnimating = false;
            fadeOut.Completed -= Handler;
        }
    }

    private void BtnViewLog_Click(object sender, RoutedEventArgs e) {
        System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{Log.LogPath}\"");
    }

    private void RefreshButton_OnClick(object sender, RoutedEventArgs e) {
        _wallpaperViewer.Refresh();
    }
}