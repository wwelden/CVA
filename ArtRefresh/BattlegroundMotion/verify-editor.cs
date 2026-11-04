// Run through Unity_RunCommand while the project is in Play mode.
// Creates isolated runtime samples far outside the board, writes evidence, then removes them.
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        if (!EditorApplication.isPlaying) throw new Exception("Enter Play mode first.");
        string output = Path.Combine(Application.dataPath, "../ArtRefresh/BattlegroundMotion");
        Directory.CreateDirectory(output);
        var checks = new List<string>();
        var objects = new List<GameObject>();
        var sources = new SpriteRenderer[3];
        var visuals = new SpriteRenderer[3];
        var motions = new BattlegroundAlienMotion[3];
        var scales = new Vector3[3];
        var firstPose = new Vector3[3];
        var prefabNames = new[] { "Enemy", "Brute", "Shooter" };
        for (int i = 0; i < 3; i++)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/" + prefabNames[i] + ".prefab");
            var sample = UnityEngine.Object.Instantiate(prefab, new Vector3(96f + i * 4f, 100f, 0f), Quaternion.identity);
            sample.name = "Animation verification " + prefabNames[i];
            result.RegisterObjectCreation(sample);
            objects.Add(sample);
            foreach (var collider in sample.GetComponents<Collider2D>()) collider.enabled = false;
            sample.GetComponent<Rigidbody2D>().simulated = false;
            sources[i] = sample.GetComponent<SpriteRenderer>();
            motions[i] = sample.GetComponent<BattlegroundAlienMotion>();
            visuals[i] = sample.transform.Find("Alien animated visual").GetComponent<SpriteRenderer>();
            scales[i] = sample.transform.localScale;
        }
        var cameraObject = new GameObject("Animation verification camera");
        result.RegisterObjectCreation(cameraObject);
        objects.Add(cameraObject);
        var camera = cameraObject.AddComponent<Camera>();
        camera.enabled = false;
        camera.orthographic = true;
        camera.orthographicSize = 2.4f;
        camera.transform.position = new Vector3(100f, 99.9f, -10f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.95f, 0.9f, 0.8f);
        float originalScale = Time.timeScale;
        float originalGlobalScale = GameData.globalTimeScale;
        var manager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        bool pausedByCheck = false;
        bool originalBackground = Application.runInBackground;
        Application.runInBackground = true;
        float start = Time.time;
        int startFrame = Time.frameCount;
        int stage = 0;
        double pausedAt = 0;
        Vector3 pausedPose = Vector3.zero;
        EditorApplication.CallbackFunction check = null;
        Action<bool, string> assert = (condition, label) => {
            if (!condition) throw new Exception(label);
            checks.Add(label);
        };
        Action<string> capture = name => {
            var target = new RenderTexture(1200, 480, 24);
            var oldTarget = RenderTexture.active;
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var texture = new Texture2D(1200, 480, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, 1200, 480), 0, 0);
            texture.Apply();
            File.WriteAllBytes(Path.Combine(output, name + ".png"), texture.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = oldTarget;
            target.Release();
            UnityEngine.Object.Destroy(target);
            UnityEngine.Object.Destroy(texture);
        };
        Action cleanup = () => {
            EditorApplication.update -= check;
            if (pausedByCheck && manager != null && manager.paused) manager.TogglePause();
            GameData.globalTimeScale = originalGlobalScale;
            Time.timeScale = originalScale;
            Application.runInBackground = originalBackground;
            foreach (var burst in UnityEngine.Object.FindObjectsByType<BattlegroundFoodBurst>())
                if (burst.transform.position.x > 90f) UnityEngine.Object.Destroy(burst.gameObject);
            foreach (var obj in objects) if (obj != null) UnityEngine.Object.Destroy(obj);
        };
        check = () => {
            try
            {
                if (!EditorApplication.isPlaying) throw new Exception("Play mode ended during verification.");
                float elapsed = Time.time - start;
                if (stage == 0 && elapsed > 0.05f)
                {
                    assert(Time.frameCount > startFrame, "Play mode frames advance");
                    for (int i = 0; i < 3; i++)
                    {
                        assert(motions[i] != null && visuals[i].enabled && sources[i].forceRenderingOff,
                            prefabNames[i] + " has active visual child and hidden source");
                        firstPose[i] = visuals[i].transform.localScale;
                    }
                    stage = 1;
                }
                else if (stage == 1 && elapsed > 0.2f)
                {
                    for (int i = 0; i < 2; i++)
                        assert(firstPose[i] != visuals[i].transform.localScale, prefabNames[i] + " gait advances");
                    for (int i = 0; i < 3; i++)
                    {
                        assert(sources[i].transform.localScale == scales[i], prefabNames[i] + " physics root scale unchanged");
                        assert(visuals[i].sprite == sources[i].sprite, prefabNames[i] + " source sprite follows Animator");
                    }
                    sources[0].color = Color.blue;
                    capture("walking");
                    // Isolate gameplay time for a few frames, then restore it in cleanup even on failure.
                    if (manager != null && !manager.paused) { manager.TogglePause(); pausedByCheck = true; }
                    else { GameData.globalTimeScale = 0f; Time.timeScale = 0f; }
                    stage = 21;
                }
                else if (stage == 21 && Time.deltaTime == 0f)
                {
                    pausedPose = visuals[1].transform.localScale;
                    pausedAt = EditorApplication.timeSinceStartup;
                    stage = 2;
                }
                else if (stage == 2 && EditorApplication.timeSinceStartup - pausedAt > 0.15)
                {
                    assert(visuals[1].transform.localScale == pausedPose, "Walk freezes while paused");
                    assert(visuals[0].color == Color.blue, "Status tint reaches visual child");
                    sources[0].color = Color.white;
                    if (pausedByCheck && manager != null) { manager.TogglePause(); pausedByCheck = false; }
                    GameData.globalTimeScale = originalGlobalScale;
                    Time.timeScale = originalScale;
                    for (int i = 0; i < 3; i++) motions[i].Pancake();
                    int before = UnityEngine.Object.FindObjectsByType<BattlegroundFoodBurst>().Length;
                    for (int i = 0; i < 3; i++) motions[i].Pancake();
                    int after = UnityEngine.Object.FindObjectsByType<BattlegroundFoodBurst>().Length;
                    assert(before == after, "Repeated pancake calls do not duplicate splashes");
                    BattlegroundFoodBurst.Spawn(new Vector3(98f, 101f, 0f), sources[0], BattlegroundFoodBurst.Flavor.Tomato);
                    BattlegroundFoodBurst.Spawn(new Vector3(102f, 101f, 0f), sources[0], BattlegroundFoodBurst.Flavor.Cream);
                    start = Time.time;
                    stage = 3;
                }
                else if (stage == 3 && elapsed > 0.12f)
                {
                    capture("pancake-and-splashes");
                    stage = 4;
                }
                else if (stage == 4 && elapsed > 0.55f)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        assert(Mathf.Abs(visuals[i].transform.localScale.y - 0.32f) < 0.001f,
                            prefabNames[i] + " holds flattened pancake pose");
                        assert(sources[i].transform.localScale == scales[i], prefabNames[i] + " pancake leaves root scale unchanged");
                    }
                    int remaining = 0;
                    foreach (var burst in UnityEngine.Object.FindObjectsByType<BattlegroundFoodBurst>())
                        if (burst.transform.position.x > 90f) remaining++;
                    assert(remaining == 0, "Splashes clean up after lifetime");
                    motions[0].enabled = false;
                    assert(!sources[0].forceRenderingOff && !visuals[0].enabled, "Disabling motion restores original renderer");
                    motions[0].enabled = true;
                    assert(sources[0].forceRenderingOff && visuals[0].enabled, "Re-enabling motion restores visual child");
                    File.WriteAllText(Path.Combine(output, "validation.txt"), "PASS: " + checks.Count + " runtime checks\n" + string.Join("\n", checks));
                    cleanup();
                    Debug.Log("Battleground motion verification passed: " + checks.Count + " checks.");
                }
            }
            catch (Exception exception)
            {
                File.WriteAllText(Path.Combine(output, "validation.txt"), "FAIL: " + exception + "\nPassed checks:\n" + string.Join("\n", checks));
                cleanup();
                Debug.LogException(exception);
            }
        };
        EditorApplication.update += check;
        result.Log("Runtime verification scheduled; evidence: " + output);
    }
}
