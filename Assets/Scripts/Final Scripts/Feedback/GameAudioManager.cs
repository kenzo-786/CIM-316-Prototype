    using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameAudioManager : MonoBehaviour
{
    [Serializable]
    public class SoundEffectEntry
    {
        public string id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitchMin = 1f;
        [Range(0.1f, 3f)] public float pitchMax = 1f;
        [Min(0f)] public float cooldown = 0.04f;
    }

    [Serializable]
    public class MusicEntry
    {
        public string id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Serializable]
    public class SceneMusicEntry
    {
        public string sceneName;
        public string musicId;
        [Min(0f)] public float fadeDuration = 0.5f;
    }

    private class SfxSlot
    {
        public AudioSource source;
        public float baseVolume;
    }

    [SerializeField] private SoundEffectEntry[] soundEffects;
    [SerializeField] private MusicEntry[] musicTracks;
    [SerializeField] private SceneMusicEntry[] sceneMusic;
    [SerializeField, Min(1)] private int sfxPoolSize = 16;
    [SerializeField, Range(0f, 1f)] private float musicMix = 0.5f;
    [SerializeField, Range(0f, 1f)] private float sfxMix = 0.55f;
    [SerializeField] private AudioSource musicSource;

    private readonly Dictionary<string, SoundEffectEntry> soundsById = new Dictionary<string, SoundEffectEntry>();
    private readonly Dictionary<string, MusicEntry> musicById = new Dictionary<string, MusicEntry>();
    private readonly Dictionary<string, float> lastPlayTimeById = new Dictionary<string, float>();
    private readonly List<SfxSlot> sfxSlots = new List<SfxSlot>();

    private Coroutine musicRoutine;
    private string currentMusicId;
    private float musicBaseVolume = 1f;
    private float musicFadeMultiplier = 1f;
    private int nextSfxIndex;
    private bool audioPaused;

    public static GameAudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildLookups();
        CreateMusicSource();
        CreateSfxPool();
        RefreshVolumes();
    }

    private void OnEnable()
    {
        FeedbackEventBus.OnSoundRequested += PlaySoundAtPosition;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDisable()
    {
        FeedbackEventBus.OnSoundRequested -= PlaySoundAtPosition;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void PlaySfx(string id)
    {
        if (Instance != null)
            Instance.PlaySfxInternal(id, Vector3.zero);
    }

    public static void PlayMusic(string id, float fadeDuration = 0.5f)
    {
        if (Instance != null)
            Instance.PlayMusicInternal(id, fadeDuration);
    }

    public static void StopMusic(float fadeDuration = 0.5f)
    {
        if (Instance != null)
            Instance.StopMusicInternal(fadeDuration);
    }

    public void RefreshVolumes()
    {
        float musicMultiplier = 1f;
        float sfxMultiplier = 1f;

        if (GameSettingsManager.Instance != null)
        {
            musicMultiplier = GameSettingsManager.Instance.Settings.musicVolume;
            sfxMultiplier = GameSettingsManager.Instance.Settings.sfxVolume;
        }

        if (musicSource != null)
            musicSource.volume = musicBaseVolume * musicMultiplier * musicMix * musicFadeMultiplier;

        foreach (SfxSlot slot in sfxSlots)
        {
            if (slot.source != null)
                slot.source.volume = slot.baseVolume * sfxMultiplier * sfxMix;
        }
    }

    public void SetPaused(bool paused)
    {
        if (audioPaused == paused)
            return;

        audioPaused = paused;

        if (musicSource != null)
        {
            if (paused)
                musicSource.Pause();
            else
                musicSource.UnPause();
        }

        foreach (SfxSlot slot in sfxSlots)
        {
            if (slot.source == null)
                continue;

            if (paused)
                slot.source.Pause();
            else
                slot.source.UnPause();
        }
    }

    private void BuildLookups()
    {
        soundsById.Clear();
        musicById.Clear();

        if (soundEffects != null)
        {
            foreach (SoundEffectEntry sound in soundEffects)
            {
                if (sound != null && !string.IsNullOrWhiteSpace(sound.id))
                    soundsById[sound.id.Trim()] = sound;
            }
        }

        if (musicTracks != null)
        {
            foreach (MusicEntry music in musicTracks)
            {
                if (music != null && !string.IsNullOrWhiteSpace(music.id))
                    musicById[music.id.Trim()] = music;
            }
        }
    }

    private void CreateMusicSource()
    {
        if (musicSource == null)
        {
            GameObject musicObject = new GameObject("MusicSource");
            musicObject.transform.SetParent(transform);
            musicSource = musicObject.AddComponent<AudioSource>();
        }

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
    }

    private void CreateSfxPool()
    {
        sfxSlots.Clear();

        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject sourceObject = new GameObject("SfxSource_" + (i + 1));
            sourceObject.transform.SetParent(transform);

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;

            sfxSlots.Add(new SfxSlot
            {
                source = source,
                baseVolume = 1f
            });
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        if (sceneMusic == null)
            return;

        foreach (SceneMusicEntry entry in sceneMusic)
        {
            if (entry != null && entry.sceneName == sceneName)
            {
                PlayMusicInternal(entry.musicId, entry.fadeDuration);
                return;
            }
        }
    }

    private void PlaySoundAtPosition(string id, Vector3 position)
    {
        PlaySfxInternal(id, position);
    }

    private void PlaySfxInternal(string id, Vector3 position)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        string normalizedId = id.Trim();

        if (!soundsById.TryGetValue(normalizedId, out SoundEffectEntry sound) || sound.clip == null)
        {
            Debug.LogWarning("GameAudioManager is missing a clip for sound id: " + normalizedId, this);
            return;
        }

        float cooldown = Mathf.Max(0.04f, sound.cooldown);

        if (cooldown > 0f &&
            lastPlayTimeById.TryGetValue(normalizedId, out float lastPlayTime) &&
            Time.unscaledTime < lastPlayTime + cooldown)
        {
            return;
        }

        SfxSlot slot = GetAvailableSfxSlot();
        if (slot == null || slot.source == null)
            return;

        lastPlayTimeById[normalizedId] = Time.unscaledTime;
        AudioSource source = slot.source;
        source.Stop();
        source.transform.position = position;
        source.clip = sound.clip;
        source.pitch = UnityEngine.Random.Range(
            Mathf.Min(sound.pitchMin, sound.pitchMax),
            Mathf.Max(sound.pitchMin, sound.pitchMax)
        );

        slot.baseVolume = sound.volume;
        RefreshVolumes();
        source.Play();
    }

    private SfxSlot GetAvailableSfxSlot()
    {
        foreach (SfxSlot slot in sfxSlots)
        {
            if (slot.source != null && !slot.source.isPlaying)
                return slot;
        }

        if (sfxSlots.Count == 0)
            return null;

        SfxSlot selected = sfxSlots[nextSfxIndex];
        nextSfxIndex = (nextSfxIndex + 1) % sfxSlots.Count;
        return selected;
    }

    private void PlayMusicInternal(string id, float fadeDuration)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        string normalizedId = id.Trim();

        if (!musicById.TryGetValue(normalizedId, out MusicEntry music) || music.clip == null)
        {
            Debug.LogWarning("GameAudioManager is missing a track for music id: " + normalizedId, this);
            return;
        }

        if (currentMusicId == normalizedId && musicSource != null && musicSource.isPlaying)
            return;

        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(SwitchMusicRoutine(normalizedId, music, fadeDuration));
    }

    private void StopMusicInternal(float fadeDuration)
    {
        if (musicSource == null)
            return;

        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(StopMusicRoutine(fadeDuration));
    }

    private IEnumerator SwitchMusicRoutine(string id, MusicEntry music, float fadeDuration)
    {
        if (musicSource.isPlaying)
        {
            yield return FadeMusicRoutine(0f, fadeDuration * 0.5f);
            musicSource.Stop();
        }

        currentMusicId = id;
        musicSource.clip = music.clip;
        musicBaseVolume = music.volume;
        musicFadeMultiplier = 0f;
        RefreshVolumes();
        musicSource.Play();

        yield return FadeMusicRoutine(1f, fadeDuration * 0.5f);
        musicRoutine = null;
    }

    private IEnumerator StopMusicRoutine(float fadeDuration)
    {
        yield return FadeMusicRoutine(0f, fadeDuration);
        musicSource.Stop();
        currentMusicId = null;
        musicRoutine = null;
    }

    private IEnumerator FadeMusicRoutine(float target, float duration)
    {
        float start = musicFadeMultiplier;

        if (duration <= 0f)
        {
            musicFadeMultiplier = target;
            RefreshVolumes();
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicFadeMultiplier = Mathf.Lerp(start, target, elapsed / duration);
            RefreshVolumes();
            yield return null;
        }

        musicFadeMultiplier = target;
        RefreshVolumes();
    }
}
