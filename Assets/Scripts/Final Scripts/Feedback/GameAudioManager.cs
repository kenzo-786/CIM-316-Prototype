using System.Collections.Generic;
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    [System.Serializable]
    private class SoundEntry
    {
        public string id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public float pitchMin = 1f;
        public float pitchMax = 1f;
    }

    [SerializeField] private AudioSource audioSourcePrefab;
    [SerializeField] private SoundEntry[] sounds;
    [SerializeField] private int poolSize = 12;

    private readonly Dictionary<string, SoundEntry> soundsById = new Dictionary<string, SoundEntry>();
    private readonly Queue<AudioSource> sources = new Queue<AudioSource>();

    private void Awake()
    {
        foreach (SoundEntry sound in sounds)
        {
            if (sound != null && !string.IsNullOrWhiteSpace(sound.id))
                soundsById[sound.id] = sound;
        }

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = Instantiate(audioSourcePrefab, transform);
            sources.Enqueue(source);
        }
    }

    private void OnEnable()
    {
        FeedbackEventBus.OnSoundRequested += Play;
    }

    private void OnDisable()
    {
        FeedbackEventBus.OnSoundRequested -= Play;
    }

    private void Play(string id, Vector3 position)
    {
        if (!soundsById.TryGetValue(id, out SoundEntry sound) || sound.clip == null)
            return;

        AudioSource source = sources.Dequeue();
        sources.Enqueue(source);

        source.transform.position = position;
        source.clip = sound.clip;
        source.volume = sound.volume;
        source.pitch = Random.Range(sound.pitchMin, sound.pitchMax);
        source.Play();
    }
}
