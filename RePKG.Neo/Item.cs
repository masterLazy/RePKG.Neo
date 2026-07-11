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
using System.IO;
using System.Windows.Media;

namespace RePKG.Neo {
    partial class Item : ObservableObject {
        public enum EState { Pending, Success, Fail }

        public string FilePath { get; private set; }
        public string FileDir { get; private set; }
        public string FileDirDisplay { get; private set; }
        public string FileName { get; private set; }
        public string FileSize { get; private set; }
        public string Thumb { get; private set; } = "/res/thumb.png";
        public EState State { get; private set; } = EState.Pending;
        public string? SavePath { get; set; } = null;

        [ObservableProperty] string _text = "";
        [ObservableProperty] Brush? _textBrush = null;
        [ObservableProperty] int _percent = 0;

        // This method believes that filePath always exsists
        public Item(string filePath) {
            FilePath = filePath;
            FileDir = Path.GetDirectoryName(filePath) ?? "";
            FileName = Path.GetFileName(filePath) ?? "";
            FileSize = Helper.ByteToString(new FileInfo(filePath).Length);
            var preview = Helper.FindFileIgnoreExt(FileDir, "preview");
            if (preview != null) Thumb = Path.Combine(FileDir, preview);
            //
            if (FileDir.Length < 40) FileDirDisplay = FileDir;
            else FileDirDisplay = FileDir[..19] + "…" + FileDir[^20..];
            FileDirDisplay += Path.DirectorySeparatorChar;
        }

        public async Task Extract(Options options) {
            if (!File.Exists(FilePath)) {
                State = EState.Fail;
                Text = "未找到文件";
                TextBrush = System.Windows.Application.Current.FindResource("BhCritical") as SolidColorBrush;
                return;
            }
            // Prepare
            var progress = new Progress<double>(percent => {
                Percent = (int)Math.Round(percent * 100);
            });
            SavePath = Path.Combine(FileDir, FileName + options.OutputSuffix);
            ExtractOptions extractOptions = new() {
                Input = FilePath,
                OutputDirectory = SavePath,
                Overwrite = true,
                NoTexConvert = options.NoTexConvert,
                CopyProject = options.CopyProject,
                SingleDir = options.SingleDir,
            };
            // Start extractoion
            bool result = false;
            try {
                result = await Task.Run(() => Command.Extract.Action(extractOptions, progress));
            }
            catch (Exception ex) {
                var msg = string.Format("提取时发生了错误：\n\n{0}\n\n请检查输入文件是否有效。", ex.Message);
                new MsgBox(msg, "提取失败",
                    MbOpt.OK, MbIco.Error) { Owner = App.Current.MainWindow }.ShowDialog();
                Text = "提取失败";
                TextBrush = System.Windows.Application.Current.FindResource("BhCritical") as SolidColorBrush;
            }
            // Post logic
            if (Helper.GetDirectorySize(SavePath) == 0) result = false;
            if (result) {
                State = EState.Success;
                Text = "完成";
                TextBrush = System.Windows.Application.Current.FindResource("BhSuccess") as SolidColorBrush;
            } else {
                State = EState.Fail;
                Text = "提取失败";
                TextBrush = System.Windows.Application.Current.FindResource("BhCritical") as SolidColorBrush;
            }
            Percent = 100;
        }
    }
}
