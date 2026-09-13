/**
   Copyright 2025 masterLazy

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0
 */

using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Windows;

namespace RePKG.Neo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application {
    public static string[] DroppedFiles { get; set; }

    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
        DroppedFiles = e.Args;
        if (!Path.Exists(AppDataPath)) Directory.CreateDirectory(AppDataPath);
        Log.Init(AppDataPath);
        Log.StartupInfo();
    }

    // "User/.../AppData/Roaming/RePKG.Neo/"
    public static readonly string AppDataPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RePKG.Neo");

    // Json Options
    public static readonly JsonSerializerOptions JsonOptions = new() {
        // Serialization
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs),
        // Deserialization
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement
    };
}