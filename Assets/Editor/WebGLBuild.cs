using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Headless WebGL build for the Vercel deploy:
// Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod WebGLBuild.Build
public static class WebGLBuild
{
    public const string OutputPath = "Builds/WebGL";

    // App UI (pulled in by the editor-only AI Assistant package) ships a prebuilt
    // TextMate/Oniguruma archive compiled against a different Emscripten, so the
    // wasm link fails on missing _emval_* symbols. The game never calls it, so
    // leave it out of WebGL builds.
    const string IncompatiblePlugin = "libAppUITextMateLib.a";

    // Static hosting config for Vercel: correct wasm MIME type, long caching for
    // build files, and no caching for the page/service worker so updates show up.
    const string VercelConfig = @"{
  ""$schema"": ""https://openapi.vercel.sh/vercel.json"",
  ""headers"": [
    {
      ""source"": ""/Build/(.*)\\.wasm"",
      ""headers"": [{ ""key"": ""Content-Type"", ""value"": ""application/wasm"" }]
    },
    {
      ""source"": ""/Build/(.*)"",
      ""headers"": [{ ""key"": ""Cache-Control"", ""value"": ""public, max-age=31536000, immutable"" }]
    },
    {
      ""source"": ""/(index.html|ServiceWorker.js|manifest.webmanifest)?"",
      ""headers"": [{ ""key"": ""Cache-Control"", ""value"": ""no-cache"" }]
    }
  ]
}
";

    // Replaces the PWA template's service worker, which served every file cache-first
    // (index.html included) under a fixed cache name, so returning players never got a
    // new build. Build files are content-hashed, so they're safe to serve from cache; the
    // page and everything else go to the network first. Each build gets its own cache,
    // and activating a new worker clears the old caches and reloads open pages.
    const string ServiceWorker = @"const cacheName = ""cva-__BUILD_ID__"";
const buildFiles = __BUILD_FILES__;

self.addEventListener(""install"", e => {
  self.skipWaiting();
  e.waitUntil(caches.open(cacheName).then(cache => cache.addAll(buildFiles)));
});

self.addEventListener(""activate"", e => {
  e.waitUntil((async () => {
    // Unity's own UnityCache_* store revalidates itself; leave it alone.
    const stale = (await caches.keys()).filter(k => k !== cacheName && !k.startsWith(""UnityCache""));
    await Promise.all(stale.map(k => caches.delete(k)));
    await self.clients.claim();
    // An older worker served these pages from its cache, so reload them onto this build.
    if (stale.length > 0) {
      for (const client of await self.clients.matchAll({ type: ""window"" })) client.navigate(client.url);
    }
  })());
});

self.addEventListener(""fetch"", e => {
  const url = new URL(e.request.url);
  if (e.request.method !== ""GET"" || url.origin !== self.location.origin) return;

  // Hashed build files never change at a given URL: cache first.
  if (url.pathname.includes(""/Build/"")) {
    e.respondWith(caches.match(e.request).then(hit => hit || fetch(e.request).then(res => {
      if (res.ok) { const copy = res.clone(); caches.open(cacheName).then(c => c.put(e.request, copy)); }
      return res;
    })));
    return;
  }

  // The page and its assets: network first, cached copy only when offline.
  e.respondWith(fetch(e.request).then(res => {
    if (res.ok) { const copy = res.clone(); caches.open(cacheName).then(c => c.put(e.request, copy)); }
    return res;
  }).catch(() => caches.match(e.request)));
});
";

    static void WriteServiceWorker()
    {
        var buildDir = System.IO.Path.Combine(OutputPath, "Build");
        var files = System.IO.Directory.GetFiles(buildDir)
            .Select(f => "\"Build/" + System.IO.Path.GetFileName(f) + "\"")
            .OrderBy(f => f);
        var script = ServiceWorker
            .Replace("__BUILD_ID__", System.DateTime.UtcNow.ToString("yyyyMMddHHmmss"))
            .Replace("__BUILD_FILES__", "[" + string.Join(", ", files) + "]");
        System.IO.File.WriteAllText(System.IO.Path.Combine(OutputPath, "ServiceWorker.js"), script);
    }

    [InitializeOnLoadMethod]
    static void ExcludeIncompatiblePlugins()
    {
        foreach (var importer in PluginImporter.GetAllImporters())
        {
            if (importer.assetPath.EndsWith(IncompatiblePlugin))
                importer.SetIncludeInBuildDelegate(_ => false);
        }
    }

    [MenuItem("Build/WebGL (Vercel)")]
    public static void Build()
    {
        ExcludeIncompatiblePlugins();
        // Content-hashed file names give each build new URLs, so caches can't serve stale files.
        PlayerSettings.WebGL.nameFilesAsHashes = true;
        // Otherwise the previous build's hashed files pile up and get deployed and precached.
        var oldBuildFiles = System.IO.Path.Combine(OutputPath, "Build");
        if (System.IO.Directory.Exists(oldBuildFiles)) System.IO.Directory.Delete(oldBuildFiles, true);

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputPath,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None,
        });

        var summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            System.IO.File.WriteAllText(System.IO.Path.Combine(OutputPath, "vercel.json"), VercelConfig);
            WriteServiceWorker();
        }
        Debug.Log($"WebGL build {summary.result}: {summary.totalSize / (1024 * 1024)} MB, {summary.totalErrors} errors, {summary.totalTime}");
        if (Application.isBatchMode)
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
