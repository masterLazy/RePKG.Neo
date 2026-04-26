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
using Avalonia.Media.Imaging;
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
            InitializeComponent();
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
                ShowError(string.Format(Lang.Msg_FileNotFoundContent, droppedFile),
                    Lang.Msg_FileNotFound);
            } else if (Options.AutoExtract) {
                DoExtract();
            }
        }

        // Open input file
        private async void BtnBrowseIn_Click(object? sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog {
                Title = Lang.Msg_FileFilter,
                AllowMultiple = false,
                Filters = new[] {
                    new FileDialogFilter { 
                        Name = "Package files", 
                        Extensions = new[] { "pkg", "mpkg", "tex" } 
                    },
                    new FileDialogFilter { 
                        Name = "All files", 
                        Extensions = new[] { "*" } 
                    }
                }
            };

            var result = await dialog.ShowAsync(this);
            if (result != null && result.Length > 0) {
                var tbInput = this.FindControl<TextBox>("TbInput");
                tbInput!.Text = result[0];
                MakeOutputDir();
            }
        }

        // Select output folder
        private async void BtnBrowseOut_Click(object? sender, RoutedEventArgs e) {
            var dialog = new OpenFolderDialog {
                Title = Lang.Msg_SelectOutputDir
            };

            var result = await dialog.ShowAsync(this);
            if (!string.IsNullOrEmpty(result)) {
                var tbOutput = this.FindControl<TextBox>("TbOutput");
                tbOutput!.Text = result;
            }
        }

        // Start extraction
        private async void BtnExtract_Click(object? sender, RoutedEventArgs e) {
            var tbInput = this.FindControl<TextBox>("TbInput");
            var tbOutput = this.FindControl<TextBox>("TbOutput");

            if (string.IsNullOrEmpty(tbInput?.Text)) {
                ShowInfo(Lang.Msg_SpecifyInput, Lang.Msg_Info);
                return;
            }

            if (Path.Exists(tbOutput?.Text)) {
                var confirmed = await ShowConfirmation(
                    string.Format(Lang.Msg_FolderExisted, tbOutput.Text),
                    Lang.Msg_Confirm);
                
                if (!confirmed) {
                    IsInputEnabled = true;
                    ShowInfo(Lang.Msg_CanceledContent, Lang.Msg_Canceled);
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
                ShowError(string.Format(Lang.Msg_ErrorContent, ex.Message), Lang.Msg_Error);
            }
            finally {
                IsInputEnabled = true;
                btnExtractText!.Text = Lang.Main_Extract;
                if (result) {
                    tipText!.Text = string.Format(Lang.Msg_Extracted, tbOutput.Text);
                } else {
                    ShowError(string.Format(Lang.Msg_FileNotFoundContent, tbInput.Text),
                        Lang.Msg_FileNotFound);
                }
            }
        }

        // Drop file onto window
        private void OnDrop(object? sender, DragEventArgs e) {
            if (e.Data.Contains(DataFormats.Files)) {
                var files = e.Data.GetFiles();
                if (files != null && files.Count > 0) {
                    var filePath = files[0].Path.LocalPath;
                    HandleDrop(filePath);
                    
                    if (files.Count > 1) {
                        ShowInfo(Lang.Msg_MultiDrop, Lang.Msg_Info);
                    }
                }
            } else {
                ShowError(Lang.Msg_InvalidDrop, Lang.Msg_Error);
            }
        }

        private void ShowInfo(string content, string title) {
            var msgBox = MessageBoxManager.GetMessageBoxStandard(title, content);
            msgBox.ShowAsync();
        }

        private void ShowError(string content, string title) {
            var msgBox = MessageBoxManager.GetMessageBoxStandard(title, content);
            msgBox.ShowAsync();
        }

        private async Task<bool> ShowConfirmation(string content, string title) {
            var msgBox = MessageBoxManager.GetMessageBoxStandard(title, content,
                MessageBoxButtons.YesNo);
            var result = await msgBox.ShowAsync();
            return result == ButtonResult.Yes;
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
