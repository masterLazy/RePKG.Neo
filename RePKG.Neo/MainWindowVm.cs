/*
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

namespace RePKG.Neo;

internal partial class MainWindowVm : ObservableObject {
    [ObservableProperty] private ObservableCollection<Item> _items = [];
    [ObservableProperty] private Options _options = Options.Load() ?? new Options();

    [ObservableProperty] private bool _canStart = false;
    [ObservableProperty] private bool _hasItem = false;
    [ObservableProperty] private bool _isRunning = false;
    [ObservableProperty] private bool _notRunning = true;

    private readonly HashSet<string> _itemPaths = [];
    private CancellationTokenSource? _cts;

    private readonly MbService _mbService;

    // Property update logic

    partial void OnIsRunningChanged(bool value) {
        NotRunning = !value;
        CanStart = !IsRunning && HasItem;
    }

    public MainWindowVm(MbService mbService) {
        _mbService = mbService;
        Items.CollectionChanged += (sender, e) => {
            HasItem = Items.Count > 0;
            CanStart = !IsRunning && HasItem;
        };
    }

    // Main logic


    private readonly string[] _supportedExts = [".pkg", ".mpkg", ".tex"];

    public void AddPath(string[] paths) {
        foreach (string path in paths) AddPath(path);
    }

    public void AddPath(string path) {
        bool isDir = File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        if (isDir) {
            AddPath(Directory.GetFiles(path));
            AddPath(Directory.GetDirectories(path));
        }
        else {
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

    public void StartExtract() {
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        StartExtract(_cts.Token);
    }

    public void Cancel() {
        _cts?.Cancel();
    }

    private async void StartExtract(CancellationToken token) {
        var retrySuccess = false;
        if (HasSuccess(token)) {
            MbBtn result = _mbService.ShowDialog(Lang.Msg_ExtractSucceeded, Lang.Msg_Confirm, MbOpt.YesNo,
                icon: MbIco.Info);
            if (result == MbBtn.Yes) retrySuccess = true;
            else if (result == MbBtn.No) retrySuccess = false;
            else return;
        }
        IsRunning = true;
        foreach (Item item in Items) {
            if (token.IsCancellationRequested) break;
            if (item.State == Item.EState.Success && !retrySuccess) continue;
            await item.Extract(Options, _mbService, token);
        }
        IsRunning = false;

        _cts?.Dispose();
        _cts = null;
    }

    private bool HasSuccess(CancellationToken token) {
        foreach (Item item in Items) {
            if (token.IsCancellationRequested) break;
            if (item.State == Item.EState.Success) return true;
        }
        return false;
    }
}