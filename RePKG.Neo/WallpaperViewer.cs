/*
  Copyright 2025 masterLazy

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0
*/

using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace RePKG.Neo;

public class WallpaperViewer {
    private readonly WebView2 _webView;

    private static string ViewerPage => Helper.GetEmbeddedResource("RePKG.Neo.WallpaperViewer.html");
    private const string HostName = "appassets.local";

    private string? _pkgPath;

    public WallpaperViewer(WebView2 webView) {
        _webView = webView;
    }

    public async void Initialize() {
        try {
            _webView.CreationProperties = new CoreWebView2CreationProperties {
                UserDataFolder = Path.Combine(App.AppDataPath, "WebView2")
            };
            await _webView.EnsureCoreWebView2Async();
            _webView.CoreWebView2.AddWebResourceRequestedFilter($"https://{HostName}/*",
                CoreWebView2WebResourceContext.All);
            _webView.CoreWebView2.WebResourceRequested += CoreWebView2OnWebResourceRequested;
            _webView.CoreWebView2.Navigate($"https://{HostName}/viewer");
        } catch (Exception e) {
            Log.Error($"Failed to initialize wallpaper viewer: {Helper.ExceptionToString(e)}");
        }
    }

    public void View(string path) {
        Log.Info($"Viewing: {path}");
        _pkgPath = path;
        Reload();
    }

    public void Reload() {
        _webView.CoreWebView2.PostWebMessageAsString("load");
    }

    public void Close() {
        _webView.CoreWebView2.PostWebMessageAsString("close");
    }

    private void CoreWebView2OnWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e) {
        try {
            var uri = new Uri(e.Request.Uri);
            if (uri.Host != HostName) return;

            string path = WebUtility.UrlDecode(uri.AbsolutePath); // starts with '/'
            // Log.Debug($"Requested: {path}");

            if (path == "/viewer") {
                e.Response = Respond(ViewerPage, 200);
                return;
            }
            if (path == "/webwallgl.global.min.js") {
                e.Response = RespondFile(Path.Combine(AppContext.BaseDirectory, "res/webwallgl.global.min.js"));
                return;
            }
            // Wallpaper file
            if (_pkgPath == null) {
                e.Response = Respond("Not found", 404);
                return;
            }
            string filename = path[1..];
            if (filename == "scene.pkg") {
                e.Response = RespondFile(_pkgPath);
            } else {
                string? parent = Path.GetDirectoryName(_pkgPath);
                if (parent == null || filename == "project.json" && Path.GetFileName(_pkgPath) != "scene.pkg") {
                    e.Response = Respond("Not found", 404);
                    return;
                }
                string realPath = Path.Combine(parent, filename);
                if (!File.Exists(realPath)) {
                    e.Response = Respond("Not found", 404);
                    return;
                }
                e.Response = RespondFile(realPath);
            }
        } catch (Exception ex) {
            e.Response = Respond($"Internal Error: {Helper.ExceptionToString(ex)}", 500);
            Log.Error($"Failed to handle request {e.Request.Uri}: {Helper.ExceptionToString(ex)}");
        }
    }

    private CoreWebView2WebResourceResponse Respond(string text, int statusCode, string contentType = "text/html") {
        string reasonPhrase = statusCode switch {
            404 => "Not Found",
            500 => "Internal Server Error",
            _   => "OK"
        };
        return _webView.CoreWebView2.Environment.CreateWebResourceResponse(
            new MemoryStream(Encoding.UTF8.GetBytes(text)),
            statusCode, reasonPhrase, $"Content-Type: {contentType}\r\n");
    }

    private CoreWebView2WebResourceResponse RespondFile(string path) {
        string contentType = Helper.ContentTypeOf(path);
        Stream stream = File.OpenRead(path);
        if (contentType == "text/html") {
            string html = File.ReadAllText(path);
            html = InjectShim(html);
            byte[] bytes = Encoding.UTF8.GetBytes(html);
            stream = new MemoryStream(bytes);
        }
        string headers = $"Content-Type: {contentType}\r\nContent-Length: {stream.Length}\r\n";
        return _webView.CoreWebView2.Environment.CreateWebResourceResponse(stream, 200, "OK", headers);
    }

    // Inject WE shim
    private static readonly Regex ShimProbe = new(@"\bdata-we-shim(?:-src)?\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex HeadTag = new(@"<head(\s[^>]*)?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static string InjectShim(string html) {
        if (string.IsNullOrEmpty(html) || ShimProbe.IsMatch(html)) return html;
        string inject = $"<script data-we-shim-src=\"1\">{Helper.GetEmbeddedResource("RePKG.Neo.res.web-shim.js")}</script>";
        var m = HeadTag.Match(html);
        if (m.Success) {
            return html.Insert(m.Index + m.Length, inject);
        }
        return $"<!DOCTYPE html><html><head>{inject}</head><body>{html}</body></html>";
    }
}