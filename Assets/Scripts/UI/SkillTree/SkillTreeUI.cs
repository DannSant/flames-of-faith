using Game.Combat;
using Game.Metaprogression;
using Game.UI.Style;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.SkillTree
{
    /// <summary>
    /// The skill tree screen. Shows the <see cref="SkillTreeView"/> it's given and reports the
    /// player's requests through <see cref="Callbacks"/>; it never reads meta-progression itself
    /// (see SkillTreeController).
    ///
    /// Layout: class tabs, the class (the tree's root, which unlocks it), one node per kit item, and a
    /// details panel for whatever is hovered or selected. Clicking a node buys its next tier;
    /// clicking the root unlocks the class. Both ask for confirmation when a ConfirmDialogUI is set.
    /// </summary>
    public class SkillTreeUI : MonoBehaviour
    {
        public class Callbacks
        {
            public Action<int> onTabSelected;
            public Action<string> onUpgradeNode;
            public Action onUnlockClass;
            public Action onClose;
        }

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI favorsText;

        [Header("Class tabs")]
        [SerializeField] private List<Button> tabButtons = new();
        [SerializeField] private List<TextMeshProUGUI> tabLabels = new();
        [SerializeField] private Sprite tabSprite;
        [SerializeField] private Sprite tabSelectedSprite;

        [Header("Tree")]
        [SerializeField] private Button rootButton;
        [SerializeField] private Image rootIcon;
        [SerializeField] private TextMeshProUGUI rootLabel;
        [SerializeField] private List<SkillTreeNodeUI> nodeViews = new();

        [Header("Details")]
        [SerializeField] private TextMeshProUGUI detailsTitle;
        [SerializeField] private TextMeshProUGUI detailsBody;
        [SerializeField] private TextMeshProUGUI detailsFooter;

        [Header("Footer")]
        [SerializeField] private Button closeButton;

        [Header("Confirmation (optional, usually the scene's dialog)")]
        [SerializeField] private ConfirmDialogUI confirmDialog;

        [Header("Texts")]
        [SerializeField] private string favorsFormat = "Favors: {0}";
        [SerializeField] private string sharedFavorsFormat = "Favors: {0}";
        [SerializeField] private string lockedSuffix = " (Locked)";
        [SerializeField] private string nextTierFormat = "Next: {0}";
        [SerializeField] private string maxTierText = "Highest tier reached.";
        [SerializeField] private string upgradeCostFormat = "Upgrade: {0} Favors. Click to buy.";
        [SerializeField] private string notEnoughFavorsFormat = "Upgrade: {0} Favors. Not enough Favors.";
        [SerializeField] private string classLockedNodeText = "Unlock the class to upgrade its items.";
        [SerializeField] private string unlockCostFormat = "Unlock: {0} Favors (you have {1}). Click to unlock.";
        [SerializeField] private string unlockNotEnoughFormat = "Unlock: {0} Favors (you have {1}).";
        [SerializeField] private string unlockedText = "Unlocked.";
        [SerializeField] private string confirmUpgradeFormat = "Upgrade to {0} for {1} Favors?";
        [SerializeField] private string confirmUnlockFormat = "Unlock {0} for {1} Favors?";
        // The "next tier" header uses the Highlight (yellow) color from UISettings

        private SkillTreeView view;
        private Callbacks callbacks;
        // Null = the root (class) is the one described.
        private string focusedNodeId;
        private bool wired;

        public bool IsOpen => gameObject.activeSelf;

        private void Wire()
        {
            if (wired) return;
            wired = true;

            for (int i = 0; i < tabButtons.Count; i++)
            {
                int tab = i;
                if (tabButtons[i] != null) tabButtons[i].onClick.AddListener(() => callbacks?.onTabSelected?.Invoke(tab));
            }

            if (rootButton != null)
            {
                rootButton.onClick.AddListener(RequestUnlock);
                var relay = rootButton.GetComponent<UIFocusRelay>();
                if (relay == null) relay = rootButton.gameObject.AddComponent<UIFocusRelay>();
                relay.OnFocused += () => Focus(null);
            }

            foreach (var node in nodeViews)
            {
                if (node == null) continue;
                node.OnFocused += n => Focus(n.NodeId);
                node.OnClicked += n => RequestUpgrade(n.NodeId);
            }

            if (closeButton != null) closeButton.onClick.AddListener(RequestClose);
        }

        /// <summary>Opens the screen (if needed) and shows <paramref name="newView"/>.</summary>
        public void Show(SkillTreeView newView, Callbacks newCallbacks)
        {
            Wire();
            bool opening = !gameObject.activeSelf;

            view = newView;
            callbacks = newCallbacks;
            if (opening)
            {
                focusedNodeId = null;
                gameObject.SetActive(true);
            }

            Render();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>Close button and Cancel (B / Esc, through UIWindow.OnCancel).</summary>
        public void RequestClose() => callbacks?.onClose?.Invoke();

        private void Render()
        {
            if (view == null) return;
            var settings = UISettings.Instance;

            if (favorsText != null)
            {
                favorsText.text = string.Format(view.sharedFavorPool ? sharedFavorsFormat : favorsFormat, view.favors);
                if (settings != null) favorsText.color = settings.FavorsColor;
            }
            // The footer is the cost in Favors
            if (detailsFooter != null && settings != null) detailsFooter.color = settings.FavorsColor;

            for (int i = 0; i < tabButtons.Count; i++)
            {
                bool used = i < view.tabs.Count;
                if (tabButtons[i] == null) continue;
                tabButtons[i].gameObject.SetActive(used);
                if (!used) continue;

                var tab = view.tabs[i];
                bool selected = i == view.selectedTab;
                if (tabButtons[i].image != null && tabSprite != null)
                {
                    tabButtons[i].image.sprite = selected && tabSelectedSprite != null ? tabSelectedSprite : tabSprite;
                }
                if (i < tabLabels.Count && tabLabels[i] != null)
                {
                    tabLabels[i].text = tab.unlocked ? tab.name : tab.name + lockedSuffix;
                    if (settings != null) tabLabels[i].color = GetTabLabelColor(selected, tab.unlocked, settings);
                }
            }

            if (rootIcon != null)
            {
                rootIcon.sprite = view.classIcon;
                rootIcon.enabled = view.classIcon != null;
                rootIcon.color = view.classUnlocked ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1f);
            }
            if (rootLabel != null) rootLabel.text = view.className;

            for (int i = 0; i < nodeViews.Count; i++)
            {
                if (nodeViews[i] == null) continue;
                bool used = i < view.nodes.Count;
                nodeViews[i].gameObject.SetActive(used);
                if (used) nodeViews[i].Setup(view.nodes[i], !view.classUnlocked);
            }

            RenderDetails();
        }

        private void Focus(string nodeId)
        {
            focusedNodeId = nodeId;
            RenderDetails();
        }

        /// <summary>A class tab's label, like the Stats pane's tabs: yellow when selected, white, or muted while the class is locked.</summary>
        public static Color GetTabLabelColor(bool selected, bool unlocked, UISettings settings)
        {
            if (selected) return settings.HighlightColor;
            return unlocked ? settings.TextColor : settings.MutedTextColor;
        }

        private void RenderDetails()
        {
            if (view == null) return;

            var node = FindNode(focusedNodeId);
            if (node == null)
            {
                SetDetails(view.className,
                    DamageTypeColorHelper.ColorizeMetaTags(view.classDescription ?? ""),
                    view.classUnlocked
                        ? unlockedText
                        : string.Format(view.canUnlockClass ? unlockCostFormat : unlockNotEnoughFormat, view.classUnlockCost, view.favorsForUnlock));
                return;
            }

            string body = node.currentDescription;
            string footer;
            if (node.IsMaxed)
            {
                body += "\n\n" + maxTierText;
                footer = "";
            }
            else
            {
                body += $"\n\n<color=#{UISettings.Instance.ToHex(UIColorRole.Highlight)}>{string.Format(nextTierFormat, node.nextName)}</color>\n{node.nextDescription}";
                footer = !view.classUnlocked
                    ? classLockedNodeText
                    : string.Format(node.canUpgrade ? upgradeCostFormat : notEnoughFavorsFormat, node.upgradeCost);
            }

            SetDetails($"{node.title}: {node.currentName}", body, footer);
        }

        private void SetDetails(string title, string body, string footer)
        {
            if (detailsTitle != null) detailsTitle.text = title;
            if (detailsBody != null) detailsBody.text = body;
            if (detailsFooter != null) detailsFooter.text = footer;
        }

        private SkillTreeNodeView FindNode(string nodeId)
        {
            if (view == null || string.IsNullOrEmpty(nodeId)) return null;
            return view.nodes.Find(n => n.nodeId == nodeId);
        }

        private void RequestUpgrade(string nodeId)
        {
            Focus(nodeId);
            var node = FindNode(nodeId);
            if (node == null || !node.canUpgrade) return;

            Confirm(string.Format(confirmUpgradeFormat, node.nextName, node.upgradeCost),
                () => callbacks?.onUpgradeNode?.Invoke(nodeId));
        }

        private void RequestUnlock()
        {
            Focus(null);
            if (view == null || view.classUnlocked || !view.canUnlockClass) return;

            Confirm(string.Format(confirmUnlockFormat, view.className, view.classUnlockCost),
                () => callbacks?.onUnlockClass?.Invoke());
        }

        private void Confirm(string message, Action onConfirm)
        {
            if (confirmDialog == null)
            {
                onConfirm();
                return;
            }

            // Gamepad focus goes back to the button that asked once the dialog closes
            var asker = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            void Restore()
            {
                if (asker != null && asker.activeInHierarchy && EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(asker);
                }
            }

            confirmDialog.Show(message, () => { onConfirm(); Restore(); }, Restore);
        }
    }
}
