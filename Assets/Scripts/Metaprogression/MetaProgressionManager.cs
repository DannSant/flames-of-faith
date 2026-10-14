using Game.Common;
using Game.Control;
using Game.Progression;
using Game.Saving;
using Game.Scene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        [Header("Debug (right-click this component)")]
        [SerializeField] private CharacterClass debugClass = CharacterClass.Warrior;
        [Tooltip("weapon, offhand, emblem, primary, special or dash")]
        [SerializeField] private string debugNodeId = "weapon";
        [SerializeField] private int debugFavors = 10;

        private List<IMetaProgressionStateLoader> metaProgressioncomponentsList = new List<IMetaProgressionStateLoader>();

        private MetaState m_MetaState;

        // Working copies of MetaState.favors / nodeTiers; written back on save.
        private readonly Dictionary<string, int> favorsByClass = new();
        private readonly Dictionary<(string characterClass, string nodeId), int> nodeTiers = new();

        // Favors earned inside the current level. A finished level adds them to the run's total
        // (GameSession.RunFavors, saved with the run); leaving a level any other way drops them, so
        // replaying it from the pre-level save can't farm Favors. The run's total is banked into the
        // meta progression only when the run ends (death or victory), so Favors can't be spent on
        // the skill tree in the middle of a run.
        private int levelFavors;

        /// <summary>Banked Favors changed (any class).</summary>
        public event Action OnFavorsChanged;
        /// <summary>A skill tree node changed tier (class, node id).</summary>
        public event Action<CharacterClass, string> OnNodeTierChanged;
        /// <summary>A class was unlocked with Favors.</summary>
        public event Action<CharacterClass> OnClassUnlocked;
        /// <summary>Favors earned in the current level (amount gained), not banked yet.</summary>
        public event Action<int> OnRunFavorsGained;

        /// <summary>Favors earned in the current level, not added to the run yet.</summary>
        public int PendingLevelFavors => levelFavors;

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

            nodeTiers.Clear();
            foreach (var entry in m_MetaState.nodeTiers)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.characterClass) && !string.IsNullOrEmpty(entry.nodeId))
                {
                    nodeTiers[(entry.characterClass, entry.nodeId)] = Mathf.Max(0, entry.tier);
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

                m_MetaState.nodeTiers.Clear();
                foreach (var pair in nodeTiers)
                {
                    m_MetaState.nodeTiers.Add(new NodeTierState
                    {
                        characterClass = pair.Key.characterClass,
                        nodeId = pair.Key.nodeId,
                        tier = pair.Value
                    });
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
        /// Adds Favors earned (or lost, if negative) in the current level. They join the run's total
        /// when the level is finished (<see cref="CommitLevelFavors"/>).
        /// </summary>
        public void AddRunFavors(int amount)
        {
            if (amount == 0) return;
            levelFavors += amount;
            OnRunFavorsGained?.Invoke(amount);
        }

        /// <summary>The level was finished: its Favors join the run's total (saved with the run).</summary>
        public void CommitLevelFavors()
        {
            if (levelFavors != 0 && GameSession.Instance != null)
            {
                GameSession.Instance.AddRunFavors(levelFavors);
            }
            levelFavors = 0;
        }

        /// <summary>
        /// The run is over (death or victory): the run's Favors, plus the current level's, go to the
        /// class being played and are saved. Only then can they be spent.
        /// </summary>
        public void BankRunFavors()
        {
            int amount = levelFavors + (GameSession.Instance != null ? GameSession.Instance.TakeRunFavors() : 0);
            levelFavors = 0;
            if (amount <= 0) return;

            var playerClass = GetPlayedClass();
            if (playerClass == CharacterClass.None)
            {
                Debug.LogWarning($"[Meta] No player class to bank {amount} Favors for; they're lost.");
                return;
            }

            string key = playerClass.ToString();
            favorsByClass.TryGetValue(key, out int current);
            favorsByClass[key] = Mathf.Max(0, current + amount);
            Debug.Log($"[Meta] Banked {amount} Favors for {key} (now {favorsByClass[key]}).");

            SaveMetaProgression();
            OnFavorsChanged?.Invoke();
        }

        /// <summary>Drops the Favors earned in the current level (it was left without finishing it).</summary>
        public void DiscardLevelFavors()
        {
            levelFavors = 0;
        }

        // --- Class unlocks ----------------------------------------------------------------------

        /// <summary>Whether the class can be played (and its tree upgraded).</summary>
        public bool IsClassUnlocked(CharacterClass characterClass)
        {
            var tree = SkillTrees.Get(characterClass);
            return SkillTreeRules.IsClassUnlocked(
                tree != null && tree.UnlockedByDefault,
                m_MetaState != null && m_MetaState.unlockedClasses.Contains(characterClass.ToString()),
                MetaProgressionSettings.Instance.UnlockAllClasses);
        }

        /// <summary>Whether the class was bought or starts unlocked, ignoring the developer setting.</summary>
        private bool IsClassOwned(CharacterClass characterClass)
        {
            var tree = SkillTrees.Get(characterClass);
            return (tree != null && tree.UnlockedByDefault)
                || (m_MetaState != null && m_MetaState.unlockedClasses.Contains(characterClass.ToString()));
        }

        /// <summary>Favors available to unlock a class: every class's Favors combined.</summary>
        public int GetFavorsForClassUnlock() =>
            FavorRules.CalculateSpendable(favorsByClass, string.Empty, true);

        /// <summary>
        /// Buys the class with Favors from every class's pool. Returns false (and changes nothing)
        /// when it's already owned, has no tree, or there aren't enough Favors.
        /// </summary>
        public bool TryUnlockClass(CharacterClass characterClass)
        {
            var tree = SkillTrees.Get(characterClass);
            if (tree == null || IsClassOwned(characterClass)) return false;
            if (!FavorRules.TrySpend(favorsByClass, characterClass.ToString(), tree.UnlockCost, true)) return false;

            m_MetaState.unlockedClasses.Add(characterClass.ToString());
            SaveMetaProgression();

            OnFavorsChanged?.Invoke();
            OnClassUnlocked?.Invoke(characterClass);
            return true;
        }

        // --- Skill tree -------------------------------------------------------------------------

        /// <summary>The tier of a skill tree node the player has bought (0 = base item), clamped to the node.</summary>
        public int GetNodeTier(CharacterClass characterClass, SkillTreeNode node)
        {
            if (node == null) return 0;
            nodeTiers.TryGetValue((characterClass.ToString(), node.nodeId), out int tier);
            return SkillTreeRules.ClampTier(tier, node.tiers.Count);
        }

        /// <summary>Favors to buy the node's next tier, or -1 at the top tier.</summary>
        public int GetUpgradeCost(CharacterClass characterClass, SkillTreeNode node) =>
            node == null ? -1 : node.GetUpgradeCost(GetNodeTier(characterClass, node));

        public bool CanUpgradeNode(CharacterClass characterClass, SkillTreeNode node) =>
            node != null && IsClassUnlocked(characterClass) && SkillTreeRules.CanUpgrade(node.upgradeCosts, node.tiers.Count,
                GetNodeTier(characterClass, node), GetSpendableFavors(characterClass));

        /// <summary>
        /// Buys the node's next tier with Favors. Applies from the next run. Returns false (and
        /// changes nothing) at the top tier or without enough Favors.
        /// </summary>
        public bool TryUpgradeNode(CharacterClass characterClass, SkillTreeNode node)
        {
            if (!CanUpgradeNode(characterClass, node)) return false;

            int cost = GetUpgradeCost(characterClass, node);
            if (!FavorRules.TrySpend(favorsByClass, characterClass.ToString(), cost, MetaProgressionSettings.Instance.SharedFavorPool))
            {
                return false;
            }

            int newTier = GetNodeTier(characterClass, node) + 1;
            nodeTiers[(characterClass.ToString(), node.nodeId)] = newTier;
            SaveMetaProgression();

            OnFavorsChanged?.Invoke();
            OnNodeTierChanged?.Invoke(characterClass, node.nodeId);
            return true;
        }

        // --- Debug (Inspector context menu) ------------------------------------------------------

        [ContextMenu("Debug: Add Favors to Debug Class")]
        private void DebugAddFavors()
        {
            if (!IsDebugAllowed()) return;
            string key = debugClass.ToString();
            favorsByClass.TryGetValue(key, out int current);
            favorsByClass[key] = Mathf.Max(0, current + debugFavors);
            SaveMetaProgression();
            OnFavorsChanged?.Invoke();
            Debug.Log($"[Meta] {key} now has {favorsByClass[key]} Favors.");
        }

        [ContextMenu("Debug: Upgrade Debug Node")]
        private void DebugUpgradeNode()
        {
            if (!IsDebugAllowed()) return;
            var node = SkillTrees.Get(debugClass)?.Nodes.FirstOrDefault(n => n.nodeId == debugNodeId);
            if (node == null)
            {
                Debug.LogWarning($"[Meta] No node '{debugNodeId}' in the {debugClass} skill tree.");
                return;
            }

            bool upgraded = TryUpgradeNode(debugClass, node);
            Debug.Log(upgraded
                ? $"[Meta] {debugClass}/{debugNodeId} is now tier {GetNodeTier(debugClass, node)} ({GetFavors(debugClass)} Favors left)."
                : $"[Meta] Can't upgrade {debugClass}/{debugNodeId}: tier {GetNodeTier(debugClass, node)}, cost {GetUpgradeCost(debugClass, node)}, spendable {GetSpendableFavors(debugClass)}.");
        }

        [ContextMenu("Debug: Unlock Debug Class")]
        private void DebugUnlockClass()
        {
            if (!IsDebugAllowed()) return;
            bool unlocked = TryUnlockClass(debugClass);
            Debug.Log(unlocked
                ? $"[Meta] Unlocked {debugClass}."
                : $"[Meta] Can't unlock {debugClass}: owned {IsClassOwned(debugClass)}, cost {SkillTrees.Get(debugClass)?.UnlockCost}, Favors {GetFavorsForClassUnlock()}.");
        }

        [ContextMenu("Debug: Reset Favors and Skill Trees")]
        private void DebugResetSkillTrees()
        {
            if (!IsDebugAllowed()) return;
            favorsByClass.Clear();
            nodeTiers.Clear();
            m_MetaState.unlockedClasses.Clear();
            SaveMetaProgression();
            OnFavorsChanged?.Invoke();
            Debug.Log("[Meta] Favors, skill tree tiers and class unlocks reset.");
        }

        // The meta file is only loaded in Awake, so outside Play mode these would work on an empty state.
        private bool IsDebugAllowed()
        {
            if (Application.isPlaying && m_MetaState != null) return true;
            Debug.LogWarning("[Meta] The debug actions only work in Play mode.");
            return false;
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
