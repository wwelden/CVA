// Run with Unity CLI's script run command. Preview scenes are closed without saving.
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class LayoutVerification
{
    int Width;
    int Height;
    const string Output = "ArtRefresh/UILayout";
    [System.Serializable] class Report { public int width, height, views, labels; public List<string> failures = new List<string>(); }
    Report report = new Report();
    void Check(bool condition, string message) { if (!condition) report.failures.Add(message); }
    string PathOf(Transform t) { return t.parent ? PathOf(t.parent) + "/" + t.name : t.name; }

    void Review(string name, string panel = "")
    {
        var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/" + name + ".unity");
        RenderTexture target = null;
        Texture2D texture = null;
        Camera camera = null;
        var previous = RenderTexture.active;
        try
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var candidate = root.GetComponentInChildren<Camera>();
                if (candidate) camera = candidate;
                foreach (var canvas in root.GetComponentsInChildren<Canvas>())
                {
                    if (panel != "") { var p = canvas.transform.Find(panel) ?? canvas.transform.Find("CreditsBounds/" + panel); if (p) p.gameObject.SetActive(true); }
                    // Exercise the actual dynamic result strings, rather than the editor placeholder.
                    foreach (var t in canvas.GetComponentsInChildren<TMP_Text>(true))
                    {
                        if (t.name == "earnedMonstersText") t.text = "You earned 65 Food Reserves!";
                        if (t.name == "earnedText") t.text = "You earned 40 Food Reserves.";
                    }
                }
            }
            Check(camera != null, name + ": camera missing");
            target = new RenderTexture(Width, Height, 24);
            target.Create();
            camera.targetTexture = target;
            camera.aspect = (float)Width / Height;
            camera.scene = scene;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var canvas in root.GetComponentsInChildren<Canvas>())
            {
                var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                float scale = scaler ? scaler.scaleFactor : 1;
                if (scaler && scaler.uiScaleMode == UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize)
                    scale = Mathf.Pow(Width / scaler.referenceResolution.x, 1 - scaler.matchWidthOrHeight)
                        * Mathf.Pow(Height / scaler.referenceResolution.y, scaler.matchWidthOrHeight);
                if (scaler) scaler.enabled = false;
                float logicalHeight = Height / scale;
                // Preview scenes do not lay out screen-space canvases. Reproduce their
                // pixel geometry on a camera-facing plane so Unity renders the real TMP meshes.
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = camera;
                canvas.sortingOrder = 100;
                var rect = (RectTransform)canvas.transform;
                rect.SetParent(null);
                rect.pivot = new Vector2(.5f, .5f);
                rect.sizeDelta = new Vector2(Width / scale, logicalHeight);
                rect.position = camera.transform.position + camera.transform.forward;
                rect.rotation = camera.transform.rotation;
                rect.localScale = Vector3.one * (2 * camera.orthographicSize / logicalHeight);
                foreach (var layout in canvas.GetComponentsInChildren<ArtworkTextLayout>()) layout.ApplyLayout();
                Canvas.ForceUpdateCanvases();
                if (name == "CutScene/CutScene")
                {
                    var skip = (RectTransform)canvas.transform.Find("SkipButton");
                    var corners = new Vector3[4];
                    skip.GetWorldCorners(corners);
                    var min = camera.WorldToViewportPoint(corners[0]);
                    var max = camera.WorldToViewportPoint(corners[2]);
                    Check(Mathf.Abs((max.x-min.x)*Width-160) < .1f
                        && Mathf.Abs((max.y-min.y)*Height-56) < .1f, "Intro: Skip button changed pixel size");
                    Check(Mathf.Abs((1-max.x)*Width-24) < .1f && Mathf.Abs(min.y*Height-24) < .1f,
                        "Intro: Skip button lost its corner inset");
                }
                foreach (var t in canvas.GetComponentsInChildren<TMP_Text>())
                {
                    if (string.IsNullOrWhiteSpace(t.text)) continue;
                    t.ForceMeshUpdate();
                    report.labels++;
                    string label = name + "/" + panel + ": " + PathOf(t.transform);
                    Check(!t.isTextOverflowing, label + " overflows its text rectangle");
                    var corners = new Vector3[4];
                    t.rectTransform.GetWorldCorners(corners);
                    foreach (var corner in corners)
                    {
                        var v = camera.WorldToViewportPoint(corner);
                        Check(v.x >= -.002f && v.x <= 1.002f && v.y >= -.002f && v.y <= 1.002f,
                            label + " has a text rectangle outside the viewport");
                    }
                    if (t.TryGetComponent<ArtworkTextLayout>(out var artworkLayout))
                    {
                        var a = camera.WorldToViewportPoint(artworkLayout.artwork.bounds.min);
                        var b = camera.WorldToViewportPoint(artworkLayout.artwork.bounds.max);
                        foreach (var corner in corners)
                        {
                            var v = camera.WorldToViewportPoint(corner);
                            Check(v.x >= Mathf.Max(0,a.x)-.002f && v.x <= Mathf.Min(1,b.x)+.002f
                                && v.y >= Mathf.Max(0,a.y)-.002f && v.y <= Mathf.Min(1,b.y)+.002f,
                                label + " leaves its visible artwork");
                        }
                    }
                }
                foreach (var graphic in canvas.GetComponentsInChildren<UnityEngine.UI.Graphic>()) graphic.SetAllDirty();
            }
            Canvas.ForceUpdateCanvases();
            camera.Render();
            // Keep the principal 16:10/4:3 examples as reviewable artifacts.
            if ((Width == 1280 && Height == 800 && (name == "MainMenu" || name == "LasagnaMiniGame" || name == "CutScene/CutScene"))
                || (Width == 1024 && name == "LasagnaMiniGame" && panel == ""))
            {
                RenderTexture.active = target;
                texture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                texture.Apply();
                Directory.CreateDirectory(Output + "/previews");
                File.WriteAllBytes(Output + "/previews/" + name.Replace("/", "-") + (panel == "" ? "" : "-" + panel)
                    + "-" + Width + "x" + Height + ".jpg", texture.EncodeToJPG(85));
            }
            report.views++;
        }
        finally
        {
            RenderTexture.active = previous;
            if (camera) camera.targetTexture = null;
            if (texture) Object.DestroyImmediate(texture);
            if (target) { target.Release(); Object.DestroyImmediate(target); }
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
    public static string Run(int width, int height)
    {
        var verification = new LayoutVerification { Width = width, Height = height };
        verification.report.width = width;
        verification.report.height = height;
        return verification.Execute();
    }
    string Execute()
    {
        foreach (var name in new[] { "MainMenu", "LasagnaMiniGame", "LasagnaStartGame", "OvenStart", "OvenMinigame", "VeggieStart", "SaladGame", "CutScene/CutScene" }) Review(name);
        Review("MainMenu", "rulesPanel"); Review("MainMenu", "creditsPanel");
        Review("LasagnaMiniGame", "WinPanel"); Review("LasagnaMiniGame", "losePanel");
        File.WriteAllText(Output + "/validation-" + Width + "x" + Height + ".json", JsonUtility.ToJson(report, true));
        return JsonUtility.ToJson(report);
    }
}
