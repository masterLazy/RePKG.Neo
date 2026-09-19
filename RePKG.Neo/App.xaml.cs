/*
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
using LazyWpf;
using RePKG.Neo.res;

namespace RePKG.Neo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application {
    public static string[] DroppedFiles { get; private set; } = [];
    public static string? ErrorMessage { get; private set; }

    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
        DroppedFiles = e.Args;
        try {
            if (!Path.Exists(AppDataPath)) Directory.CreateDirectory(AppDataPath);
            Log.Init(AppDataPath);
            Log.StartupInfo();
        } catch (Exception ex) {
            ErrorMessage = ex.GetType().FullName + ": " + ex.Message;
            Log.Error($"Exception occurred during starting up: {Helper.ExceptionToString(ex)}");
        }
        AppDomain.CurrentDomain.UnhandledException += (_, args) => {
            Log.Fatal(Helper.ExceptionToString(args.ExceptionObject as Exception));
            int result = Helper.MessageBox(IntPtr.Zero, Lang.Msg_FatalError, Lang.Msg_Error, 0x00040014);
            if (result == 6) { // User clicked "Yes"
                System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{Log.LogPath}\"");
            }
        };
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