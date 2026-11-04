using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Small helpers for building overlay UI from code, so no scene layouts need editing.
public static class RuntimeUI
{
    public static readonly Color PanelColor = new Color(0.13f, 0.09f, 0.07f, 0.92f);
    public static readonly Color ButtonColor = new Color(0.98f, 0.82f, 0.38f, 1f);
    public static readonly Color ButtonText = new Color(0.2f, 0.12f, 0.06f, 1f);
    public static readonly Color Cream = new Color(1f, 0.96f, 0.86f, 1f);

    private static Sprite rounded;

    // Reuse whatever font the current scene's UI already uses so overlays match it.
    public static TMP_FontAsset Font
    {
        get
        {
            var existing = Object.FindAnyObjectByType<TextMeshProUGUI>();
            if (existing != null && existing.font != null) return existing.font;
            return TMP_Settings.defaultFontAsset;
        }
    }

    public static Canvas CreateCanvas(string name, int sortingOrder)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        EnsureEventSystem();
        return canvas;
    }

    public static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem", typeof(EventSystem));
        // The project uses the new Input System package; fall back to the legacy module otherwise.
        var moduleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (moduleType != null) es.AddComponent(moduleType);
        else es.AddComponent<StandaloneInputModule>();
    }

    public static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    public static Image Panel(string name, Transform parent, Color color, Vector2 size)
    {
        var rect = Rect(name, parent);
        rect.sizeDelta = size;
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = RoundedSprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        return image;
    }

    public static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color)
    {
        var rect = Rect(name, parent);
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = Font;
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }

    public static Button Button(string name, Transform parent, string text, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        var image = Panel(name, parent, ButtonColor, size);
        var button = image.gameObject.AddComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.92f, 0.6f);
        colors.pressedColor = new Color(0.85f, 0.65f, 0.25f);
        button.colors = colors;
        button.onClick.AddListener(onClick);
        var label = Text("Label", image.transform, text, size.y * 0.5f, ButtonText);
        Stretch(label.rectTransform);
        GameAudio.AddClick(button);
        return button;
    }

    public static Slider Slider(string name, Transform parent, string caption, float value, UnityEngine.Events.UnityAction<float> onChange)
    {
        var row = Rect(name, parent);
        row.sizeDelta = new Vector2(520, 60);

        var label = Text("Caption", row, caption, 30, Cream);
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.rectTransform.anchorMin = new Vector2(0, 0);
        label.rectTransform.anchorMax = new Vector2(0.35f, 1);
        label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;

        var area = Rect("Slider", row);
        area.anchorMin = new Vector2(0.38f, 0.3f);
        area.anchorMax = new Vector2(1f, 0.7f);
        area.offsetMin = area.offsetMax = Vector2.zero;
        var back = area.gameObject.AddComponent<Image>();
        back.sprite = RoundedSprite;
        back.type = Image.Type.Sliced;
        back.color = new Color(1f, 1f, 1f, 0.2f);

        var fillArea = Rect("Fill Area", area);
        Stretch(fillArea);
        var fill = Panel("Fill", fillArea, ButtonColor, Vector2.zero);
        Stretch(fill.rectTransform);

        var handleArea = Rect("Handle Area", area);
        Stretch(handleArea);
        var handle = Panel("Handle", handleArea, Cream, new Vector2(34, 34));
        handle.rectTransform.anchorMin = handle.rectTransform.anchorMax = new Vector2(0, 0.5f);

        var slider = area.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = value;
        slider.onValueChanged.AddListener(onChange);
        return slider;
    }

    // A soft rounded rectangle generated once, usable as a 9-sliced sprite.
    private static Sprite RoundedSprite
    {
        get
        {
            if (rounded != null) return rounded;
            const int size = 32, radius = 12;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(0, Mathf.Max(radius - x, x - (size - 1 - radius)));
                    float dy = Mathf.Max(0, Mathf.Max(radius - y, y - (size - 1 - radius)));
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    pixels[y * size + x] = new Color(1, 1, 1, Mathf.Clamp01(radius - d + 0.5f));
                }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            rounded = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            rounded.hideFlags = HideFlags.HideAndDontSave;
            return rounded;
        }
    }
}
