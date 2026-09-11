using System;
using System.Collections.Generic;
using Game.Common;
using Game.GameSettings;
using UnityEngine;

namespace Game.Audio
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource sfxSourceLowVolume;

        [SerializeField] private float defaultMusicLowVolume = 0.1f;
        [SerializeField] private float defaultMusicVolume = 0.7f;

        [Header("Overlapping Enemy SFX Limiting")]
        [SerializeField] private ThrottledSfxQueue enemyHitSfxQueue = new ThrottledSfxQueue();
        [SerializeField] private ThrottledSfxQueue enemyDeathSfxQueue = new ThrottledSfxQueue();

        private float masterVolume = 1f;
        private float musicVolume;
        private float sfxVolume = 1f;

        // Caps how many identical hit/death SFX can pile up when many enemies are hit
        // or die in the same frame (e.g. a piercing arrow, or a wave-clear), and spaces
        // playback out over time instead of letting them all mix together at full volume.
        [Serializable]
        private class ThrottledSfxQueue
        {
            [SerializeField] private int maxQueueSize = 5;
            [SerializeField] private float staggerInterval = 0.05f;

            private readonly Queue<AudioClip> pendingClips = new Queue<AudioClip>();
            private float nextPlayTime = 0f;

            public void Enqueue(AudioClip clip)
            {
                if (clip == null || pendingClips.Count >= maxQueueSize)
                {
                    return;
                }
                pendingClips.Enqueue(clip);
            }

            public void Tick(AudioSource source)
            {
                if (pendingClips.Count == 0 || Time.time < nextPlayTime)
                {
                    return;
                }

                AudioClip clip = pendingClips.Dequeue();
                source.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                source.PlayOneShot(clip);
                nextPlayTime = Time.time + staggerInterval;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            SetMusicVolume(defaultMusicVolume);
        }

        private void Start()
        {
            PauseManager pauseManager = PauseManager.Instance;
            if (pauseManager != null)
            {
                pauseManager.onPauseToggled += OnPauseToggled;
            }
        }

        private void Update()
        {
            enemyHitSfxQueue.Tick(sfxSourceLowVolume);
            enemyDeathSfxQueue.Tick(sfxSourceLowVolume);
        }

        private void OnDestroy()
        {
            PauseManager pauseManager = PauseManager.Instance;
            if (pauseManager != null)
            {
                pauseManager.onPauseToggled -= OnPauseToggled;
            }
        }

        public AudioClip CurrentMusicClip => musicSource.clip;
        public bool IsMusicPlaying => musicSource.isPlaying;
        public float MusicTime
        {
            get => musicSource.time;
            set => musicSource.time = value;
        }

        public void PlayMusic(AudioClip clip, float startTime = 0f)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.time = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, clip.length - 0.01f));
            musicSource.Play();
        }

        public void PlaySFX(AudioClip clip, bool randomizePitch = false)
        {
            if (randomizePitch)
            {
                sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
            }
            else
            {
                sfxSource.pitch = 1f;
            }
            sfxSource.PlayOneShot(clip);
        }

        public void PlayLowVolumeSFX(AudioClip clip, bool randomizePitch = false)
        {            
            if (randomizePitch)
            {
                sfxSourceLowVolume.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
            }
            else
            {
                sfxSourceLowVolume.pitch = 1f;
            }
            sfxSourceLowVolume.PlayOneShot(clip);
        }

        public void PlayEnemyHitSFX(AudioClip clip)
        {
            enemyHitSfxQueue.Enqueue(clip);
        }

        public void PlayEnemyDeathSFX(AudioClip clip)
        {
            enemyDeathSfxQueue.Enqueue(clip);
        }

        private void OnPauseToggled(bool isPaused)
        {
            if (isPaused)
            {
                SetLowVolume();
            }
            else
            {
                SetNormalVolume();
            }
        }

        public void SetLowVolume()
        {
            float duckRatio = defaultMusicLowVolume / defaultMusicVolume;
            musicSource.volume = musicVolume * masterVolume * duckRatio;
        }
        private void SetNormalVolume()
        {
            musicSource.volume = musicVolume * masterVolume;
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume * masterVolume;
            sfxSource.volume = sfxVolume * masterVolume;
            sfxSourceLowVolume.volume = sfxVolume * masterVolume;
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume * masterVolume;
        }

        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            sfxSource.volume = sfxVolume * masterVolume;
            sfxSourceLowVolume.volume = sfxVolume * masterVolume;
        }

        public void ResetMusicVolume()
        {
            SetNormalVolume();
        }
    }

}