using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Sources (2 for crossfade)")]
    [SerializeField] private AudioSource sourceA;
    [SerializeField] private AudioSource sourceB;

    [Header("Settings")]
    [SerializeField] private float fadeSeconds = 1.0f;
    [SerializeField] private float targetVolume = 0.8f;

    private AudioSource active;
    private AudioSource inactive;
    private Coroutine fadeCo;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Auto-assign if not set
        if (!sourceA || !sourceB)
        {
            var sources = GetComponents<AudioSource>();
            if (sources.Length >= 2)
            {
                sourceA = sources[0];
                sourceB = sources[1];
            }
        }

        active = sourceA;
        inactive = sourceB;

        if (active) { active.loop = true; active.playOnAwake = false; active.volume = 0f; }
        if (inactive) { inactive.loop = true; inactive.playOnAwake = false; inactive.volume = 0f; }
    }

    public void PlayMusic(AudioClip clip, bool forceRestart = false)
    {
        if (!clip || !active || !inactive) return;

        // Si ya está sonando ese clip
        if (!forceRestart && active.clip == clip && active.isPlaying)
            return;

        // prepara el nuevo
        inactive.Stop();
        inactive.clip = clip;
        inactive.loop = true;
        inactive.volume = 0f;
        inactive.Play();

        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(CrossFade());
    }

    public void StopMusic()
    {
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = null;

        if (active) active.Stop();
        if (inactive) inactive.Stop();
    }

    private IEnumerator CrossFade()
    {
        float t = 0f;

        float startActiveVol = active.volume;
        while (t < fadeSeconds)
        {
            t += Time.unscaledDeltaTime;
            float k = fadeSeconds <= 0f ? 1f : Mathf.Clamp01(t / fadeSeconds);

            // active baja, inactive sube
            active.volume = Mathf.Lerp(startActiveVol, 0f, k);
            inactive.volume = Mathf.Lerp(0f, targetVolume, k);

            yield return null;
        }

        active.volume = 0f;
        active.Stop();

        // swap
        var temp = active;
        active = inactive;
        inactive = temp;

        fadeCo = null;
    }
}
