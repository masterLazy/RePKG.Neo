/*
   Copyright 2025 masterLazy

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0
 */

using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace RePKG.Neo;

internal static class Helper {
    public static string ByteToString(long bytes) {
        float fBytes = bytes;
        if (bytes < 1000) return bytes.ToString() + " B";
        fBytes /= 1024;
        if (fBytes < 1000) return fBytes.ToString("0.") + " KiB";
        fBytes /= 1024;
        if (fBytes < 1000) return fBytes.ToString("0.") + " MiB";
        fBytes /= 1024;
        return fBytes.ToString("0.") + " GiB";
    }

    public static string? FindFileIgnoreExt(string path, string fileNameWithoutExt) {
        if (!Directory.Exists(path)) return null;
        string[] allFiles = Directory.GetFiles(path);
        foreach (string file in allFiles) {
            string name = Path.GetFileName(file);
            string nameNoExt = Path.GetFileNameWithoutExtension(file);
            if (nameNoExt.Equals(
                    fileNameWithoutExt,
                    StringComparison.OrdinalIgnoreCase))
                return name;
        }
        return null;
    }

    public static long GetDirectorySize(string path) {
        if (!Directory.Exists(path)) return 0;
        long totalSize;
        try {
            var files = new DirectoryInfo(path).EnumerateFiles();
            totalSize = files.Sum(file => file.Length);
            var subDirs = Directory.EnumerateDirectories(path);
            foreach (string subDir in subDirs) totalSize += GetDirectorySize(subDir);
        }
        catch {
            return 0;
        }
        return totalSize;
    }
    
    public static string ExceptionToString(Exception? ex) {
        return ex == null ? "null" : $"{ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}";
    }

    public static string ContentTypeOf(string filename) {
        filename = Path.GetExtension(filename);
        return filename switch {
            ".html" =>  "text/html",
            ".css"  =>  "text/css",
            ".js"  =>  "application/javascript",
            ".png"  =>  "image/png",
            ".gif"  =>  "image/gif",
            ".ico"  =>  "image/x-icon",
            ".svg"  =>  "image/svg+xml",
            ".webp" =>  "image/webp",
            ".json" =>  "application/json",
            _ => "application/octet-stream"
        };
    }
    
    public static string GetEmbeddedResource(string resourceName) {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) {
            return string.Empty;
        }
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
    
    // Win32 APIs
    
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
    
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    
    public const int SW_RESTORE = 9;
}