using System;
using Game.Control;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// One entry per GameSound: its clips (one picked at random), volume, pitch and how often it may restart.
    /// An entry with no clips is silent and logs nothing, so hooks can ship before their clips
    /// (Tools/Flames of Faith/Audio/Report Silent Sounds lists them). Player sounds can have a voice per class
    /// (Class Clips), which wins over the shared clips for that class. Loaded from Resources/Audio/GameSoundTable.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSoundTable", menuName = "Audio/Game Sound Table")]
    public class GameSoundTable : ScriptableObject
    {
        private const string ResourcePath = "Audio/GameSoundTable";

        [Serializable]
        public class ClassClips
        {
            public CharacterClass characterClass;
            [Tooltip("This class's voice for the sound; one plays at random.")]
            public AudioClip[] clips = new AudioClip[0];
        }

        [Serializable]
        public class Entry
        {
            public GameSound sound;
            [Tooltip("One of these plays at random. Empty = silent. For a player sound, the fallback of a class with no Class Clips.")]
            public AudioClip[] clips = new AudioClip[0];
            [Tooltip("Player sounds only (hurt, dodge, dash, level-up, death...): a voice per class, played instead of Clips for that class.")]
            public List<ClassClips> classClips = new List<ClassClips>();
            [Tooltip("Volume of this sound, on top of the SFX and master volumes.")]
            [Range(0f, 1f)]
            public float volume = 1f;
            [Tooltip("Play at a random pitch between the table's Random Pitch Min and Max.")]
            public bool randomizePitch;
            [Tooltip("Seconds before this sound can start again (real time). Keeps sounds that can fire many times a frame from piling up.")]
            [Min(0f)]
            public float minInterval = 0.05f;

            /// <summary>Whether any clip is set, shared or per class (false = silent).</summary>
            public bool HasClips => HasAny(clips) || (classClips != null && classClips.Exists(c => c != null && HasAny(c.clips)));

            /// <summary>The clips that play for a class: its own when it has some, else the shared ones.</summary>
            public AudioClip[] ClipsFor(CharacterClass voice)
            {
                if (voice != CharacterClass.None && classClips != null)
                {
                    foreach (var classEntry in classClips)
                    {
                        if (classEntry != null && classEntry.characterClass == voice && HasAny(classEntry.clips)) return classEntry.clips;
                    }
                }
                return clips;
            }

            public bool HasClipsFor(CharacterClass voice) => HasAny(ClipsFor(voice));

            private static bool HasAny(AudioClip[] list) => list != null && list.Length > 0;
        }

        [Tooltip("Lowest pitch of a sound with Randomize Pitch.")]
        [SerializeField] private float randomPitchMin = 0.9f;
        [Tooltip("Highest pitch of a sound with Randomize Pitch.")]
        [SerializeField] private float randomPitchMax = 1.1f;
        [SerializeField] private List<Entry> entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => entries;
        public float RandomPitchMin => randomPitchMin;
        public float RandomPitchMax => Mathf.Max(randomPitchMin, randomPitchMax);

        public bool TryGet(GameSound sound, out Entry entry)
        {
            foreach (var candidate in entries)
            {
                if (candidate != null && candidate.sound == sound)
                {
                    entry = candidate;
                    return true;
                }
            }
            entry = null;
            return false;
        }

        /// <summary>Whether a sound last started at <paramref name="lastPlayed"/> may start again at <paramref name="now"/>.</summary>
        public static bool CanRestart(float lastPlayed, float now, float minInterval)
        {
            // The tolerance absorbs float rounding (1.05f - 1f is 0.0499999), so the boundary itself counts
            return now - lastPlayed >= minInterval - 1e-5f;
        }

        private static GameSoundTable instance;

        public static GameSoundTable Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<GameSoundTable>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning($"GameSoundTable not found at Resources/{ResourcePath}. Every game sound is silent.");
                        instance = CreateInstance<GameSoundTable>();
                    }
                }
                return instance;
            }
        }
    }
}
