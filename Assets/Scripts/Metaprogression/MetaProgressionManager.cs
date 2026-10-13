using Game.Common;
using Game.Control;
using Game.Progression;
using Game.Saving;
using Game.Scene;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game.Metaprogression
{
    [System.Serializable]
    public class MetaState
    {
        public const int CurrentVersion = 1;

        // 0 = a file from before Favors existed; it loads fine (the new lists start empty).
        public int saveVersion = CurrentVersion;
        public string playerName = "Player";
        public List<string> unlockedEffects = null;

        // Enums are stored by name, so reordering CharacterClass can't remap saved values.
        public List<ClassFavors> favors = new();
        public List<string> unlockedClasses = new();
        public List<NodeTierState> nodeTiers = new();

        /// <summary>Lists missing from an older file come back null.</summary>
        public void EnsureLists()
        {
            favors ??= new List<ClassFavors>();
            unlockedClasses ??= new List<string>();
            nodeTiers ??= new List<NodeTierState>();
        }
    }

    [System.Serializable]
    public class ClassFavors
    {
        public string characterClass;
        public int amount;
    }

    [System.Serializable]
    public class NodeTierState
    {
        public string characterClass;
        public string nodeId;
        public int tier;
    }

    public class MetaProgressionManager : Singleton<MetaProgressionManager>
    {
        [SerializeField] private List<GameObject> gameObjectsWithMetaProgression = new List<GameObject>();

        private List<IMetaProgressionStateLoader> metaProgressioncomponentsList = new List<IMetaProgressionStateLoader>();

        private MetaState m_MetaState;

        // Working copy of MetaState.favors; written back on save.
        private readonly Dictionary<string, int> favorsByClass = new();

        // Favors earned inside the current level. They are only banked when the level ends (finished,
        // death, boss kill), so quitting mid-level and replaying it from the pre-level save can't farm them.
        private int pendingRunFavors;
        private CharacterClass pendingRunFavorsClass = CharacterClass.None;

        /// <summary>Banked Favors changed (any class).</summary>
        public event Action OnFavorsChanged;
        /// <summary>Favors earned in the current level (amount gained), not banked yet.</summary>
        public event Action<int> OnRunFavorsGained;

        public int PendingRunFavors => pendingRunFavors;

        private string SaveFilePath =>
             Path.Combine(Application.persistentDataPath, $"{GetPlayerName()}_meta.dat");

        protected override void Awake()
        {
            base.Awake();
            BuildMetaProgressionList();
            LoadMetaProgression();
        }

        private void BuildMetaProgressionList()
        {
            metaProgressioncomponentsList.Clear();
            foreach (var go in gameObjectsWithMetaProgression)
            {
                var loader = go.GetComponent<IMetaProgressionStateLoader>();
                if (loader != null && !metaProgressioncomponentsList.Contains(loader))
                {
                    metaProgressioncomponentsList.Add(loader);
                }
            }
        }

        private void LoadMetaProgression()
        {
            // Load the meta state from file, if failed or not found, return a new MetaState with null properties
            m_MetaState = LoadStateFromFile();
            m_MetaState.EnsureLists();

            favorsByClass.Clear();
            foreach (var entry in m_MetaState.favors)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.characterClass))
                {
                    favorsByClass[entry.characterClass] = Mathf.Max(0, entry.amount);
                }
            }

            if (metaProgressioncomponentsList == null || metaProgressioncomponentsList.Count == 0)
            {
                Debug.LogWarning("No MetaProgression components registered in GameSession.");
                return;
            }

            //For each component with metaprogression state, load the state and initialize it
            foreach (var loader in metaProgressioncomponentsList)
            {
                loader.LoadState(m_MetaState);
                loader.Initialize();
            }
        }

        private void SaveMetaProgression()
        {
            try
            {
                m_MetaState.saveVersion = MetaState.CurrentVersion;
                m_MetaState.favors.Clear();
                foreach (var pair in favorsByClass)
                {
                    m_MetaState.favors.Add(new ClassFavors { characterClass = pair.Key, amount = pair.Value });
                }

                SaveFileIO.WriteAtomically(SaveFilePath, JsonUtility.ToJson(m_MetaState, Application.isEditor));
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to save meta progression: " + ex);
            }
        }

        private MetaState LoadStateFromFile()
        {
            //Check if the file exists, if not return a new MetaState
            //The new metastate will have null properties, this will indicate it is a fresh start
            try
            {
                if (!SaveFileIO.TryReadJson(SaveFilePath, out string json))
                    return new MetaState();
                return JsonUtility.FromJson<MetaState>(json) ?? new MetaState();
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed loading meta progression: " + ex);
                return new MetaState();
            }
        }

        public string GetPlayerName()
        {
            if (m_MetaState == null)
                return "Player";
            return m_MetaState.playerName;
        }

        public void SaveUnlockedEffects(List<string> unlockedEffectIDs)
        {
            m_MetaState.unlockedEffects = unlockedEffectIDs;
            SaveMetaProgression();
        }

        // --- Favors ---------------------------------------------------------------------------

        /// <summary>Banked Favors earned with <paramref name="characterClass"/>.</summary>
        public int GetFavors(CharacterClass characterClass) =>
            favorsByClass.TryGetValue(characterClass.ToString(), out int amount) ? amount : 0;

        /// <summary>Favors <paramref name="characterClass"/> can spend (all classes' when the pool is shared).</summary>
        public int GetSpendableFavors(CharacterClass characterClass) =>
            FavorRules.CalculateSpendable(favorsByClass, characterClass.ToString(), MetaProgressionSettings.Instance.SharedFavorPool);

        /// <summary>
        /// Adds Favors earned (or lost, if negative) in the current level for the class being played.
        /// They count once <see cref="BankRunFavors"/> runs at the end of the level.
        /// </summary>
        public void AddRunFavors(int amount)
        {
            if (amount == 0) return;

            var playerClass = GetPlayedClass();
            if (playerClass == CharacterClass.None)
            {
                Debug.LogWarning("[Meta] No player class to earn Favors for.");
                return;
            }

            pendingRunFavorsClass = playerClass;
            pendingRunFavors += amount;
            OnRunFavorsGained?.Invoke(amount);
        }

        /// <summary>Saves the Favors earned in the current level. Call when the level ends (finished, death, boss kill).</summary>
        public void BankRunFavors()
        {
            if (pendingRunFavors == 0 || pendingRunFavorsClass == CharacterClass.None)
            {
                DiscardRunFavors();
                return;
            }

            string key = pendingRunFavorsClass.ToString();
            favorsByClass.TryGetValue(key, out int current);
            favorsByClass[key] = Mathf.Max(0, current + pendingRunFavors);
            Debug.Log($"[Meta] Banked {pendingRunFavors} Favors for {key} (now {favorsByClass[key]}).");

            DiscardRunFavors();
            SaveMetaProgression();
            OnFavorsChanged?.Invoke();
        }

        /// <summary>Drops the Favors earned in the current level (it was left without finishing it).</summary>
        public void DiscardRunFavors()
        {
            pendingRunFavors = 0;
            pendingRunFavorsClass = CharacterClass.None;
        }

        private static CharacterClass GetPlayedClass()
        {
            var classManager = PlayerManager.Instance != null
                ? PlayerManager.Instance.GetPlayerComponent<PlayerClassManager>()
                : null;
            return classManager != null ? classManager.CharacterClass : CharacterClass.None;
        }
    }

}
