/**
   Copyright 2025 masterLazy

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0
 */
using System.IO;
using System.Text.Json;

namespace RePKG.Neo {
    // Only contains what we need
    internal class ProjectJson {
        public string title { get; set; } = "";
        public string preview { get; set; } = "";

        public static ProjectJson? ReadFrom(string filePath) {
            try {
                var json = File.ReadAllText(filePath);
                var projectJson = JsonSerializer.Deserialize<ProjectJson>(json, App.JsonOptions);
                return projectJson;
            }
            catch {
                return null;
            }
        }
    }
}
