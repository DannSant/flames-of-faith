using Game.Progression;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Saving
{
    /// <summary>
    /// Everything needed to continue a run after the game is closed. Written by RunSaveService.
    /// ScriptableObject references are stored by id/name so the file survives asset changes,
    /// and enums are stored by name so reordering them can't silently remap saved values.
    /// </summary>
    [Serializable]
    public class RunSaveData
    {
        // 2: class kits became several items (weapon, off-hand, Emblem, Glyphs); older runs lack them.
        public const int CurrentVersion = 2;

        public int saveVersion = CurrentVersion;

        [Header("Session")]
        public int selectedPlayerIndex;
        public string difficulty;
        public int levelsBeaten;
        // Favors earned in the run's finished levels, banked when the run ends. Missing from older
        // saves, which then read 0 (no version bump needed).
        public int runFavors;
        // False while the player hasn't entered a level yet; continuing then starts the run fresh
        public bool runStarted;

        [Header("Player")]
        public float currentHealth;
        public float currentGrace;
        public int currencyAmount;
        public PlayerExperienceData experience;
        public List<StatSaveEntry> stats = new();
        public List<EffectSaveEntry> effects = new();

        [Header("Map")]
        public RunMapSaveData map = new();
    }

    [Serializable]
    public class StatSaveEntry
    {
        public string stat;
        public int value;
    }

    [Serializable]
    public class EffectSaveEntry
    {
        public string effectId;
        public int count;
    }

    [Serializable]
    public class RunMapSaveData
    {
        public int seed;
        public int actNumber;
        public string mapId;
        public string currentNodeId;
        public List<NodeSaveEntry> nodes = new();
        public List<EdgeSaveEntry> edges = new();
    }

    [Serializable]
    public class NodeSaveEntry
    {
        public string id;
        public string state;
    }

    [Serializable]
    public class EdgeSaveEntry
    {
        public string fromNodeId;
        public string toNodeId;
        public bool enabled;
    }
}
