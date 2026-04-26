/**
   Copyright 2025 masterLazy

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0
 */
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using RePKG.Command;
using RePKG.Neo.Res;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace RePKG.Neo {
    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Options _options = new();
        public Options Options {
            get { return _options; }
            set {
                if (_options == value) return;
                _options = value;
                OnPropertyChanged(nameof(Options));
            }
        }

        private bool _isInputEnabled = true;
        public bool IsInputEnabled {
            get { return _isInputEnabled; }
            set {
                if (value == _isInputEnabled) return;
                _isInputEnabled = value;
                OnPropertyChanged(nameof(IsInputEnabled));
            }
        }

        public MainWindow() {
            AvaloniaXamlLoader.Load(this);
            DataContext = this;
            Options = Options.Load() ?? new();
            
            // Setup drag and drop
            AddHandler(DragDrop.DropEvent, OnDrop);
            
            HandleDrop(App.droppedFile);
        }

        private void MakeOutputDir() {
            var tbInput = this.FindControl<TextBox>("TbInput");
            var tbOutput = this.FindControl<TextBox>("TbOutput");
            
            if (tbInput?.Text != null) {
                int i = tbInput.Text.LastIndexOf('.');
                if (i > 0) {
                    tbOutput!.Text = string.Concat(tbInput.Text.AsSpan(0, i), "-repkg\\");
                }
            }
        }

        private void HandleDrop(string? droppedFile) {
            if (string.IsNullOrEmpty(droppedFile)) return;

            var tbInput = this.FindControl<TextBox>("TbInput");
            var tbOutput = this.FindControl<TextBox>("TbOutput");

            if (!File.Exists(droppedFile)) {
                droppedFile += "\\scene.pkg";
            }
            
            tbInput!.Text = droppedFile;
            MakeOutputDir();
            
            if (!File.Exists(droppedFile)) {
                ShowErrorAsync(string.Format(Lang.Msg_FileNotFoundContent, droppedFile),
                    Lang.Msg_FileNotFound);
            } else if (Options.AutoExtract) {
                DoExtract();
            }
        }

        // Open input file
        private async void BtnBrowseIn_Click(object? sender, RoutedEventArgs e) {
            var topLevel = GetTopLevel(this);
            if (topLevel?.StorageProvider == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                Title = "Select Package File",
                AllowMultiple = false,
                FileTypeFilter = new[] {
                    new FilePickerFileType("Package Files") { 
                        Patterns = new[] { "*.pkg", "*.mpkg", "*.tex" } 
                    },
                    new FilePickerFileType("All Files") { 
                        Patterns = new[] { "*" } 
                    }
                }
            });

            if (files.Count > 0) {
                var tbInput = this.FindControl<TextBox>("TbInput");
                tbInput!.Text = files[0].Path.LocalPath;
                MakeOutputDir();
            }
        }

        // Select output folder
        private async void BtnBrowseOut_Click(object? sender, RoutedEventArgs e) {
            var topLevel = GetTopLevel(this);
            if (topLevel?.StorageProvider == null) return;

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
                Title = "Select Output Directory",
                AllowMultiple = false
            });

            if (folders.Count > 0) {
                var tbOutput = this.FindControl<TextBox>("TbOutput");
                tbOutput!.Text = folders[0].Path.LocalPath;
            }
        }

        // Start extraction
        private async void BtnExtract_Click(object? sender, RoutedEventArgs e) {
            var tbInput = this.FindControl<TextBox>("TbInput");
            var tbOutput = this.FindControl<TextBox>("TbOutput");

            if (string.IsNullOrEmpty(tbInput?.Text)) {
                await ShowInfoAsync(Lang.Msg_SpecifyInput, Lang.Msg_Info);
                return;
            }

            if (Path.Exists(tbOutput?.Text)) {
                var confirmed = await ShowConfirmationAsync(
                    string.Format(Lang.Msg_FolderExisted, tbOutput.Text),
                    Lang.Msg_Confirm);
                
                if (!confirmed) {
                    IsInputEnabled = true;
                    await ShowInfoAsync(Lang.Msg_CanceledContent, Lang.Msg_Canceled);
                    return;
                }
            }

            DoExtract();
        }

        private async void DoExtract() {
            var tbInput = this.FindControl<TextBox>("TbInput");
            var tbOutput = this.FindControl<TextBox>("TbOutput");
            var progressBar = this.FindControl<ProgressBar>("ProgressBar");
            var btnExtractText = this.FindControl<TextBlock>("BtnExtractText");
            var tipText = this.FindControl<TextBlock>("Text_Tip");

            IsInputEnabled = false;
            btnExtractText!.Text = Lang.Main_Extracting;

            var progress = new Progress<double>(percent => {
                if (progressBar != null) {
                    progressBar.Value = percent * 100;
                }
            });

            ExtractOptions extractOptions = new() {
                Input = tbInput!.Text,
                OutputDirectory = tbOutput!.Text,
                Overwrite = true,
                NoTexConvert = Options.NoTexConvert,
                CopyProject = Options.CopyProject,
            };

            bool result = false;
            try {
                result = await Task.Run(() => Extract.Action(extractOptions, progress));
            }
            catch (Exception ex) {
                await ShowErrorAsync(string.Format(Lang.Msg_ErrorContent, ex.Message), Lang.Msg_Error);
            }
            finally {
                IsInputEnabled = true;
                btnExtractText!.Text = Lang.Main_Extract;
                if (result) {
                    tipText!.Text = string.Format(Lang.Msg_Extracted, tbOutput.Text);
                } else {
                    await ShowErrorAsync(string.Format(Lang.Msg_FileNotFoundContent, tbInput.Text),
                        Lang.Msg_FileNotFound);
                }
            }
        }

        // Drop file onto window
        private void OnDrop(object? sender, DragEventArgs e) {
            if (e.Data.Contains(DataFormats.Files)) {
                var files = e.Data.GetFiles();
                if (files != null) {
                    var fileList = files.ToList();
                    if (fileList.Count > 0) {
                        var filePath = fileList[0].Path.LocalPath;
                        HandleDrop(filePath);
                        
                        if (fileList.Count > 1) {
                            ShowInfoAsync(Lang.Msg_MultiDrop, Lang.Msg_Info);
                        }
                    }
                }
            } else {
                ShowErrorAsync(Lang.Msg_InvalidDrop, Lang.Msg_Error);
            }
        }

        private async Task ShowInfoAsync(string content, string title) {
            await Task.Delay(100); // Brief delay to ensure window is ready
            System.Diagnostics.Debug.WriteLine($"{title}: {content}");
        }

        private async Task ShowErrorAsync(string content, string title) {
            await Task.Delay(100);
            System.Diagnostics.Debug.WriteLine($"ERROR - {title}: {content}");
        }

        private async Task<bool> ShowConfirmationAsync(string content, string title) {
            // For now, always return true (default behavior)
            // In production, would show actual dialog
            await Task.Delay(100);
            System.Diagnostics.Debug.WriteLine($"CONFIRM - {title}: {content}");
            return true;
        }

        public static void ChangeLanguage(string culture) {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        }

        protected override void OnClosing(WindowClosingEventArgs e) {
            Options.Save();
            base.OnClosing(e);
        }
    }
}
