/*
   Copyright 2025 masterLazy

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0
 */

using System.IO;
using System.Text.Json;

namespace RePKG.Neo;

public class Options {
    public const string CurrentVersion = "v1";
    public string Version { get; init; } = CurrentVersion;

    public bool NoTexConvert { get; set; } = false;
    public bool NoRawTex { get; set; } = true;
    public bool CopyProject { get; set; } = false;
    public bool AutoExtract { get; set; } = false;
    public bool SingleDir { get; set; } = false;

    public string OutputSuffix { get; set; } = "-extract";

    public static readonly string OptionsFile = Path.Combine(App.AppDataPath, "options.json");

    public bool Save() {
        try {
            string json = JsonSerializer.Serialize(this, App.JsonOptions);
            File.WriteAllText(OptionsFile, json);
        }
        catch {
            return false;
        }
        return true;
    }

    public static Options? Load() {
        try {
            string json = File.ReadAllText(OptionsFile);
            var options = JsonSerializer.Deserialize<Options>(json, App.JsonOptions);
            return options?.Version != CurrentVersion ? null : options;
        }
        catch {
            return null;
        }
    }

    public override string ToString() {
        string json = JsonSerializer.Serialize(this, App.JsonOptions);
        return json;
    }
}