using UnityEngine;

// Short, render-only splashes. No colliders, damage, or gameplay random calls.
public sealed class BattlegroundFoodBurst : MonoBehaviour
{
    public enum Flavor { Tomato, Cream, Syrup, Goo }
    private static Sprite dropletSprite;
    private readonly SpriteRenderer[] drops = new SpriteRenderer[10];
    private readonly Vector3[] velocities = new Vector3[10];
    private readonly float[] sizes = new float[10];
    private float age;
    private Color color;
    private const float Lifetime = 0.42f;

    public static void Spawn(Vector3 position, SpriteRenderer source, Flavor flavor)
    {
        if (source == null || !source.enabled || source.sprite == null) return;
        var effect = new GameObject(flavor + " impact");
        effect.transform.position = position;
        var burst = effect.AddComponent<BattlegroundFoodBurst>();
        burst.Initialize(source, flavor);
        if (flavor == Flavor.Syrup) GameAudio.Play(Sfx.Syrup, 0.7f);
        else if (flavor != Flavor.Goo) GameAudio.Play(Sfx.Splat, 0.55f, 0.12f);
    }

    private static Sprite DropletSprite
    {
        get
        {
            if (dropletSprite != null) return dropletSprite;
            var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            texture.name = "Battleground splash droplet";
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            var pixels = new Color[32 * 32];
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                {
                    float radius = new Vector2(x - 15.5f, y - 15.5f).magnitude;
                    float edge = Mathf.Clamp01(15f - radius);
                    float outline = Mathf.Lerp(0.28f, 1f, Mathf.Clamp01(13f - radius));
                    pixels[y * 32 + x] = new Color(outline, outline, outline, edge);
                }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            dropletSprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
            dropletSprite.name = "Battleground splash droplet";
            dropletSprite.hideFlags = HideFlags.HideAndDontSave;
            return dropletSprite;
        }
    }

    private void Initialize(SpriteRenderer source, Flavor flavor)
    {
        color = flavor == Flavor.Tomato ? new Color(1f, 0.22f, 0.08f) :
            flavor == Flavor.Cream ? new Color(1f, 0.93f, 0.73f) :
            flavor == Flavor.Goo ? new Color(0.35f, 0.85f, 0.75f) : new Color(0.9f, 0.53f, 0.12f);
        float width = Mathf.Max(source.bounds.size.x, source.bounds.size.y);
        float radius = Mathf.Clamp(width * 0.6f, 0.3f, 0.85f);
        for (int i = 0; i < drops.Length; i++)
        {
            var child = new GameObject("Splash droplet");
            child.layer = source.gameObject.layer;
            child.transform.SetParent(transform, false);
            var drop = child.AddComponent<SpriteRenderer>();
            drop.sprite = DropletSprite;
            drop.sharedMaterial = source.sharedMaterial;
            drop.sortingLayerID = source.sortingLayerID;
            drop.sortingOrder = source.sortingOrder + 1;
            drop.color = color;
            float angle = i * Mathf.PI * 2f / drops.Length + 0.25f;
            float speed = radius * (1.8f + (i % 3) * 0.45f);
            velocities[i] = new Vector3(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed + radius, 0f);
            sizes[i] = radius * (i % 3 == 0 ? 0.24f : 0.14f);
            child.transform.localScale = Vector3.one * sizes[i];
            drops[i] = drop;
        }
        Destroy(gameObject, Lifetime);
    }

    private void Update()
    {
        age += Time.deltaTime;
        float t = Mathf.Clamp01(age / Lifetime);
        for (int i = 0; i < drops.Length; i++)
        {
            if (drops[i] == null) continue;
            Transform drop = drops[i].transform;
            drop.localPosition = velocities[i] * age + Vector3.down * (2.5f * age * age);
            drop.localScale = Vector3.one * sizes[i] * (1f - t * 0.75f);
            drops[i].color = new Color(color.r, color.g, color.b, 1f - t * t);
        }
    }
}
