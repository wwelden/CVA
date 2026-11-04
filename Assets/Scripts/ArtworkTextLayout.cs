using UnityEngine;

/// <summary>Keeps an overlay label inside the visible part of its world-space artwork.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class ArtworkTextLayout : MonoBehaviour
{
    public Camera artworkCamera;
    public Renderer artwork;
    public Rect normalizedRegion = new Rect(0f, 0f, 1f, 1f);
    public float horizontalPadding = 24f;

    private RectTransform label;

    private void LateUpdate()
    {
        ApplyLayout();
    }

    public void ApplyLayout()
    {
        if (!artworkCamera || !artwork) return;
        if (!label) label = GetComponent<RectTransform>();
        var parent = label.parent as RectTransform;
        if (!parent || parent.rect.width <= 0f) return;

        var bounds = artwork.bounds;
        var min = artworkCamera.WorldToViewportPoint(bounds.min);
        var max = artworkCamera.WorldToViewportPoint(bounds.max);
        if (min.z <= 0f || max.z <= 0f) return;

        var left = Mathf.Lerp(min.x, max.x, normalizedRegion.xMin);
        var right = Mathf.Lerp(min.x, max.x, normalizedRegion.xMax);
        var bottom = Mathf.Lerp(min.y, max.y, normalizedRegion.yMin);
        var top = Mathf.Lerp(min.y, max.y, normalizedRegion.yMax);

        // Clip the text region to the screen before applying its inset. The artwork
        // can extend offscreen on narrower windows without taking the label with it.
        var anchorMin = new Vector2(Mathf.Clamp01(left), Mathf.Clamp01(bottom));
        var anchorMax = new Vector2(Mathf.Clamp01(right), Mathf.Clamp01(top));
        var inset = Mathf.Min(horizontalPadding, (anchorMax.x - anchorMin.x) * parent.rect.width * .1f);
        var offsetMin = new Vector2(inset, 0f);
        var offsetMax = new Vector2(-inset, 0f);
        if (label.anchorMin != anchorMin) label.anchorMin = anchorMin;
        if (label.anchorMax != anchorMax) label.anchorMax = anchorMax;
        if (label.offsetMin != offsetMin) label.offsetMin = offsetMin;
        if (label.offsetMax != offsetMax) label.offsetMax = offsetMax;
    }
}
