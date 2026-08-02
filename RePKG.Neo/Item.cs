/**
   Copyright 2025 masterLazy

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0
 */
using CommunityToolkit.Mvvm.ComponentModel;
using LazyWpf;
using RePKG.Command;
using RePKG.Neo.res;
using System.IO;
using System.Windows.Media;

namespace RePKG.Neo {
    partial class Item : ObservableObject {
        public enum EState { Pending, Handling, Success, Fail }

        public string FilePath { get; private set; }
        public string FileDir { get; private set; }
        public string FileName { get; private set; }
        public string FileSize { get; private set; }
        public EState State { get; private set; } = EState.Pending;
        public string? SavePath { get; set; } = null;

        public string Thumb { get; private set; } = "/res/thumb.png";
        public string Title { get; private set; }
        public string DisplayDir { get; private set; }

        [ObservableProperty] string _text = "";
        [ObservableProperty] Brush? _textBrush = null;
        [ObservableProperty] int _percent = 0;

        private IProgress<int>? _progress;

        // This method believes that filePath always exsists
        public Item(string filePath) {
            // Basic infos
            FilePath = filePath;
            FileDir = Path.GetDirectoryName(filePath) ?? "";
            FileName = Path.GetFileName(filePath) ?? "";
            FileSize = Helper.ByteToString(new FileInfo(filePath).Length);
            // Infos associated with project.json
            var jsonPath = Path.Combine(FileDir, "project.json");
            if (File.Exists(jsonPath)) {
                var json = ProjectJson.ReadFrom(jsonPath);
                if (json == null) return;
                if (json.preview != null) Thumb = Path.Combine(FileDir, json.preview);
                if (json.title != null) Title = json.title;
            }
            // For display
            if (FileDir.Length < 40) DisplayDir = FileDir;
            else DisplayDir = FileDir[..19] + "…" + FileDir[^20..];
            DisplayDir += Path.DirectorySeparatorChar;
        }

        public async Task Extract(Options options) {
            if (!File.Exists(FilePath)) {
                State = EState.Fail;
                Text = Lang.Item_FileNotFound;
                TextBrush = System.Windows.Application.Current.FindResource("BhCritical") as SolidColorBrush;
                return;
            }

            //
            State = EState.Handling;
            Text = Lang.Item_Handling;
            TextBrush = System.Windows.Application.Current.FindResource("BhAttention") as SolidColorBrush;

            // Prepare
            bool isDryRunning = false;
            int totalWork = 0;
            _progress = new Progress<int>(work => {
                if (isDryRunning) {
                    totalWork = work;
                } else if (totalWork > 0) {
                    Percent = (int)Math.Round(100.0 * work / totalWork);
                    Text = $"{Lang.Item_Handling} ({work}/{totalWork})";
                }
            });

            SavePath = Path.Combine(FileDir, FileName + options.OutputSuffix);
            ExtractOptions extractOptions = new() {
                Input = FilePath,
                OutputDirectory = SavePath,
                Overwrite = true,
                NoTexConvert = options.NoTexConvert,
                NoRawTex = options.NoRawTex,
                CopyProject = options.CopyProject,
                SingleDir = options.SingleDir,
            };

            // Dry running
            isDryRunning = true;
            bool result = await TryExtractAsync(extractOptions, true);
            if (!result) {
                State = EState.Fail;
                Text = Lang.Item_ExtractFailed;
                TextBrush = System.Windows.Application.Current.FindResource("BhCritical") as SolidColorBrush;
                _progress = null;
                return;
            }

            // Start extractoion
            _progress.Report(0);
            isDryRunning = false;
            result = await TryExtractAsync(extractOptions);
            if (Helper.GetDirectorySize(SavePath) == 0) result = false;

            // Post logic
            if (result) {
                State = EState.Success;
                Text = Lang.Item_Complete;
                TextBrush = System.Windows.Application.Current.FindResource("BhSuccess") as SolidColorBrush;
            } else {
                State = EState.Fail;
                Text = Lang.Item_ExtractFailed;
                TextBrush = System.Windows.Application.Current.FindResource("BhCritical") as SolidColorBrush;
            }
            Percent = 100;
            _progress = null;
        }



        private async Task<bool> TryExtractAsync(ExtractOptions extractOptions, bool isDryRunning = false) {
            bool result = false;
            try {
                result = await Task.Run(() => Command.Extract.Action(extractOptions, _progress, isDryRunning));
            }
            catch (Exception ex) {
                var msg = string.Format(Lang.Msg_ExtractError, ex.Message);
                new MsgBox(msg, Lang.Msg_ExtractFailed_Title,
                    MbOpt.OK, MbIco.Error) { Owner = App.Current.MainWindow }.ShowDialog();
                Text = Lang.Item_ExtractFailed;
                TextBrush = System.Windows.Application.Current.FindResource("BhCritical") as SolidColorBrush;
                result = false;
            }
            return result;
        }
    }
}
