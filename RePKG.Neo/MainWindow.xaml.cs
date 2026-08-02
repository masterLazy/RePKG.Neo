/**
  Copyright 2025 masterLazy

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0
*/
using LazyWpf;
using RePKG.Neo.res;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace RePKG.Neo {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private MainWindowVM DataCtx => (MainWindowVM)DataContext;
        private bool _isPopupAnimating = false;
        private readonly MbService _mbService;

        public MainWindow() {
            _mbService = new MbService(this);
            DataContext = new MainWindowVM(_mbService);
            InitializeComponent();
            if (App.DroppedFiles.Length > 0) {
                DataCtx.AddPath(App.DroppedFiles);
                if (DataCtx.Options.AutoExtract) DataCtx.StartExtract();
            }
        }

        private void Window_StateChanged(object sender, EventArgs e) {
            switch (WindowState) {
                case WindowState.Maximized:
                    double left = SystemParameters.ResizeFrameVerticalBorderWidth + SystemParameters.FixedFrameVerticalBorderWidth + SystemParameters.BorderWidth;
                    double top = SystemParameters.ResizeFrameHorizontalBorderHeight + SystemParameters.FixedFrameHorizontalBorderHeight + SystemParameters.BorderWidth;
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
                Multiselect = true,
            };
            bool? result = dialog.ShowDialog();
            if (result == true) {
                DataCtx.AddPath(dialog.FileNames);
                if (DataCtx.Options.AutoExtract) DataCtx.StartExtract();
            }
        }

        private void BtnAddFolder_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFolderDialog dialog = new() {
                Title = Lang.FolderDialog_Title,
                Multiselect = true
            };
            bool? result = dialog.ShowDialog();
            if (result == true) {
                DataCtx.AddPath(dialog.FolderNames);
                if (DataCtx.Options.AutoExtract) DataCtx.StartExtract();
            }
        }

        private void Border_Drop(object sender, DragEventArgs e) {
            if (DataCtx.IsRunning) {
                _mbService.ShowDialog(Lang.Msg_DropWhenRunning, Lang.Msg_Info, option: MbOpt.OK, icon: MbIco.Info);
                return;
            }
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                DataCtx.AddPath(files);
                if (DataCtx.Options.AutoExtract) DataCtx.StartExtract();
            } else {
                _mbService.ShowDialog(Lang.Msg_InvalidDrop, Lang.Msg_Info, option: MbOpt.OK, icon: MbIco.Info);
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
            if (sender is not LazyWpf.Button btn) return;
            if (btn.DataContext is not Item item) return;
            DataCtx.RemoveItem(item);
        }

        private void BtnReveal_Click(object sender, RoutedEventArgs e) {
            if (sender is not LazyWpf.Button btn) return;
            if (btn.DataContext is not Item item) return;
            if (item.SavePath != null) {
                System.Diagnostics.Process.Start("explorer.exe", $"\"{item.SavePath}\"");
            } else {
                System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{item.FilePath}\"");
            }
        }

        private void BtnOptions_Click(object sender, RoutedEventArgs e) {
            if (_isPopupAnimating) return;
            if (PopupOptions.IsOpen)
                ClosePopupWithAnimation();
            else
                OpenPopupWithAnimation();
        }

        private void PopupOptions_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            if (PopupOptions.IsOpen && !PopupOptions.IsMouseOver)
                ClosePopupWithAnimation();
        }

        private void Window_Activated(object sender, EventArgs e) {
            LayoutRoot.BorderBrush = FindResource("BhPrimary") as SolidColorBrush;
        }

        private void Window_Deactivated(object sender, EventArgs e) {
            LayoutRoot.BorderBrush = new SolidColorBrush(Color.FromRgb(0x87, 0x87, 0x87));
        }

        private void OpenPopupWithAnimation() {
            if (_isPopupAnimating) return;
            _isPopupAnimating = true;
            PopupBorder.Opacity = 0;
            PopupOptions.IsOpen = true;
            var fadeIn = (Storyboard)PopupOptions.Resources["FadeInStoryboard"];
            void handler(object? s, EventArgs _) {
                _isPopupAnimating = false;
                fadeIn.Completed -= handler;
            }
            fadeIn.Completed += handler;
            fadeIn.Begin(PopupBorder);
        }

        private void ClosePopupWithAnimation() {
            if (_isPopupAnimating || !PopupOptions.IsOpen) return;
            _isPopupAnimating = true;
            var fadeOut = (Storyboard)PopupOptions.Resources["FadeOutStoryboard"];
            void handler(object? s, EventArgs _) {
                PopupOptions.IsOpen = false;
                _isPopupAnimating = false;
                fadeOut.Completed -= handler;
            }
            fadeOut.Completed += handler;
            fadeOut.Begin(PopupBorder);
        }

        private void BtnViewLog_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{Log.LogPath}\"");
        }
    }
}