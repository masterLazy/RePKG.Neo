/**
   Copyright 2025 masterLazy

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0
 */
using CommunityToolkit.Mvvm.ComponentModel;
using LazyWpf;
using RePKG.Neo.res;
using System.Collections.ObjectModel;
using System.IO;

namespace RePKG.Neo {
    partial class MainWindowVM : ObservableObject {
        [ObservableProperty] ObservableCollection<Item> _items = [];
        [ObservableProperty] Options _options = Options.Load() ?? new();

        [ObservableProperty] bool _canStart = false;
        [ObservableProperty] bool _hasItem = false;
        [ObservableProperty] bool _isRunning = false;
        [ObservableProperty] bool _notRunning = true;

        private HashSet<string> _itemPaths = [];
        public bool Stopping { get; set; }

        private readonly MbService _mbService;

        // Property update logic

        partial void OnIsRunningChanged(bool value) {
            NotRunning = !value;
            CanStart = !IsRunning && HasItem;
        }

        public MainWindowVM(MbService mbService) {
            _mbService = mbService;
            Items.CollectionChanged += (sender, e) => {
                HasItem = Items.Count > 0;
                CanStart = !IsRunning && HasItem;
            };
        }

        // Main logic


        private string[] _supportedExts = { ".pkg", ".mpkg", ".tex" };

        public void AddPath(string[] paths) {
            foreach (string path in paths) {
                AddPath(path);
            }
        }

        public void AddPath(string path) {
            bool isDir = File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            if (isDir) {
                AddPath(Directory.GetFiles(path));
                AddPath(Directory.GetDirectories(path));
            } else {
                if (!_supportedExts.Contains(Path.GetExtension(path).ToLower())) return;
                if (_itemPaths.Contains(path.ToLower())) return;
                Items.Add(new Item(path));
                _itemPaths.Add(path.ToLower());
            }
        }

        public void RemoveItem(Item item) {
            Items.Remove(item);
            _itemPaths.Remove(item.FilePath.ToLower());
        }

        public void ClearItems() {
            Items.Clear();
            _itemPaths.Clear();
        }

        public async void StartExtract() {
            bool retrySuccess = false;
            if (HasSuccess()) {
                var result = _mbService.ShowDialog(Lang.Msg_ExtractSucceeded, Lang.Msg_Confirm, option: MbOpt.YesNo, icon: MbIco.Info);
                if (result == MbBtn.Yes) retrySuccess = true;
                else if (result == MbBtn.No) retrySuccess = false;
                else return;
            }
            IsRunning = true;
            Stopping = false;
            foreach (var item in Items) {
                if (Stopping) break;
                if (item.State == Item.EState.Success && !retrySuccess) continue;
                await item.Extract(Options, _mbService);
            }
            IsRunning = false;
            Stopping = false;
        }

        public bool HasSuccess() {
            foreach (var item in Items) {
                if (Stopping) break;
                if (item.State == Item.EState.Success) return true;
            }
            return false;
        }
    }
}
