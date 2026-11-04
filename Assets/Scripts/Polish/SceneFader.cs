using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Fades to black before every scene change and back in afterwards.
// Use SceneFader.Load(...) in place of SceneManager.LoadScene(...).
public sealed class SceneFader : MonoBehaviour
{
    private const float FadeOutTime = 0.22f;
    private const float FadeInTime = 0.3f;

    private static SceneFader instance;
    private CanvasGroup group;
    private Coroutine running;

    // True from the moment a load is requested until the new scene has started fading in.
    public static bool IsTransitioning { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;
        var go = new GameObject("Scene Fader");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SceneFader>();
        instance.Build();
        SceneManager.sceneLoaded += (_, __) => instance.FadeIn();
    }

    private void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;
        gameObject.AddComponent<GraphicRaycaster>();
        group = gameObject.AddComponent<CanvasGroup>();

        var image = new GameObject("Fade", typeof(RectTransform), typeof(Image));
        image.transform.SetParent(transform, false);
        var rect = (RectTransform)image.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        image.GetComponent<Image>().color = Color.black;

        group.alpha = 1f;
        group.blocksRaycasts = true;
    }

    public static void Load(string sceneName)
    {
        if (instance == null) { SceneManager.LoadScene(sceneName); return; }
        if (IsTransitioning) return;
        IsTransitioning = true;
        instance.Run(instance.FadeOutThen(() => SceneManager.LoadScene(sceneName)));
    }

    public static void Load(int buildIndex)
    {
        if (instance == null) { SceneManager.LoadScene(buildIndex); return; }
        if (IsTransitioning) return;
        IsTransitioning = true;
        instance.Run(instance.FadeOutThen(() => SceneManager.LoadScene(buildIndex)));
    }

    private void FadeIn()
    {
        IsTransitioning = false;
        Run(Fade(group.alpha, 0f, FadeInTime));
    }

    private void Run(IEnumerator routine)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(routine);
    }

    private IEnumerator FadeOutThen(System.Action load)
    {
        group.blocksRaycasts = true; // swallow double-clicks during the fade
        yield return Fade(group.alpha, 1f, FadeOutTime);
        load();
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        group.blocksRaycasts = true;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        group.alpha = to;
        group.blocksRaycasts = to > 0.01f;
    }
}
