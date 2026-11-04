using UnityEngine;

// Animate a render-only child so lane movement and physics retain their original transforms.
[DisallowMultipleComponent]
public sealed class BattlegroundAlienMotion : MonoBehaviour
{
    private SpriteRenderer source;
    private SpriteRenderer visual;
    private Enemy enemy;
    private Brute brute;
    private Shooter shooter;
    private float phase;
    private float pancakeAge = -1f;
    private bool previousForceRenderingOff;
    private float hitAge = -1f;
    private const float HitTime = 0.14f;

    public static void Attach(GameObject owner)
    {
        if (owner.GetComponent<BattlegroundAlienMotion>() == null)
            owner.AddComponent<BattlegroundAlienMotion>();
    }

    private void Awake()
    {
        source = GetComponent<SpriteRenderer>();
        enemy = GetComponent<Enemy>();
        brute = GetComponent<Brute>();
        shooter = GetComponent<Shooter>();
        if (source == null) { enabled = false; return; }

        var child = new GameObject("Alien animated visual");
        child.layer = gameObject.layer;
        child.transform.SetParent(transform, false);
        visual = child.AddComponent<SpriteRenderer>();
        visual.sharedMaterials = source.sharedMaterials;
        // Different starting steps without changing Unity's gameplay random sequence.
        phase = Mathf.Repeat(transform.position.y * 3.1f + Time.time, Mathf.PI * 2f);
    }

    private void OnEnable()
    {
        if (source == null || visual == null) return;
        previousForceRenderingOff = source.forceRenderingOff;
        source.forceRenderingOff = true;
        visual.enabled = source.enabled && !previousForceRenderingOff;
    }

    public void Pancake()
    {
        if (pancakeAge >= 0f || source == null) return;
        pancakeAge = 0f;
        BattlegroundFoodBurst.Spawn(source.bounds.center, source, BattlegroundFoodBurst.Flavor.Syrup);
    }

    // Syrup wore off: go back to the normal walk.
    public void EndPancake()
    {
        pancakeAge = -1f;
    }

    // Quick flash and squash when food lands a hit.
    public void Hit()
    {
        hitAge = 0f;
    }

    private void LateUpdate()
    {
        if (source == null || visual == null) return;
        // Keep the existing Animator and status tint on the source renderer authoritative.
        visual.sprite = source.sprite;
        visual.color = source.color;
        float flash = 0f;
        if (hitAge >= 0f)
        {
            hitAge += Time.deltaTime;
            flash = 1f - Mathf.Clamp01(hitAge / HitTime);
            if (flash <= 0f) hitAge = -1f;
            // Mostly-white sprites flash warm red; already-tinted ones (the red brute) flash pale.
            Color c = source.color;
            Color target = c.g > 0.6f && c.b > 0.6f ? new Color(1f, 0.45f, 0.4f, c.a) : new Color(1f, 0.9f, 0.85f, c.a);
            visual.color = Color.Lerp(c, target, flash);
        }
        visual.flipX = source.flipX;
        visual.flipY = source.flipY;
        visual.sortingLayerID = source.sortingLayerID;
        visual.sortingOrder = source.sortingOrder;
        visual.enabled = source.enabled && !previousForceRenderingOff;
        if (source.sprite == null) return;

        float sx = 1f, sy = 1f, bob = 0f, tilt = 0f;
        float height = source.sprite.bounds.size.y;
        if (pancakeAge >= 0f)
        {
            // Stuck in syrup, not flattened: a brief splat that springs back,
            // then a slow sticky wobble for as long as the alien is held.
            pancakeAge += Time.deltaTime;
            float splat = Mathf.Sin(pancakeAge * 30f) * Mathf.Exp(-pancakeAge * 7f);
            float wobble = Mathf.Sin(pancakeAge * 3.5f + phase);
            sx = 1f + 0.18f * splat + 0.03f * wobble;
            sy = 1f - 0.22f * splat - 0.03f * wobble;
            tilt = wobble * 2.5f;
        }
        else
        {
            bool moving = enemy != null ? enemy.isActiveAndEnabled && Mathf.Abs(enemy.speed) > 0.0001f :
                brute != null ? brute.isActiveAndEnabled && Mathf.Abs(brute.speed) > 0.0001f : false;
            // Shooter already has authored walk/attack frames; only add its pancake reaction.
            if (moving && shooter == null)
            {
                bool heavy = brute != null;
                phase += Time.deltaTime * (heavy ? 7f : 12f);
                float step = Mathf.Sin(phase);
                float bounce = Mathf.Abs(step);
                bob = height * (heavy ? 0.018f : 0.035f) * bounce;
                sx = 1f + Mathf.Cos(phase * 2f) * (heavy ? 0.018f : 0.025f);
                sy = 1f - Mathf.Cos(phase * 2f) * (heavy ? 0.025f : 0.035f);
                tilt = step * (heavy ? 2.2f : 4f);
            }
        }

        if (flash > 0f)
        {
            sx *= 1f + 0.16f * flash;
            sy *= 1f - 0.12f * flash;
        }

        // Rotate and squash around the feet, including sprites with off-center pivots.
        Vector3 foot = new Vector3(source.sprite.bounds.center.x, source.sprite.bounds.min.y, 0f);
        Quaternion rotation = Quaternion.Euler(0f, 0f, tilt);
        Vector3 scale = new Vector3(sx, sy, 1f);
        visual.transform.localScale = scale;
        visual.transform.localRotation = rotation;
        visual.transform.localPosition = foot - rotation * Vector3.Scale(foot, scale) + Vector3.up * bob;
    }

    private void OnDisable()
    {
        if (source != null) source.forceRenderingOff = previousForceRenderingOff;
        if (visual != null) visual.enabled = false;
    }

    private void OnDestroy()
    {
        if (visual != null) Destroy(visual.gameObject);
    }
}
