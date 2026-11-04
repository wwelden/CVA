using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Sfx
{
    Click, Place, Throw, Splat, Syrup, AlienHit, AlienDie, Error,
    Freeze, Burn, WaveStart, GameOver, Chop, Catch, Pause, Unpause
}

// Saved volume settings, a pool for sound effects synthesized in code,
// music volume for the existing music sources, and click sounds on every button.
public sealed class GameAudio : MonoBehaviour
{
    private const string MusicKey = "cva.musicVolume";
    private const string SfxKey = "cva.sfxVolume";
    private const int SampleRate = 22050;

    private static GameAudio instance;
    private static readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();
    private static readonly Dictionary<Sfx, float> lastPlayed = new Dictionary<Sfx, float>();
    private readonly Dictionary<AudioSource, float> musicBaseVolume = new Dictionary<AudioSource, float>();
    private readonly List<AudioSource> pool = new List<AudioSource>();
    private int next;
    private System.Random noise = new System.Random(7);

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicKey, 0.8f);
        set { PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxKey, 0.8f);
        set { PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;
        var go = new GameObject("Game Audio");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<GameAudio>();
        for (int i = 0; i < 10; i++)
        {
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            instance.pool.Add(source);
        }
        SceneManager.sceneLoaded += (_, __) => instance.OnSceneLoaded();
    }

    public static void Play(Sfx sfx, float volume = 1f, float pitchJitter = 0.06f)
    {
        if (instance == null) return;
        // Many identical hits in the same instant sound like one loud click; space them out.
        float now = Time.unscaledTime;
        if (lastPlayed.TryGetValue(sfx, out float last) && now - last < 0.035f) return;
        lastPlayed[sfx] = now;

        AudioSource source = instance.pool[instance.next];
        instance.next = (instance.next + 1) % instance.pool.Count;
        source.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        source.PlayOneShot(instance.Clip(sfx), volume * SfxVolume);
    }

    public static void ApplyMusicVolume()
    {
        if (instance != null) instance.RefreshMusic();
    }

    private void OnSceneLoaded()
    {
        RefreshMusic();
        AddButtonClicks();
    }

    private void LateUpdate()
    {
        // Music sources configure themselves in Start, so keep re-applying the multiplier.
        float music = MusicVolume;
        foreach (var pair in musicBaseVolume)
            if (pair.Key != null) pair.Key.volume = pair.Value * music;
    }

    private void RefreshMusic()
    {
        var stale = new List<AudioSource>();
        foreach (var key in musicBaseVolume.Keys) if (key == null) stale.Add(key);
        foreach (var key in stale) musicBaseVolume.Remove(key);

        var laserSources = new HashSet<AudioSource>();
        foreach (var manager in FindObjectsByType<AudioManager2>(FindObjectsInactive.Include))
            if (manager.LaserSource != null) laserSources.Add(manager.LaserSource);

        foreach (var source in FindObjectsByType<AudioSource>(FindObjectsInactive.Include))
        {
            if (pool.Contains(source) || laserSources.Contains(source)) continue;
            if (!musicBaseVolume.ContainsKey(source)) musicBaseVolume[source] = source.volume;
        }
    }

    public static void AddButtonClicks()
    {
        foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include))
            AddClick(button);
    }

    public static void AddClick(Button button)
    {
        if (button == null || button.GetComponent<ClickSoundTag>() != null) return;
        button.gameObject.AddComponent<ClickSoundTag>();
        button.onClick.AddListener(() => Play(Sfx.Click, 0.6f));
    }

    private sealed class ClickSoundTag : MonoBehaviour { }

    // ---------- synthesis ----------

    private AudioClip Clip(Sfx sfx)
    {
        if (clips.TryGetValue(sfx, out AudioClip clip)) return clip;
        float[] data = Synthesize(sfx);
        clip = AudioClip.Create("sfx_" + sfx, data.Length, 1, SampleRate, false);
        clip.SetData(data, 0);
        clips[sfx] = clip;
        return clip;
    }

    private float[] Synthesize(Sfx sfx)
    {
        switch (sfx)
        {
            case Sfx.Click: return Tone(0.045f, 1100f, 900f, Wave.Sine, 0.5f);
            case Sfx.Place: return Mix(Tone(0.12f, 190f, 90f, Wave.Sine, 0.9f), Noise(0.05f, 0.25f, 0.4f));
            case Sfx.Throw: return Noise(0.14f, 0.35f, 0.12f, rising: true);
            case Sfx.Splat: return Mix(Noise(0.13f, 0.6f, 0.08f), Tone(0.08f, 260f, 120f, Wave.Sine, 0.4f));
            case Sfx.Syrup: return Tone(0.22f, 420f, 160f, Wave.Sine, 0.6f, wobble: 18f);
            case Sfx.AlienHit: return Tone(0.06f, 420f, 260f, Wave.Square, 0.25f);
            case Sfx.AlienDie: return Mix(Tone(0.26f, 760f, 140f, Wave.Square, 0.25f), Noise(0.12f, 0.3f, 0.2f));
            case Sfx.Error: return Concat(Tone(0.07f, 150f, 150f, Wave.Square, 0.3f), Silence(0.03f), Tone(0.07f, 130f, 130f, Wave.Square, 0.3f));
            case Sfx.Freeze: return Mix(Tone(0.45f, 1560f, 1760f, Wave.Sine, 0.25f, wobble: 9f), Tone(0.45f, 2340f, 2640f, Wave.Sine, 0.15f, wobble: 11f));
            case Sfx.Burn: return Mix(Noise(0.45f, 0.45f, 0.5f), Tone(0.45f, 90f, 60f, Wave.Sine, 0.4f));
            case Sfx.WaveStart: return Concat(Tone(0.09f, 523f, 523f, Wave.Square, 0.22f), Tone(0.09f, 659f, 659f, Wave.Square, 0.22f), Tone(0.09f, 784f, 784f, Wave.Square, 0.22f), Tone(0.2f, 1046f, 1046f, Wave.Square, 0.22f));
            case Sfx.GameOver: return Concat(Tone(0.2f, 392f, 392f, Wave.Square, 0.22f), Tone(0.2f, 330f, 330f, Wave.Square, 0.22f), Tone(0.45f, 262f, 240f, Wave.Square, 0.22f));
            case Sfx.Chop: return Mix(Noise(0.05f, 0.5f, 0.02f), Tone(0.04f, 1800f, 1200f, Wave.Sine, 0.25f));
            case Sfx.Catch: return Tone(0.09f, 660f, 990f, Wave.Sine, 0.45f);
            case Sfx.Pause: return Concat(Tone(0.06f, 620f, 620f, Wave.Sine, 0.4f), Tone(0.08f, 420f, 420f, Wave.Sine, 0.4f));
            case Sfx.Unpause: return Concat(Tone(0.06f, 420f, 420f, Wave.Sine, 0.4f), Tone(0.08f, 620f, 620f, Wave.Sine, 0.4f));
            default: return Silence(0.01f);
        }
    }

    private enum Wave { Sine, Square }

    private static float[] Tone(float seconds, float startHz, float endHz, Wave wave, float gain, float wobble = 0f)
    {
        int n = Mathf.CeilToInt(seconds * SampleRate);
        var data = new float[n];
        double phase = 0;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n;
            float hz = Mathf.Lerp(startHz, endHz, t);
            if (wobble > 0f) hz *= 1f + 0.04f * Mathf.Sin(2f * Mathf.PI * wobble * i / SampleRate);
            phase += 2 * Mathf.PI * hz / SampleRate;
            float s = Mathf.Sin((float)phase);
            if (wave == Wave.Square) s = s >= 0 ? 0.6f : -0.6f;
            data[i] = s * gain * Envelope(t, 0.02f);
        }
        return data;
    }

    private float[] Noise(float seconds, float gain, float smoothing, bool rising = false)
    {
        int n = Mathf.CeilToInt(seconds * SampleRate);
        var data = new float[n];
        float prev = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n;
            float raw = (float)(noise.NextDouble() * 2 - 1);
            // One-pole low-pass: higher smoothing = duller, thuddier noise.
            prev = Mathf.Lerp(raw, prev, smoothing);
            float env = rising ? Mathf.Sin(t * Mathf.PI) : Envelope(t, 0.01f);
            data[i] = prev * gain * env;
        }
        return data;
    }

    private static float Envelope(float t, float attack)
    {
        float a = Mathf.Clamp01(t / attack);
        return a * (1f - t) * (1f - t);
    }

    private static float[] Silence(float seconds) => new float[Mathf.CeilToInt(seconds * SampleRate)];

    private static float[] Mix(float[] a, float[] b)
    {
        var data = new float[Mathf.Max(a.Length, b.Length)];
        for (int i = 0; i < data.Length; i++)
            data[i] = Mathf.Clamp((i < a.Length ? a[i] : 0f) + (i < b.Length ? b[i] : 0f), -1f, 1f);
        return data;
    }

    private static float[] Concat(params float[][] parts)
    {
        int length = 0;
        foreach (var p in parts) length += p.Length;
        var data = new float[length];
        int offset = 0;
        foreach (var p in parts) { p.CopyTo(data, offset); offset += p.Length; }
        return data;
    }
}
