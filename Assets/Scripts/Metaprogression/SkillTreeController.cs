using Game.Control;
using Game.UI.SkillTree;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Metaprogression
{
    /// <summary>
    /// Drives the skill tree screen: builds a <see cref="SkillTreeView"/> from the class's skill
    /// tree and meta progress, pushes it to <see cref="SkillTreeUI"/>, and carries out the UI's
    /// requests (switch class, upgrade a node, unlock the class). The UI only receives plain data
    /// and callbacks.
    /// </summary>
    public class SkillTreeController : MonoBehaviour
    {
        [SerializeField] private SkillTreeUI ui;
        [Tooltip("The classes shown as tabs, in order.")]
        [SerializeField] private List<CharacterClassData> classes = new();

        private int selectedTab;
        private SkillTreeUI.Callbacks callbacks;

        private void Awake()
        {
            callbacks = new SkillTreeUI.Callbacks
            {
                onTabSelected = SelectTab,
                onUpgradeNode = UpgradeNode,
                onUnlockClass = UnlockClass,
                onClose = Close
            };
        }

        private void OnEnable()
        {
            if (MetaProgressionManager.Instance != null)
            {
                MetaProgressionManager.Instance.OnFavorsChanged += Refresh;
            }
        }

        private void OnDisable()
        {
            if (MetaProgressionManager.Instance != null)
            {
                MetaProgressionManager.Instance.OnFavorsChanged -= Refresh;
            }
        }

        /// <summary>Opens the skill tree on the first class (hook a menu button to this).</summary>
        public void Open() => Open(0);

        /// <summary>Opens the skill tree on a class (e.g. the one hovered in class select).</summary>
        public void Open(CharacterClass characterClass)
        {
            int index = classes.FindIndex(c => c != null && c.characterClass == characterClass);
            Open(Mathf.Max(0, index));
        }

        private void Open(int tab)
        {
            if (ui == null || classes.Count == 0) return;
            selectedTab = Mathf.Clamp(tab, 0, classes.Count - 1);
            ui.Show(BuildView(), callbacks);
        }

        public void Close()
        {
            if (ui != null) ui.Hide();
        }

        private void SelectTab(int tab)
        {
            selectedTab = Mathf.Clamp(tab, 0, classes.Count - 1);
            Refresh();
        }

        private void UpgradeNode(string nodeId)
        {
            var meta = MetaProgressionManager.Instance;
            var node = FindNode(nodeId);
            if (meta == null || node == null) return;

            // OnFavorsChanged refreshes the screen
            meta.TryUpgradeNode(SelectedClass, node);
        }

        private void UnlockClass()
        {
            MetaProgressionManager.Instance?.TryUnlockClass(SelectedClass);
        }

        private void Refresh()
        {
            if (ui != null && ui.IsOpen) ui.Show(BuildView(), callbacks);
        }

        private CharacterClass SelectedClass => classes[selectedTab].characterClass;

        private SkillTreeNode FindNode(string nodeId)
        {
            var tree = SkillTrees.Get(SelectedClass);
            if (tree == null) return null;
            foreach (var node in tree.Nodes)
            {
                if (node.nodeId == nodeId) return node;
            }
            return null;
        }

        private SkillTreeView BuildView()
        {
            var meta = MetaProgressionManager.Instance;
            var classData = classes[selectedTab];
            var characterClass = classData.characterClass;
            var tree = SkillTrees.Get(characterClass);

            var view = new SkillTreeView
            {
                selectedTab = selectedTab,
                className = classData.characterName,
                classDescription = classData.classDescription,
                classIcon = classData.defaultSprite,
                classUnlocked = meta == null || meta.IsClassUnlocked(characterClass),
                classUnlockCost = tree != null ? tree.UnlockCost : 0,
                favorsForUnlock = meta != null ? meta.GetFavorsForClassUnlock() : 0,
                favors = meta != null ? meta.GetSpendableFavors(characterClass) : 0,
                sharedFavorPool = MetaProgressionSettings.Instance.SharedFavorPool,
            };
            view.canUnlockClass = !view.classUnlocked && view.favorsForUnlock >= view.classUnlockCost;

            foreach (var entry in classes)
            {
                if (entry == null) continue;
                view.tabs.Add(new SkillTreeClassTabView
                {
                    name = entry.characterName,
                    unlocked = meta == null || meta.IsClassUnlocked(entry.characterClass)
                });
            }

            if (tree == null) return view;

            foreach (var node in tree.Nodes)
            {
                int tier = meta != null ? meta.GetNodeTier(characterClass, node) : 0;
                var current = node.GetTier(tier);
                var next = node.GetTier(tier + 1);

                view.nodes.Add(new SkillTreeNodeView
                {
                    nodeId = node.nodeId,
                    title = string.IsNullOrEmpty(node.title) ? node.nodeId : node.title,
                    icon = current != null ? current.EffectIcon : null,
                    tier = tier,
                    maxTier = node.MaxTier,
                    currentName = current != null ? current.EffectName : "",
                    currentDescription = current != null ? current.GetFormattedDescription() : "",
                    nextName = next != null ? next.EffectName : null,
                    nextDescription = next != null ? next.GetFormattedDescription() : null,
                    upgradeCost = meta != null ? meta.GetUpgradeCost(characterClass, node) : node.GetUpgradeCost(tier),
                    canUpgrade = meta != null && meta.CanUpgradeNode(characterClass, node),
                });
            }

            return view;
        }
    }
}
