using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Apply to every WebGL build, including Build Profiles and cloud builds.
public sealed class WebGLBranding : IPostprocessBuildWithReport
{
    public int callbackOrder => 100;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.WebGL)
            Apply(report.summary.outputPath);
    }

    public static void Apply(string outputPath)
    {
        var source = Path.Combine(Application.dataPath, "Editor/WebGLBranding");
        var templateData = Path.Combine(outputPath, "TemplateData");
        Directory.CreateDirectory(templateData);
        foreach (var file in Directory.GetFiles(source, "cva-alien-chef-*.png"))
            File.Copy(file, Path.Combine(templateData, Path.GetFileName(file)), true);
        File.Copy(Path.Combine(source, "favicon.ico"), Path.Combine(templateData, "favicon.ico"), true);
        File.Copy(Path.Combine(source, "favicon.ico"), Path.Combine(outputPath, "favicon.ico"), true);

        var indexPath = Path.Combine(outputPath, "index.html");
        var html = File.ReadAllText(indexPath);
        // Distinct PNG filenames bypass the browser's cached Unity favicon.
        html = Regex.Replace(html, @"\s*<link\b[^>]*\brel=""(?:shortcut icon|icon|apple-touch-icon)""[^>]*>", "", RegexOptions.IgnoreCase);
        const string links = "    <link rel=\"icon\" type=\"image/png\" sizes=\"16x16\" href=\"TemplateData/cva-alien-chef-16.png\">\n"
            + "    <link rel=\"icon\" type=\"image/png\" sizes=\"32x32\" href=\"TemplateData/cva-alien-chef-32.png\">\n"
            + "    <link rel=\"apple-touch-icon\" sizes=\"180x180\" href=\"TemplateData/cva-alien-chef-180.png\">\n";
        html = html.Replace("</head>", links + "  </head>");
        File.WriteAllText(indexPath, html);

        var manifestPath = Path.Combine(outputPath, "manifest.webmanifest");
        if (File.Exists(manifestPath))
        {
            var manifest = File.ReadAllText(manifestPath);
            const string icons = "\"icons\": ["
                + "{\"src\":\"TemplateData/cva-alien-chef-192.png\",\"sizes\":\"192x192\",\"type\":\"image/png\",\"purpose\":\"any\"},"
                + "{\"src\":\"TemplateData/cva-alien-chef-512.png\",\"sizes\":\"512x512\",\"type\":\"image/png\",\"purpose\":\"any\"}]";
            manifest = Regex.Replace(manifest, @"""icons""\s*:\s*\[[\s\S]*?\]", icons);
            File.WriteAllText(manifestPath, manifest);
        }
    }
}
