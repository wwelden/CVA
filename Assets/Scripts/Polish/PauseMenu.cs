using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Overlay pause menu for the battle scene, plus a shared volume panel.
public sealed class PauseMenu : MonoBehaviour
{
    private GameObject root;

    public static PauseMenu Create(GameManager manager)
    {
        Canvas canvas = RuntimeUI.CreateCanvas("Pause Menu", 900);
        var menu = canvas.gameObject.AddComponent<PauseMenu>();
        menu.Build(canvas.transform, manager);
        menu.SetVisible(false);
        return menu;
    }

    private void Build(Transform canvas, GameManager manager)
    {
        var dim = RuntimeUI.Rect("Dim", canvas);
        RuntimeUI.Stretch(dim);
        dim.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
        root = dim.gameObject;

        Image panel = RuntimeUI.Panel("Panel", dim, RuntimeUI.PanelColor, new Vector2(640, 720));
        var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 22;
        layout.padding = new RectOffset(40, 40, 40, 40);
        layout.childControlWidth = layout.childControlHeight = false;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;

        var title = RuntimeUI.Text("Title", panel.transform, "Paused", 72, RuntimeUI.Cream);
        title.rectTransform.sizeDelta = new Vector2(520, 100);

        Vector2 buttonSize = new Vector2(420, 78);
        RuntimeUI.Button("Resume", panel.transform, "Resume", buttonSize, manager.TogglePause);
        RuntimeUI.Button("Restart", panel.transform, "Restart Wave", buttonSize, manager.RestartWave);
        RuntimeUI.Button("Quit", panel.transform, "Quit to Menu", buttonSize, manager.QuitToMenu);
        VolumeControls.Add(panel.transform);

        var hint = RuntimeUI.Text("Hint", panel.transform, "P / Esc to resume", 24, new Color(1f, 1f, 1f, 0.55f));
        hint.rectTransform.sizeDelta = new Vector2(520, 36);
    }

    public void SetVisible(bool visible)
    {
        if (root != null) root.SetActive(visible);
    }
}

public static class VolumeControls
{
    public static void Add(Transform parent)
    {
        RuntimeUI.Slider("Music", parent, "Music", GameAudio.MusicVolume, v =>
        {
            GameAudio.MusicVolume = v;
        });
        RuntimeUI.Slider("Sound", parent, "Effects", GameAudio.SfxVolume, v =>
        {
            GameAudio.SfxVolume = v;
            GameAudio.Play(Sfx.Click, 0.6f, 0f);
        });
    }
}

// Adds a small "Sound" button to the main menu that opens the volume sliders.
public sealed class MenuSoundSettings : MonoBehaviour
{
    private GameObject panel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Hook()
    {
        SceneManager.sceneLoaded += (scene, _) => TryAdd(scene);
        TryAdd(SceneManager.GetActiveScene());
    }

    private static void TryAdd(Scene scene)
    {
        if (scene.name != "MainMenu" || FindAnyObjectByType<MenuSoundSettings>() != null) return;
        Canvas canvas = RuntimeUI.CreateCanvas("Sound Settings", 800);
        canvas.gameObject.AddComponent<MenuSoundSettings>().Build(canvas.transform);
    }

    private void Build(Transform canvas)
    {
        Button open = RuntimeUI.Button("Sound Button", canvas, "Sound", new Vector2(170, 64), Toggle);
        var rect = (RectTransform)open.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-28, 28);

        // Full-screen dim behind an opaque box, so menu text doesn't show through and can't be clicked.
        var dim = RuntimeUI.Rect("Sound Dim", canvas);
        RuntimeUI.Stretch(dim);
        dim.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        Color opaque = RuntimeUI.PanelColor;
        opaque.a = 1f;
        Image box = RuntimeUI.Panel("Sound Panel", dim, opaque, new Vector2(600, 300));
        var layout = box.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 18;
        layout.padding = new RectOffset(30, 30, 30, 30);
        layout.childControlWidth = layout.childControlHeight = false;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        var title = RuntimeUI.Text("Title", box.transform, "Sound", 48, RuntimeUI.Cream);
        title.rectTransform.sizeDelta = new Vector2(520, 60);
        VolumeControls.Add(box.transform);
        RuntimeUI.Button("Done", box.transform, "Done", new Vector2(220, 60), Toggle);
        panel = dim.gameObject;
        panel.SetActive(false);
    }

    private void Toggle()
    {
        panel.SetActive(!panel.activeSelf);
    }
}

// "Wave N" title card at the start of each battle, with a note when a new food unlocks.
public sealed class WaveBanner : MonoBehaviour
{
    private const float Duration = 2.4f;
    private CanvasGroup group;
    private RectTransform card;
    private float age;

    public static void ShowForWave(int wave)
    {
        string unlock = wave == 3 ? "New food: Pancakes!  (4)" :
            wave == 4 ? "New food: Ice Cream!  (5)" :
            wave == 5 ? "New food: Salsa!  (6)" : null;

        Canvas canvas = RuntimeUI.CreateCanvas("Wave Banner", 850);
        canvas.GetComponent<GraphicRaycaster>().enabled = false;
        var banner = canvas.gameObject.AddComponent<WaveBanner>();
        banner.group = canvas.gameObject.AddComponent<CanvasGroup>();
        banner.group.blocksRaycasts = false;
        banner.group.interactable = false;

        Image strip = RuntimeUI.Panel("Strip", canvas.transform, RuntimeUI.PanelColor, new Vector2(900, unlock == null ? 170 : 240));
        strip.raycastTarget = false;
        banner.card = strip.rectTransform;
        var title = RuntimeUI.Text("Wave", strip.transform, "Wave " + wave, 110, RuntimeUI.Cream);
        title.rectTransform.sizeDelta = new Vector2(860, 130);
        title.rectTransform.anchoredPosition = new Vector2(0, unlock == null ? 0 : 35);
        if (unlock != null)
        {
            var note = RuntimeUI.Text("Unlock", strip.transform, unlock, 44, RuntimeUI.ButtonColor);
            note.rectTransform.sizeDelta = new Vector2(860, 60);
            note.rectTransform.anchoredPosition = new Vector2(0, -70);
        }
        GameAudio.Play(Sfx.WaveStart, 0.8f, 0f);
    }

    private void Update()
    {
        age += Time.unscaledDeltaTime;
        float inT = Mathf.Clamp01(age / 0.3f);
        float outT = Mathf.Clamp01((age - (Duration - 0.4f)) / 0.4f);
        float ease = 1f - Mathf.Pow(1f - inT, 3f);
        card.anchoredPosition = new Vector2(Mathf.Lerp(-1400f, 0f, ease) + outT * 1400f, 120f);
        card.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, ease);
        group.alpha = 1f - outT;
        if (age >= Duration) Destroy(gameObject);
    }
}
