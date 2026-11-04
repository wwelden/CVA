using TMPro;
using UnityEngine;

// Softer status colors than pure Color.blue / Color.red, so sprites stay readable.
public static class StatusTint
{
    public static readonly Color Frozen = new Color(0.62f, 0.85f, 1f, 1f);
    public static readonly Color Burning = new Color(1f, 0.62f, 0.45f, 1f);
    public static readonly Color Syrup = new Color(0.6f, 0.4f, 0.2f, 1f);
    public static readonly Color BruteSyrup = new Color(0.4f, 0.075f, 0.075f, 1f);
}

// Hit, kill and score feedback for the battle scene. Render-only: no colliders or gameplay state.
public static class BattleFeedback
{
    public static void AlienHit(GameObject alien)
    {
        if (alien == null) return;
        var motion = alien.GetComponent<BattlegroundAlienMotion>();
        if (motion != null) motion.Hit();
        GameAudio.Play(Sfx.AlienHit, 0.5f, 0.12f);
    }

    public static void AlienKilled(GameObject alien, int points, bool heavy)
    {
        if (alien == null) return;
        var renderer = alien.GetComponent<SpriteRenderer>();
        Vector3 center = renderer != null ? renderer.bounds.center : alien.transform.position;
        if (renderer != null)
        {
            AlienPop.Spawn(renderer);
            BattlegroundFoodBurst.Spawn(center, renderer, BattlegroundFoodBurst.Flavor.Goo);
        }
        ScorePopup(center + Vector3.up * 0.3f, "+" + points);
        GameAudio.Play(Sfx.AlienDie, heavy ? 0.9f : 0.65f);
        CameraShake.Shake(heavy ? 0.12f : 0.035f, heavy ? 0.22f : 0.1f);
    }

    public static void ScorePopup(Vector3 position, string text)
    {
        FloatingText.Spawn(position, text, new Color(1f, 0.9f, 0.3f));
    }
}

// Briefly enlarges and fades a copy of a sprite where an alien was defeated.
public sealed class AlienPop : MonoBehaviour
{
    private const float Lifetime = 0.2f;
    private SpriteRenderer copy;
    private Color baseColor;
    private Vector3 baseScale;
    private float age;

    public static void Spawn(SpriteRenderer source)
    {
        if (source == null || source.sprite == null) return;
        var go = new GameObject("Alien pop");
        go.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
        go.transform.localScale = source.transform.lossyScale;
        var pop = go.AddComponent<AlienPop>();
        pop.copy = go.AddComponent<SpriteRenderer>();
        pop.copy.sprite = source.sprite;
        pop.copy.flipX = source.flipX;
        pop.copy.sharedMaterial = source.sharedMaterial;
        pop.copy.sortingLayerID = source.sortingLayerID;
        pop.copy.sortingOrder = source.sortingOrder + 1;
        pop.baseColor = source.color;
        pop.baseScale = go.transform.localScale;
    }

    private void Update()
    {
        age += Time.deltaTime;
        float t = Mathf.Clamp01(age / Lifetime);
        transform.localScale = new Vector3(baseScale.x * (1f + 0.35f * t), baseScale.y * (1f - 0.25f * t), baseScale.z);
        copy.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (1f - t));
        if (t >= 1f) Destroy(gameObject);
    }
}

// World-space text that drifts up and fades, e.g. "+100".
public sealed class FloatingText : MonoBehaviour
{
    private const float Lifetime = 0.8f;
    private TextMeshPro label;
    private Color color;
    private float age;

    public static void Spawn(Vector3 position, string text, Color color)
    {
        var go = new GameObject("Floating text");
        go.transform.position = new Vector3(position.x, position.y, 0f);
        var floating = go.AddComponent<FloatingText>();
        floating.color = color;
        floating.label = go.AddComponent<TextMeshPro>();
        floating.label.text = text;
        floating.label.fontSize = 4f;
        floating.label.alignment = TextAlignmentOptions.Center;
        floating.label.color = color;
        floating.label.outlineWidth = 0.25f;
        floating.label.outlineColor = new Color32(40, 20, 10, 255);
        floating.label.sortingOrder = 500;
        floating.label.rectTransform.sizeDelta = new Vector2(4f, 1f);
    }

    private void Update()
    {
        age += Time.deltaTime;
        float t = Mathf.Clamp01(age / Lifetime);
        transform.position += Vector3.up * (1.1f * (1f - t) * Time.deltaTime);
        float pop = t < 0.15f ? Mathf.Lerp(0.6f, 1.15f, t / 0.15f) : Mathf.Lerp(1.15f, 1f, (t - 0.15f) / 0.85f);
        transform.localScale = Vector3.one * pop;
        label.color = new Color(color.r, color.g, color.b, 1f - t * t);
        if (t >= 1f) Destroy(gameObject);
    }
}

// Adds a short decaying shake to the main camera without moving its resting position.
public sealed class CameraShake : MonoBehaviour
{
    private Vector3 rest;
    private float strength;
    private float remaining;
    private float duration;

    public static void Shake(float amount, float seconds)
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        var shake = cam.GetComponent<CameraShake>();
        if (shake == null)
        {
            shake = cam.gameObject.AddComponent<CameraShake>();
            shake.rest = cam.transform.localPosition;
        }
        if (amount >= shake.strength * (shake.remaining / Mathf.Max(shake.duration, 0.0001f)))
        {
            shake.strength = amount;
            shake.duration = shake.remaining = seconds;
        }
    }

    private void LateUpdate()
    {
        if (remaining <= 0f) return;
        remaining -= Time.unscaledDeltaTime;
        if (remaining <= 0f)
        {
            transform.localPosition = rest;
            strength = 0f;
            return;
        }
        float k = strength * (remaining / duration);
        float time = Time.unscaledTime * 40f;
        var offset = new Vector2(Mathf.PerlinNoise(time, 0.3f) - 0.5f, Mathf.PerlinNoise(0.7f, time) - 0.5f) * 2f;
        transform.localPosition = rest + (Vector3)(offset * k);
    }
}
