using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class FoodDamageVerification
{
    static int checks;
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static void Check(bool valid, string label) { if (!valid) throw new Exception(label); checks++; }
    static void Tick(BattlegroundPlantHit hit) { typeof(BattlegroundPlantHit).GetMethod("LateUpdate", Private).Invoke(hit, null); }
    static void Set(BattlegroundPlantHit hit, string name, float value) { typeof(BattlegroundPlantHit).GetField(name, Private).SetValue(hit, value); }
    static void Damage(GameObject sample, float damage)
    {
        foreach (var component in sample.GetComponents<MonoBehaviour>())
        {
            var method = component.GetType().GetMethod("TakeDamage");
            if (method != null) { method.Invoke(component, new object[] { damage }); return; }
        }
        throw new Exception("No damage receiver");
    }
    public static string Run()
    {
        checks = 0;
        string output = Path.Combine(Application.dataPath, "../ArtRefresh/FoodDamage");
        Directory.CreateDirectory(output);
        var scene = EditorSceneManager.NewPreviewScene();
        var previous = RenderTexture.active;
        RenderTexture target = null;
        Texture2D texture = null;
        try
        {
            var names = new[] { "Lasagna", "Salad", "IceCream", "Salsa" };
            for (int row = 0; row < names.Length; row++)
            {
                Sprite cachedMid = null;
                for (int stage = 0; stage < 3; stage++)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/" + names[row] + ".prefab");
                    var sample = UnityEngine.Object.Instantiate(prefab, new Vector3(stage * 2.6f, -row * 1.9f, 0), Quaternion.identity);
                    SceneManager.MoveGameObjectToScene(sample, scene);
                    foreach (var collider in sample.GetComponents<Collider2D>()) collider.enabled = false;
                    var body = sample.GetComponent<Rigidbody2D>(); if (body) body.simulated = false;
                    foreach (var component in sample.GetComponents<MonoBehaviour>())
                    {
                        var awake = component.GetType().GetMethod("Awake", Private);
                        if (awake != null) awake.Invoke(component, null);
                    }
                    var source = sample.GetComponent<SpriteRenderer>();
                    var sprite = source.sprite;
                    Vector3 position = sample.transform.position, scale = sample.transform.localScale;
                    var hit = sample.GetComponent<BattlegroundPlantHit>();
                    Check(hit != null && source.forceRenderingOff, names[row] + " initialized from its health component");
                    float maximum = names[row] == "Lasagna" ? sample.GetComponent<Lasagna>().Health : 10f;
                    Tick(hit);
                    var visual = sample.transform.Find("Food damage visual").GetComponent<SpriteRenderer>();
                    Check(visual.sprite == sprite && hit.DamageStage == 0, "Fresh artwork is unchanged");
                    Check(sample.transform.Find("Health bar fill") == null, "Healthy monster has no bar");
                    if (stage > 0)
                    {
                        Damage(sample, maximum * 0.4f);
                        Tick(hit);
                        Check(hit.DamageStage == 1 && Mathf.Abs(hit.HealthFraction - 0.6f) < 0.001f, "First threshold tracks real health");
                        Check(visual.sprite != sprite && visual.sprite.pixelsPerUnit == sprite.pixelsPerUnit
                            && visual.sprite.pivot == sprite.pivot, "Worn sprite preserves sizing and pivot");
                        if (cachedMid == null) cachedMid = visual.sprite;
                        else Check(cachedMid == visual.sprite, "Worn sprite is cached across instances");
                        var fill = sample.transform.Find("Health bar fill").GetComponent<SpriteRenderer>();
                        Check(fill.enabled, "Damage reveals health bar");
                        Set(hit, "flashUntil", Time.time + 0.07f);
                        Tick(hit);
                        Check(visual.transform.localPosition.sqrMagnitude > 0 && visual.color != source.color, "Hit flashes and recoils artwork");
                        Check(sample.transform.position == position && sample.transform.localScale == scale, "Recoil leaves physics root untouched");
                        // Expire the flash and exercise both status tints.
                        Set(hit, "flashUntil", float.NegativeInfinity);
                        source.color = Color.blue; Tick(hit);
                        Check(visual.color == Color.blue && source.color == Color.blue, "Freeze tint survives wear");
                        source.color = Color.red; Tick(hit);
                        Check(visual.color == Color.red, "Burn tint survives wear");
                        source.color = Color.white;
                        Set(hit, "barUntil", Time.time + 0.2f); Tick(hit);
                        Check(Mathf.Abs(fill.color.a - 0.5f) < 0.01f, "Bar fades near expiry");
                        Set(hit, "barUntil", float.NegativeInfinity); Tick(hit);
                        Check(!fill.enabled, "Bar hides after expiry");
                        if (stage == 2)
                        {
                            Damage(sample, maximum * 0.35f); Tick(hit);
                            Check(hit.DamageStage == 2 && Mathf.Abs(hit.HealthFraction - 0.25f) < 0.001f, "Critical threshold tracks real health");
                            Check(visual.sprite != cachedMid, "Critical artwork adds more wear");
                        }
                        Set(hit, "flashUntil", float.NegativeInfinity);
                        Set(hit, "barUntil", Time.time + 2f);
                        Tick(hit);
                        hit.enabled = false;
                        typeof(BattlegroundPlantHit).GetMethod("OnDisable", Private).Invoke(hit, null);
                        Check(!source.forceRenderingOff && !visual.enabled, "Disabling feedback restores original renderer");
                        hit.enabled = true;
                        typeof(BattlegroundPlantHit).GetMethod("OnEnable", Private).Invoke(hit, null); Tick(hit);
                        Check(source.forceRenderingOff && visual.enabled, "Re-enabling feedback restores visual child");
                        Set(hit, "barUntil", Time.time + 2f); Tick(hit);
                    }
                    Check(sample.transform.position == position && sample.transform.localScale == scale, "All wear stages preserve root geometry");
                    var label = new GameObject("Label"); SceneManager.MoveGameObjectToScene(label, scene);
                    label.transform.position = position + new Vector3(0, -0.9f, 0);
                    var text = label.AddComponent<TextMesh>();
                    text.text = stage == 0 ? names[row] + " — healthy" : stage == 1 ? "60% health" : "25% health";
                    text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center;
                    text.characterSize = 0.075f; text.fontSize = 32; text.color = new Color(0.23f, 0.12f, 0.07f);
                }
            }
            // Exercise the existing integer-lives path without changing production prefab health.
            foreach (var type in new[] { typeof(Pie), typeof(Tomato) })
            {
                var sample = new GameObject(type.Name + " multi-hit verification");
                SceneManager.MoveGameObjectToScene(sample, scene);
                sample.transform.position = new Vector3(100f, 100f, 0f);
                var source = sample.AddComponent<SpriteRenderer>();
                source.sprite = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Pie.prefab").GetComponent<SpriteRenderer>().sprite;
                var receiver = sample.AddComponent(type);
                type.GetField("lives").SetValue(receiver, 3);
                type.GetMethod("Awake", Private).Invoke(receiver, null);
                var hit = sample.GetComponent<BattlegroundPlantHit>();
                type.GetMethod("TakeHit").Invoke(receiver, null); Tick(hit);
                Check(hit.DamageStage == 1 && (int)type.GetField("lives").GetValue(receiver) == 2, type.Name + " first hit preserves existing lives behavior");
                type.GetMethod("TakeHit").Invoke(receiver, null); Tick(hit);
                Check(hit.DamageStage == 2 && (int)type.GetField("lives").GetValue(receiver) == 1, type.Name + " critical stage at one third health");
            }
            var cameraObject = new GameObject("Damage preview camera"); SceneManager.MoveGameObjectToScene(cameraObject, scene);
            var camera = cameraObject.AddComponent<Camera>(); camera.scene = scene; camera.enabled = false;
            camera.orthographic = true; camera.orthographicSize = 4.15f;
            camera.transform.position = new Vector3(2.6f, -2.9f, -10);
            camera.backgroundColor = new Color(0.96f, 0.9f, 0.76f); camera.clearFlags = CameraClearFlags.SolidColor;
            target = new RenderTexture(1400, 1200, 24); target.Create(); camera.targetTexture = target;
            camera.Render(); RenderTexture.active = target;
            texture = new Texture2D(1400,1200,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,1400,1200),0,0); texture.Apply();
            File.WriteAllBytes(Path.Combine(output,"damage-stages.png"),texture.EncodeToPNG());
            string summary = "PASS: " + checks + " Unity preview checks. Actual prefab damage methods, two wear thresholds, render-only recoil, status tints, cache, bar fade, disable/enable. Not a gameplay playthrough.";
            File.WriteAllText(Path.Combine(output,"validation.txt"),summary);
            return summary;
        }
        finally
        {
            RenderTexture.active = previous;
            if (texture) UnityEngine.Object.DestroyImmediate(texture);
            if (target) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
