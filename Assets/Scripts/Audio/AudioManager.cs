using System;
using System.Collections.Generic;
using Game.Common;
using Game.Control;
using Game.Feel;
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

        [Header("SFX voices")]
        [Tooltip("How many SFX can play at once. Each sound gets its own voice, so its pitch never changes sounds already playing.")]
        [Min(1)]
        [SerializeField] private int sfxVoiceCount = 16;
        [Tooltip("Volume of the low-volume SFX (enemy hits, deaths and attacks) relative to the others. 1 keeps how they sounded " +
                 "before the voice pool: the low-volume source's own 0.2 was overwritten by the SFX volume setting.")]
        [Range(0f, 1f)]
        [SerializeField] private float lowVolumeScale = 1f;

        [Header("Overlapping Enemy SFX Limiting")]
        [SerializeField] private ThrottledSfxQueue enemyHitSfxQueue = new ThrottledSfxQueue();
        [SerializeField] private ThrottledSfxQueue enemyDeathSfxQueue = new ThrottledSfxQueue();
        [SerializeField] private ThrottledSfxQueue enemyAttackSfxQueue = new ThrottledSfxQueue();

        private float masterVolume = 1f;
        private float musicVolume;
        private float sfxVolume = 1f;

        // Built at runtime from sfxSource, so no prefab or scene changes are needed
        private SfxVoicePool voices;
        private readonly Dictionary<GameSound, float> lastPlayed = new Dictionary<GameSound, float>();
        private readonly PickupPitchChain xpPitchChain = new PickupPitchChain();

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

            public void Tick(SfxVoicePool pool, float volumeScale)
            {
                if (pendingClips.Count == 0 || Time.time < nextPlayTime)
                {
                    return;
                }

                AudioClip clip = pendingClips.Dequeue();
                pool.Play(clip, volumeScale, UnityEngine.Random.Range(0.8f, 1.2f));
                nextPlayTime = Time.time + staggerInterval;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            voices = new SfxVoicePool(sfxSource, sfxVoiceCount, transform);
            voices.SetBaseVolume(sfxVolume * masterVolume);
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
            enemyHitSfxQueue.Tick(voices, lowVolumeScale);
            enemyDeathSfxQueue.Tick(voices, lowVolumeScale);
            enemyAttackSfxQueue.Tick(voices, lowVolumeScale);
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
            if (clip == null) return;
            voices.Play(clip, 1f, randomizePitch ? UnityEngine.Random.Range(0.8f, 1.2f) : 1f);
        }

        public void PlayLowVolumeSFX(AudioClip clip, bool randomizePitch = false)
        {
            if (clip == null) return;
            voices.Play(clip, lowVolumeScale, randomizePitch ? UnityEngine.Random.Range(0.8f, 1.2f) : 1f);
        }

        /// <summary>
        /// Plays a game sound from GameSoundTable. Safe with no clips (silent, nothing logged) and from anywhere,
        /// also while paused. <paramref name="pitch"/> multiplies the entry's (random) pitch.
        /// Returns whether the sound has clips (it may still skip a restart inside its Min Interval).
        /// </summary>
        public bool Play(GameSound sound, float pitch = 1f) => Play(sound, CharacterClass.None, pitch);

        /// <summary>
        /// A player sound in a class's voice: that class's clips when the entry has some, else the shared clips.
        /// </summary>
        public bool Play(GameSound sound, CharacterClass voice, float pitch = 1f)
        {
            var table = GameSoundTable.Instance;
            if (!table.TryGet(sound, out var entry)) return false;
            AudioClip[] clips = entry.ClipsFor(voice);
            if (clips == null || clips.Length == 0) return false;

            // Real time, so UI sounds work in pause windows too
            float now = Time.unscaledTime;
            if (lastPlayed.TryGetValue(sound, out float last) && !GameSoundTable.CanRestart(last, now, entry.minInterval)) return true;

            AudioClip clip = clips[UnityEngine.Random.Range(0, clips.Length)];
            if (clip == null) return true;
            lastPlayed[sound] = now;

            float randomPitch = entry.randomizePitch ? UnityEngine.Random.Range(table.RandomPitchMin, table.RandomPitchMax) : 1f;
            voices.Play(clip, entry.volume, pitch * randomPitch);
            return true;
        }

        /// <summary>Play(sound) when the AudioManager exists, nothing otherwise. The entry point for game code.</summary>
        public static bool TryPlay(GameSound sound, float pitch = 1f)
        {
            var manager = Instance;
            return manager != null && manager.Play(sound, pitch);
        }

        /// <summary>TryPlay for player sounds, in the playing class's voice (see <see cref="Game.Progression.PlayerVoice"/>).</summary>
        public static bool TryPlay(GameSound sound, CharacterClass voice, float pitch = 1f)
        {
            var manager = Instance;
            return manager != null && manager.Play(sound, voice, pitch);
        }

        /// <summary>
        /// The XP pickup sound: each pickup in quick succession plays a little higher (PickupPitchChain),
        /// and a sweep that collects many tokens in one frame plays it once.
        /// </summary>
        public void PlayXpPickup()
        {
            var settings = GameFeelSettings.Instance;
            if (xpPitchChain.TryNext(Time.unscaledTime, Time.frameCount, settings.XpChainWindow, settings.XpChainStep,
                    settings.XpChainMaxPitch, out float pitch))
            {
                Play(GameSound.PickupXp, pitch);
            }
        }

        /// <summary>An enemy's attack sound, throttled like the hit and death sounds so a screen of shooters stays readable.</summary>
        public void PlayEnemyAttackSFX(AudioClip clip)
        {
            enemyAttackSfxQueue.Enqueue(clip);
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
            voices?.SetBaseVolume(sfxVolume * masterVolume);
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
            voices?.SetBaseVolume(sfxVolume * masterVolume);
        }

        public void ResetMusicVolume()
        {
            SetNormalVolume();
        }
    }

}