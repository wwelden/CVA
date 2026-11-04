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
      ""headers"": [{ ""key"": ""Cache-Control"", ""value"": ""public, max-age=3600"" }]
    },
    {
      ""source"": ""/(index.html|ServiceWorker.js|manifest.webmanifest)?"",
      ""headers"": [{ ""key"": ""Cache-Control"", ""value"": ""no-cache"" }]
    }
  ]
}
";

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
            System.IO.File.WriteAllText(System.IO.Path.Combine(OutputPath, "vercel.json"), VercelConfig);
        Debug.Log($"WebGL build {summary.result}: {summary.totalSize / (1024 * 1024)} MB, {summary.totalErrors} errors, {summary.totalTime}");
        if (Application.isBatchMode)
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
