using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// One entry per GameSound: its clips (one picked at random), volume, pitch and how often it may restart.
    /// An entry with no clips is silent and logs nothing, so hooks can ship before their clips
    /// (Tools/Flames of Faith/Audio/Report Silent Sounds lists them). Loaded from Resources/Audio/GameSoundTable.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSoundTable", menuName = "Audio/Game Sound Table")]
    public class GameSoundTable : ScriptableObject
    {
        private const string ResourcePath = "Audio/GameSoundTable";

        [Serializable]
        public class Entry
        {
            public GameSound sound;
            [Tooltip("One of these plays at random. Empty = silent.")]
            public AudioClip[] clips = new AudioClip[0];
            [Tooltip("Volume of this sound, on top of the SFX and master volumes.")]
            [Range(0f, 1f)]
            public float volume = 1f;
            [Tooltip("Play at a random pitch between the table's Random Pitch Min and Max.")]
            public bool randomizePitch;
            [Tooltip("Seconds before this sound can start again (real time). Keeps sounds that can fire many times a frame from piling up.")]
            [Min(0f)]
            public float minInterval = 0.05f;

            public bool HasClips => clips != null && clips.Length > 0;
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
            return now - lastPlayed >= minInterval;
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
