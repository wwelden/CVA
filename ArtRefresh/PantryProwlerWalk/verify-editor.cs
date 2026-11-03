using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class ProwlerWalkCheck
{
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
    public static void Run()
    {
        try
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/PantryProwlerWalk.png").OfType<Sprite>().OrderBy(s => s.name).ToArray();
            Check(sprites.Length == 6, "Atlas slice count");
            foreach (var sprite in sprites)
            {
                Check(sprite.rect.width == 900 && sprite.rect.height == 660, "Sprite rect");
                Check(sprite.pixelsPerUnit == 100, "Sprite PPU");
                Check(Vector2.Distance(sprite.pivot, new Vector2(450, 330)) < .01f, "Sprite pivot");
            }
            var walk = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/PantryProwlerWalk.anim");
            var idle = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/PantryProwlerIdle.anim");
            Check(walk != null && idle != null, "Imported animation clips");
            Check(Mathf.Abs(walk.length - .75f) < .001f && walk.isLooping, "Walk loop duration");
            var binding = AnimationUtility.GetObjectReferenceCurveBindings(walk).Single();
            Check(binding.path == "" && binding.propertyName == "m_Sprite", "Only root sprite animates");
            var keys = AnimationUtility.GetObjectReferenceCurve(walk, binding);
            Check(keys.Length == 7 && keys.All(k => k.value != null), "All sprite keyframes resolve");
            Check(keys.Take(6).Select(k => k.value).Distinct().Count() == 6 && keys[0].value == keys[6].value, "Six frames and closing key");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Shooter.prefab");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var animator = go.GetComponent<Animator>();
            var bridge = go.GetComponent<PantryProwlerAnimation>();
            var shooter = go.GetComponent<Shooter>();
            Check(animator != null && bridge != null, "Prefab components");
            var controller = (AnimatorController)animator.runtimeAnimatorController;
            Check(controller.parameters.Single().name == "Walking", "Controller parameter");
            var machine = controller.layers[0].stateMachine;
            Check(machine.defaultState.name == "Idle", "Default idle state");
            Check(machine.states.Length == 2 && machine.states.All(s => !s.state.writeDefaultValues), "Two states with write defaults off");
            foreach (var state in machine.states)
            {
                var transition = state.state.transitions.Single();
                Check(transition.duration == 0 && !transition.hasExitTime, "Immediate state transitions");
                Check(transition.conditions.Single().mode == (state.state.name == "Idle" ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot), "Transition conditions");
            }
            var renderer = go.GetComponent<SpriteRenderer>();
            var position = go.transform.position;
            renderer.color = new Color(.2f, .4f, .8f, 1);
            var color = renderer.color;
            for (int i = 0; i < 6; i++)
            {
                walk.SampleAnimation(go, i / 8f);
                Check(renderer.sprite == keys[i].value, "Animation sample " + i);
                Check(renderer.color == color && go.transform.position == position, "Tint and position preserved");
            }
            idle.SampleAnimation(go, 0);
            Check(renderer.sprite == AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Alien3.png"), "Original idle sprite");
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(PantryProwlerAnimation).GetMethod("Awake", flags).Invoke(bridge, null);
            Action<bool, string> tick = (expected, label) => {
                typeof(PantryProwlerAnimation).GetMethod("Update", flags).Invoke(bridge, null);
                Check(animator.GetBool("Walking") == expected, label);
            };
            tick(true, "Moving walks");
            shooter.reloading = true; tick(false, "Reload idles");
            shooter.reloading = false; tick(true, "Reload recovery walks");
            shooter.pancaked = true; tick(false, "Immobilized idles");
            shooter.pancaked = false; shooter.speed = 0; tick(false, "Zero speed idles");
            shooter.speed = .5f; shooter.enabled = false; tick(false, "Disabled movement idles");
            shooter.enabled = true; tick(true, "Reenabled movement walks");
            typeof(PantryProwlerAnimation).GetMethod("OnDisable", flags).Invoke(bridge, null);
            Check(!animator.GetBool("Walking"), "Disabled animation clears parameter");
            UnityEngine.Object.DestroyImmediate(go);
            File.WriteAllText("../prowler-unity-result.json", "{\"passed\":true,\"editor\":\"" + Application.unityVersion + "\",\"atlasSlices\":6,\"clipSampling\":true,\"movementStates\":8,\"tintAndTransformPreserved\":true,\"fullGameplayPlaytest\":false}");
            Debug.Log("PROWLER_WALK_CHECK_PASSED");
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            File.WriteAllText("../prowler-unity-result.json", "{\"passed\":false,\"error\":\"" + e.Message.Replace("\"", "'") + "\"}");
            EditorApplication.Exit(1);
        }
    }
}
