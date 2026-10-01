/*
  Copyright 2025 masterLazy

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0
*/

using System.IO;
using System.Text;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace RePKG.Neo;

public class WallpaperViewer {
    private readonly WebView2 _webView;
    private readonly Dictionary<string, string> _paths = new();
    
    private const string ViewerPage = """
      <html>
      <head>
      <script src="https://cdn.jsdelivr.net/npm/webwallgl/webwallgl.global.min.js"></script>
      <style>
      html,body{margin:0;height:100%;overflow:hidden;background:#111}
      #stage{position:relative;width:100%;height:100%;background:#111}
      #wp{position:absolute;inset:0}
      #loading{position:absolute;inset:0;z-index:10;display:flex;flex-direction:column;gap:14px;align-items:center;justify-content:center;background:#111;color:#999;font:14px/1.4 system-ui,-apple-system,sans-serif;transition:opacity .3s}
      #loading.hide{opacity:0;pointer-events:none}
      #loading.idle .spinner{display:none}
      #loading.error .spinner{display:none}
      #loading.error .text{color:#fff;text-align:center;word-break:break-all;max-width:70%}
      .spinner{width:28px;height:28px;border:3px solid #333;border-top-color:#eaeaea;border-radius:50%;animation:spin .8s linear infinite}
      @keyframes spin{to{transform:rotate(360deg)}}
      </style>
      </head>
      <body>
      <div id="stage">
        <div id="wp"></div>
        <div id="loading" class="idle"><div class="spinner"></div><div class="text">No wallpaper to view</div></div>
      </div>
      <script>
      (function(){
        const { mount, httpSource } = WebWallGL;
        const wpEl = document.getElementById('wp');
        const loading = document.getElementById('loading');
        const textEl = loading.querySelector('.text');
        let current = null, requestedPkg = null, seq = 0, chain = Promise.resolve();
      
        function show(msg){ loading.className = ''; textEl.textContent = msg; }
        function idle(msg){ loading.className = 'idle'; textEl.textContent = msg || 'No wallpaper to view'; }
        function hide(){ loading.classList.add('hide'); }
        function fail(e){ loading.className = 'error'; textEl.textContent = 'Failed: ' + ((e && e.message) || e); }
      
        function load(pkg){
          if (pkg === requestedPkg) return;
          requestedPkg = pkg;
          const id = ++seq;
          chain = chain.then(async () => {
            if (id !== seq) return;
            if (current) { try { current.destroy(); } catch(e){} current = null; }
            show('Loading...');
            try {
              const wp = await mount(wpEl, { source: httpSource('/pkg/' + pkg), fps: 60, fit: "contain" });
              if (id !== seq) { try { wp.destroy(); } catch(e){} return; }
              current = wp;
              hide();
            } catch (e) {
              if (id === seq) {
                fail(e);
                console.error('Failed: ' + ((e && e.message) || e));
                if (requestedPkg === pkg) requestedPkg = null;
              }
            }
          });
        }
      
        if (window.chrome && window.chrome.webview) {
          window.chrome.webview.addEventListener('message', e => {
            load(e.data);
          });
        }
      })();
      </script>
      </body>
      </html>
      """;

    public WallpaperViewer(WebView2 webView) {
        _webView = webView;
    }

    public async void Initialize() {
        try {
            _webView.CreationProperties = new CoreWebView2CreationProperties {
                UserDataFolder = Path.Combine(App.AppDataPath, "WebView2")
            };
            await _webView.EnsureCoreWebView2Async();
            _webView.CoreWebView2.AddWebResourceRequestedFilter("https://appassets.local/*",
                CoreWebView2WebResourceContext.All);
            _webView.CoreWebView2.WebResourceRequested += CoreWebView2OnWebResourceRequested;
            _webView.CoreWebView2.Navigate("https://appassets.local/");
        } catch (Exception e) {
            Log.Error($"Failed to initialize wallpaper viewer: {Helper.ExceptionToString(e)}");
        }
    }

    public void Refresh() {
        _webView.CoreWebView2.Reload();
    }

    public void View(string path) {
        string id = Helper.Sha256Of(path);
        _paths.TryAdd(id, path);
        _webView.CoreWebView2.PostWebMessageAsString(id);
    }

    private void CoreWebView2OnWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e) {
        try {
            var uri = new Uri(e.Request.Uri);
            if (uri.Host != "appassets.local") return;

            string path = uri.AbsolutePath; // starts with '/'

            if (path == "/") {
                e.Response = Respond(ViewerPage, 200);
                return;
            }
            if (path.StartsWith("/pkg/")) {
                string rel = path["/pkg/".Length..];
                string id = rel[..rel.IndexOf('/')];
                string filename = rel[rel.LastIndexOf('/')..];
                if (!_paths.ContainsKey(id) || filename != "/scene.pkg") {
                    e.Response = Respond($"Not found", 404);
                    return;
                }

                var stream = File.OpenRead(_paths[id]);
                string headers = $"Content-Type: application/octet-stream\r\n";
                e.Response = _webView.CoreWebView2.Environment.CreateWebResourceResponse(stream, 200, "OK", headers);
                return;
            }
            e.Response = Respond($"Not found", 404);
        } catch (Exception ex) {
            e.Response = Respond($"Internal Error: {Helper.ExceptionToString(ex)}", 500);
        }
    }

    private CoreWebView2WebResourceResponse Respond(string text, int statusCode, string contentType = "text/html") {
        return _webView.CoreWebView2.Environment.CreateWebResourceResponse(
            new MemoryStream(Encoding.UTF8.GetBytes(text)),
            statusCode, "OK", $"Content-Type: {contentType}\r\n");
    }
}