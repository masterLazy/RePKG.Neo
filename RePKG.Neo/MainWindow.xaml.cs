/**
   Copyright 2025 masterLazy

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0
 */
using RePKG.Command;
using System.Windows;
using System.IO;
using System.ComponentModel;

namespace RePKG.Neo {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            HandleDrop(App.droppedFile);
        }

        private void MakeOutputDir() {
            int i = TbInput.Text.LastIndexOf('.');
            TbOutput.Text = string.Concat(TbInput.Text.AsSpan(0, i), "-repkg\\");
        }

        private void HandleDrop(string droppedFile) {
            if (!string.IsNullOrEmpty(droppedFile)) {
                if (!File.Exists(droppedFile)) {
                    droppedFile += "\\scene.pkg";
                }
                TbInput.Text = droppedFile;
                MakeOutputDir();
                if (!File.Exists(droppedFile)) {
                    MessageBox.Show(string.Format(Res.Lang.Msg_FileNotFoundContent, droppedFile),
                        Res.Lang.Msg_FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Open input file
        private void BtnBrowseIn_Click(object sender, RoutedEventArgs e) {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "scene";
            dialog.DefaultExt = ".pkg";
            dialog.Filter = Res.Lang.Msg_FileFilter;
            bool? result = dialog.ShowDialog();
            if (result == true) {
                TbInput.Text = dialog.FileName;
                MakeOutputDir();
            }
        }

        // Select output folder
        private void BtnBrowseOut_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFolderDialog dialog = new();
            dialog.Multiselect = false;
            dialog.Title = Res.Lang.Msg_SelectOutputDir;
            bool? result = dialog.ShowDialog();
            if (result == true) {
                TbOutput.Text = dialog.FolderName;
            }
        }

        // Start extraction
        private void BtnExtract_Click(object sender, RoutedEventArgs e) {
            IsInputEnabled = false;
            if (string.IsNullOrEmpty(TbInput.Text)) {
                MessageBox.Show(Res.Lang.Msg_SpecifyInput,
                        Res.Lang.Msg_Info, MessageBoxButton.OK, MessageBoxImage.Information);
                IsInputEnabled = true;
                return;
            }
            if (Path.Exists(TbOutput.Text)) {
                if (MessageBox.Show(string.Format(Res.Lang.Msg_FolderExisted, TbOutput.Text),
                    Res.Lang.Msg_Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) {
                    IsInputEnabled = true;
                    MessageBox.Show(Res.Lang.Msg_CanceledContent,
                    Res.Lang.Msg_Canceled, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            // Do extract
            ExtractOptions extractOptions = new() {
                Input = TbInput.Text,
                OutputDirectory = TbOutput.Text,
                Overwrite = true,
                NoTexConvert = CbNoCvt.IsChecked == true,
                CopyProject = CbCopy.IsChecked == true,
            };
            try {
                var result = Extract.Action(extractOptions);
                // Info
                if (result) {
                    MessageBox.Show(string.Format(Res.Lang.Msg_Extracted, TbOutput.Text),
                        Res.Lang.Msg_Success, MessageBoxButton.OK, MessageBoxImage.Information);
                } else {
                    MessageBox.Show(string.Format(Res.Lang.Msg_FileNotFoundContent, TbInput.Text),
                        Res.Lang.Msg_FileNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex) {
                MessageBox.Show(string.Format(Res.Lang.Msg_ErrorContent, ex.Message),
                    Res.Lang.Msg_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally {
                IsInputEnabled = true;
            }
        }

        // Drop file onto window
        private void root_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                HandleDrop(files[0]);
                if (files.Length > 1) {
                    MessageBox.Show(Res.Lang.Msg_MultiDrop,
                            Res.Lang.Msg_Info, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            } else {
                MessageBox.Show(Res.Lang.Msg_InvalidDrop,
                    Res.Lang.Msg_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}