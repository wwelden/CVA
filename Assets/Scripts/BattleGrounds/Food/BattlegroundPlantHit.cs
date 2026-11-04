using System.Collections.Generic;
using UnityEngine;

// All motion and wear are confined to a visual child. Physics and status-effect tints stay on the source.
[DisallowMultipleComponent]
public sealed class BattlegroundPlantHit : MonoBehaviour
{
    private const float FlashDuration = 0.14f;
    private const float PulseInterval = 0.32f;
    private const float BarDuration = 2.2f;
    private SpriteRenderer source;
    private SpriteRenderer visual;
    private SpriteRenderer barBackground;
    private SpriteRenderer barFill;
    private float maximumHealth;
    private float flashUntil = float.NegativeInfinity;
    private float nextPulse;
    private float barUntil = float.NegativeInfinity;
    private bool sourceWasHidden;
    private static Sprite barSprite;
    private static readonly Dictionary<Sprite, Sprite[]> wornSprites = new Dictionary<Sprite, Sprite[]>();

    public float HealthFraction { get; private set; } = 1f;
    public int DamageStage { get; private set; }

    public static void Attach(GameObject target, float maximum)
    {
        var feedback = target.GetComponent<BattlegroundPlantHit>();
        if (feedback == null) feedback = target.AddComponent<BattlegroundPlantHit>();
        feedback.maximumHealth = Mathf.Max(maximum, 0.001f);
        feedback.Initialize();
    }

    public static void ReportDamage(GameObject target, float remainingHealth)
    {
        var feedback = target.GetComponent<BattlegroundPlantHit>();
        if (feedback == null || feedback.maximumHealth <= 0f) return;
        feedback.HealthFraction = Mathf.Clamp01(remainingHealth / feedback.maximumHealth);
        feedback.DamageStage = feedback.HealthFraction <= 1f / 3f ? 2 :
            feedback.HealthFraction <= 2f / 3f ? 1 : 0;
        feedback.barUntil = Time.time + BarDuration;
        feedback.Pulse();
    }

    // Retained for single-hit foods, which disappear before a persistent health state can be shown.
    public static void Flash(GameObject target)
    {
        var feedback = target.GetComponent<BattlegroundPlantHit>();
        if (feedback == null) feedback = target.AddComponent<BattlegroundPlantHit>();
        feedback.Initialize();
        feedback.Pulse();
    }

    private void Initialize()
    {
        if (source != null) return;
        source = GetComponent<SpriteRenderer>();
        if (source == null) return;
        sourceWasHidden = source.forceRenderingOff;
        var child = new GameObject("Food damage visual");
        child.layer = source.gameObject.layer;
        child.transform.SetParent(source.transform, false);
        visual = child.AddComponent<SpriteRenderer>();
        visual.sprite = source.sprite;
        visual.sharedMaterial = source.sharedMaterial;
        source.forceRenderingOff = true;
    }

    private void Pulse()
    {
        // Contact damage arrives every physics step; leave a gap so hits remain distinct.
        if (Time.time < nextPulse) return;
        flashUntil = Time.time + FlashDuration;
        nextPulse = Time.time + PulseInterval;
    }

    private void OnEnable()
    {
        if (source != null) source.forceRenderingOff = true;
    }

    private void OnDisable()
    {
        if (source != null) source.forceRenderingOff = sourceWasHidden;
        if (visual != null) visual.enabled = false;
        if (barBackground != null) barBackground.enabled = false;
        if (barFill != null) barFill.enabled = false;
        flashUntil = barUntil = float.NegativeInfinity;
    }

    private void LateUpdate()
    {
        if (source == null || visual == null) return;
        bool visible = source.enabled && !sourceWasHidden && source.sprite != null;
        visual.enabled = visible;
        if (!visible)
        {
            if (barBackground != null) barBackground.enabled = false;
            if (barFill != null) barFill.enabled = false;
            return;
        }
        visual.sortingLayerID = source.sortingLayerID;
        visual.sortingOrder = source.sortingOrder;
        visual.flipX = source.flipX;
        visual.flipY = source.flipY;
        visual.sprite = DamageStage == 0 ? source.sprite : WornSprite(source.sprite, DamageStage);
        float pulse = Mathf.Clamp01((flashUntil - Time.time) / FlashDuration);
        visual.color = Color.Lerp(source.color, new Color(1f, 0.2f, 0.15f, source.color.a), pulse * 0.8f);
        float kick = Mathf.Sin(pulse * Mathf.PI);
        visual.transform.localPosition = Vector3.right * (source.sprite.bounds.size.x * 0.045f * kick);
        visual.transform.localRotation = Quaternion.Euler(0f, 0f, -5f * kick);
        UpdateBar();
    }

    private void UpdateBar()
    {
        bool show = HealthFraction > 0f && HealthFraction < 1f && Time.time < barUntil;
        if (show && barBackground == null)
        {
            barBackground = CreateBar("Health bar outline");
            barFill = CreateBar("Health bar fill");
        }
        if (barBackground == null) return;
        barBackground.enabled = barFill.enabled = show;
        if (!show) return;
        float alpha = Mathf.Clamp01((barUntil - Time.time) / 0.4f);
        float width = Mathf.Clamp(source.bounds.size.x * 0.8f, 0.45f, 1.1f);
        Vector3 center = new Vector3(source.bounds.center.x, source.bounds.max.y + 0.13f, source.transform.position.z);
        SetBarGeometry(barBackground, center, width + 0.04f, 0.105f);
        SetBarGeometry(barFill, center + Vector3.left * (width * (1f - HealthFraction) * 0.5f), width * HealthFraction, 0.065f);
        barBackground.color = new Color(0.12f, 0.07f, 0.04f, alpha);
        Color color = DamageStage == 2 ? new Color(1f, 0.32f, 0.18f) :
            DamageStage == 1 ? new Color(1f, 0.74f, 0.2f) : new Color(0.5f, 0.9f, 0.3f);
        color.a = alpha;
        barFill.color = color;
    }

    private SpriteRenderer CreateBar(string label)
    {
        if (barSprite == null)
        {
            barSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            barSprite.hideFlags = HideFlags.HideAndDontSave;
        }
        var child = new GameObject(label);
        child.layer = source.gameObject.layer;
        child.transform.SetParent(transform, false);
        var renderer = child.AddComponent<SpriteRenderer>();
        renderer.sprite = barSprite;
        renderer.sharedMaterial = source.sharedMaterial;
        return renderer;
    }

    private void SetBarGeometry(SpriteRenderer renderer, Vector3 position, float width, float height)
    {
        renderer.sortingLayerID = source.sortingLayerID;
        renderer.sortingOrder = source.sortingOrder + (renderer == barFill ? 3 : 2);
        renderer.transform.position = position;
        renderer.transform.rotation = Quaternion.identity;
        Vector3 scale = renderer.transform.parent.lossyScale;
        renderer.transform.localScale = new Vector3(width / Mathf.Max(Mathf.Abs(scale.x), 0.0001f),
            height / Mathf.Max(Mathf.Abs(scale.y), 0.0001f), 1f);
    }

    private static Sprite WornSprite(Sprite original, int stage)
    {
        if (!wornSprites.TryGetValue(original, out var stages))
        {
            stages = new Sprite[2];
            wornSprites.Add(original, stages);
        }
        if (stages[stage - 1] != null) return stages[stage - 1];
        // Read a GPU copy; imported sprites need not have Read/Write enabled.
        var previous = RenderTexture.active;
        var copy = RenderTexture.GetTemporary(original.texture.width, original.texture.height, 0);
        Texture2D texture = null;
        try
        {
            Graphics.Blit(original.texture, copy);
            RenderTexture.active = copy;
            var rect = original.rect;
            texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(copy);
        }
        var pixels = texture.GetPixels();
        int width = texture.width, height = texture.height;
        int left = width, right = 0, bottom = height, top = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (pixels[y * width + x].a > 0.1f)
                { left = Mathf.Min(left, x); right = Mathf.Max(right, x); bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y); }
        float spanX = Mathf.Max(1, right - left), spanY = Mathf.Max(1, top - bottom);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                Color pixel = pixels[index];
                if (pixel.a <= 0f) continue;
                Vector2 uv = new Vector2((x - left) / spanX, (y - bottom) / spanY);
                // A chipped edge and branching cracks; the critical stage adds a second missing chunk.
                bool chip = Vector2.Distance(uv, new Vector2(0.96f, 0.73f)) < (stage == 2 ? 0.2f : 0.13f);
                chip |= stage == 2 && Vector2.Distance(uv, new Vector2(0.1f, 0.33f)) < 0.16f;
                if (chip) { pixel.a = 0f; pixels[index] = pixel; continue; }
                float crack = Mathf.Min(DistanceToSegment(uv, new Vector2(0.87f, 0.65f), new Vector2(0.68f, 0.53f)),
                    DistanceToSegment(uv, new Vector2(0.68f, 0.53f), new Vector2(0.75f, 0.36f)));
                if (stage == 2) crack = Mathf.Min(crack, DistanceToSegment(uv, new Vector2(0.16f, 0.37f), new Vector2(0.35f, 0.23f)));
                float alpha = pixel.a;
                pixel = Color.Lerp(pixel, new Color(pixel.grayscale * 0.7f, pixel.grayscale * 0.6f, pixel.grayscale * 0.4f), stage * 0.12f);
                if (crack < 0.012f) pixel = Color.Lerp(pixel, new Color(0.23f, 0.1f, 0.035f), 0.85f);
                pixel.a = alpha;
                pixels[index] = pixel;
            }
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        texture.name = original.name + " damage " + stage;
        texture.filterMode = original.texture.filterMode;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height),
            new Vector2(original.pivot.x / width, original.pivot.y / height), original.pixelsPerUnit);
        sprite.name = texture.name;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        stages[stage - 1] = sprite;
        return sprite;
    }

    private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 line = b - a;
        return Vector2.Distance(point, a + line * Mathf.Clamp01(Vector2.Dot(point - a, line) / line.sqrMagnitude));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache()
    {
        foreach (var stages in wornSprites.Values)
            foreach (var sprite in stages)
                if (sprite != null) { Destroy(sprite.texture); Destroy(sprite); }
        wornSprites.Clear();
        if (barSprite != null) Destroy(barSprite);
        barSprite = null;
    }
}
