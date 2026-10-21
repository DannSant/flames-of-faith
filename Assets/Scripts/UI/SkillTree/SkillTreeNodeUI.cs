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
    /// One node of the skill tree: the item's icon at its current tier, a pip per tier, and the
    /// cost of the next one. Reports focus (hover / gamepad select) and clicks to <see cref="SkillTreeUI"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SkillTreeNodeUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI costText;

        [Header("Tier pips")]
        [Tooltip("Inactive template, cloned once per tier.")]
        [SerializeField] private Image pipTemplate;
        // Owned pips use the Highlight (yellow) color from UISettings
        [SerializeField] private Color pipEmptyColor = new Color(1f, 1f, 1f, 0.25f);

        // The cost uses UISettings: Positive (green) when affordable or maxed ("MAX"), Negative (red) otherwise
        [Tooltip("Icon tint while the class is locked.")]
        [SerializeField] private Color lockedIconTint = new Color(0.4f, 0.4f, 0.4f, 1f);

        private readonly List<Image> pips = new();
        private Button button;

        public string NodeId { get; private set; }
        public Button Button => button != null ? button : button = GetComponent<Button>();

        public event Action<SkillTreeNodeUI> OnFocused;
        public event Action<SkillTreeNodeUI> OnClicked;

        private void Awake()
        {
            Button.onClick.AddListener(() => OnClicked?.Invoke(this));
            if (pipTemplate != null) pipTemplate.gameObject.SetActive(false);
        }

        public void Setup(SkillTreeNodeView view, bool classLocked)
        {
            NodeId = view.nodeId;

            if (titleText != null) titleText.text = view.title;

            if (icon != null)
            {
                icon.sprite = view.icon;
                icon.enabled = view.icon != null;
                icon.color = classLocked ? lockedIconTint : Color.white;
            }

            if (costText != null)
            {
                costText.text = view.IsMaxed ? "MAX" : view.upgradeCost.ToString();
                costText.color = GetCostColor(view.IsMaxed, view.canUpgrade, UISettings.Instance);
            }

            UpdatePips(view.tier, view.maxTier);
        }

        /// <summary>The node's cost label, like the shop's prices: green when it can be bought (or is maxed), red when not.</summary>
        public static Color GetCostColor(bool maxed, bool canUpgrade, UISettings settings)
        {
            return maxed || canUpgrade ? settings.PositiveColor : settings.NegativeColor;
        }

        private void UpdatePips(int tier, int maxTier)
        {
            if (pipTemplate == null) return;

            int count = maxTier + 1;
            while (pips.Count < count)
            {
                var pip = Instantiate(pipTemplate, pipTemplate.transform.parent);
                pip.gameObject.SetActive(true);
                pips.Add(pip);
            }

            for (int i = 0; i < pips.Count; i++)
            {
                bool used = i < count;
                pips[i].gameObject.SetActive(used);
                if (used) pips[i].color = i <= tier ? UISettings.Instance.HighlightColor : pipEmptyColor;
            }
        }

        public void OnSelect(BaseEventData eventData) => OnFocused?.Invoke(this);

        public void OnPointerEnter(PointerEventData eventData) => OnFocused?.Invoke(this);
    }
}
