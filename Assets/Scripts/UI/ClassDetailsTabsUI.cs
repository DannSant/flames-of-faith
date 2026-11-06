using Game.Progression;
using Game.UI.Navigation;
using Game.UI.Style;
using System;
using System.Collections.Generic;
using Game.Control;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>The class select's detail tabs, in display order.</summary>
    public enum ClassDetailsTab
    {
        Abilities,
        Lore,
        Playstyle
    }

    /// <summary>One item of a class's starting kit, as the Abilities tab shows it (plain data).</summary>
    public class ClassKitEntry
    {
        public Sprite icon;
        public Color frameColor = Color.white;
        /// <summary>The quality-colored name (rich text): the row's label and the tooltip's title.</summary>
        public string name;
        public string description;
        public string statLines;
    }

    /// <summary>One of the class's recommended stats, for the Playstyle tab.</summary>
    public class ClassStatEntry
    {
        public string statName;
        public StatRole role;
        public string justification;
    }

    /// <summary>What the class select's tabs show for one class, built by the menu controller.</summary>
    public class ClassDetailsView
    {
        public readonly List<ClassKitEntry> kit = new();
        public string lore;
        public string playstyle;
        public readonly List<ClassStatEntry> stats = new();
    }

    /// <summary>
    /// The class select's details tabs: Abilities (the starting kit, a row per item with its tooltip), Lore and
    /// Playstyle (the class's texts, its stat guide with gold / silver stars). Every row is selectable, so the
    /// gamepad can read each item's tooltip and scroll long text; the list scrolls inside its masked view.
    /// The menu controller pushes a plain <see cref="ClassDetailsView"/>; the chosen tab stays while the player
    /// previews or chooses other classes. Navigation is linked by hand (<see cref="LinkNavigation"/>).
    /// Layout: MainMenuRestyler.
    /// </summary>
    public class ClassDetailsTabsUI : MonoBehaviour
    {
        [Header("Tabs")]
        [Tooltip("Abilities, Lore, Playstyle, in that order.")]
        [SerializeField] private Button[] tabButtons = new Button[3];
        [SerializeField] private TextMeshProUGUI[] tabLabels = new TextMeshProUGUI[3];

        [Header("List")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [Tooltip("An item row: slot frame, icon, name. Inactive template.")]
        [SerializeField] private ClassDetailsRowUI kitRowTemplate;
        [Tooltip("A paragraph of text. Inactive template.")]
        [SerializeField] private ClassDetailsRowUI textRowTemplate;
        [Tooltip("A recommended stat: star and text. Inactive template.")]
        [SerializeField] private ClassDetailsRowUI statRowTemplate;
        [SerializeField] private string statsHeader = "Stats to look for";
        [SerializeField] private Color keyStatColor = new Color(0.95f, 0.76f, 0.31f);
        [SerializeField] private Color complementaryStatColor = new Color(0.78f, 0.82f, 0.86f);

        [Header("Tooltip")]
        [SerializeField] private GeneralTooltipPaneUI tooltipPane;

        [Header("Navigation")]
        [Tooltip("The class cards, top to bottom.")]
        [SerializeField] private Selectable[] cards = new Selectable[0];
        [SerializeField] private Selectable skillTreeButton;
        [SerializeField] private Selectable backButton;
        [SerializeField] private Selectable startRunButton;

        private readonly List<ClassDetailsRowUI> rows = new();
        private ClassDetailsView view;
        private int chosenCard;
        private bool tabsHooked;

        public ClassDetailsTab CurrentTab { get; private set; } = ClassDetailsTab.Abilities;
        public IReadOnlyList<ClassDetailsRowUI> Rows => rows;

        private void Awake() => HookTabs();

        private void HookTabs()
        {
            if (tabsHooked) return;
            tabsHooked = true;
            for (int i = 0; i < tabButtons.Length; i++)
            {
                var tab = (ClassDetailsTab)i;
                if (tabButtons[i] != null) tabButtons[i].onClick.AddListener(() => SelectTab(tab));
            }
        }

        public void SetTooltipPane(GeneralTooltipPaneUI pane) => tooltipPane = pane;

        /// <summary>Shows a class's details on the current tab.</summary>
        public void Show(ClassDetailsView details)
        {
            HookTabs();
            view = details;
            Rebuild();
        }

        /// <summary>The chosen class's card: where Left from the details goes back to.</summary>
        public void SetChosenClass(CharacterClassData chosen)
        {
            chosenCard = 0;
            for (int i = 0; i < cards.Length; i++)
            {
                var card = cards[i] != null ? cards[i].GetComponent<ClassSelectHoverButton>() : null;
                if (card != null && card.ClassData == chosen) chosenCard = i;
            }
            LinkNavigation();
        }

        public void SelectTab(ClassDetailsTab tab)
        {
            CurrentTab = tab;
            Rebuild();
        }

        private void Rebuild()
        {
            HideTooltip();
            foreach (var row in rows)
            {
                if (row != null) DestroyRow(row.gameObject);
            }
            rows.Clear();

            if (view != null)
            {
                switch (CurrentTab)
                {
                    case ClassDetailsTab.Abilities:
                        foreach (var item in view.kit) AddKitRow(item);
                        break;
                    case ClassDetailsTab.Lore:
                        foreach (string paragraph in Paragraphs(view.lore)) AddTextRow(paragraph, UIColorRole.Text);
                        break;
                    case ClassDetailsTab.Playstyle:
                        foreach (string paragraph in Paragraphs(view.playstyle)) AddTextRow(paragraph, UIColorRole.Text);
                        if (view.stats.Count > 0) AddTextRow(statsHeader, UIColorRole.Highlight);
                        foreach (var stat in view.stats) AddStatRow(stat);
                        break;
                }
            }

            ApplyTabs();
            LinkNavigation();
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.StopMovement();
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private static void DestroyRow(GameObject row)
        {
            row.SetActive(false);
            if (Application.isPlaying) Destroy(row);
            else DestroyImmediate(row);
        }

        /// <summary>A text split on blank lines, each paragraph trimmed; empty parts dropped.</summary>
        public static List<string> Paragraphs(string text)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return result;
            foreach (string part in text.Replace("\r\n", "\n").Split(new[] { "\n\n" }, StringSplitOptions.None))
            {
                string trimmed = part.Trim();
                if (trimmed.Length > 0) result.Add(trimmed);
            }
            return result;
        }

        private ClassDetailsRowUI Spawn(ClassDetailsRowUI template)
        {
            if (template == null || content == null) return null;
            var row = Instantiate(template, content);
            row.gameObject.SetActive(true);
            row.OnRowSelected += ScrollToShow;
            rows.Add(row);
            return row;
        }

        private void AddKitRow(ClassKitEntry item)
        {
            var row = Spawn(kitRowTemplate);
            if (row == null) return;
            row.SetText(item.name);
            row.SetIcon(item.icon, item.frameColor);
            row.SetTooltip(anchor =>
            {
                if (tooltipPane != null) tooltipPane.ShowTooltip(item.description, item.name, null, item.statLines, anchor);
            }, HideTooltip);
        }

        private void AddTextRow(string text, UIColorRole color)
        {
            var row = Spawn(textRowTemplate);
            if (row == null) return;
            row.SetText(text);
            var settings = UISettings.Instance;
            if (settings != null) row.SetTextColor(color == UIColorRole.Highlight ? settings.HighlightColor : settings.TextColor);
        }

        private void AddStatRow(ClassStatEntry stat)
        {
            var row = Spawn(statRowTemplate);
            if (row == null) return;
            row.SetText(FormatStat(stat));
            row.SetStar(stat.role, keyStatColor, complementaryStatColor);
        }

        /// <summary>"<b>Melee Damage</b>: why it matters", or just the name without a justification.</summary>
        public static string FormatStat(ClassStatEntry stat)
        {
            string name = $"<b>{stat.statName}</b>";
            return string.IsNullOrWhiteSpace(stat.justification) ? name : $"{name}: {stat.justification.Trim()}";
        }

        private void HideTooltip()
        {
            if (tooltipPane != null) tooltipPane.HideTooltip();
        }

        // The selected tab uses the picked tab art and a yellow label, like the Stats pane's
        private void ApplyTabs()
        {
            var settings = UISettings.Instance;
            if (settings == null) return;
            for (int i = 0; i < tabButtons.Length; i++)
            {
                bool selected = i == (int)CurrentTab;
                if (tabButtons[i] != null && tabButtons[i].targetGraphic is Image image)
                {
                    var sprite = settings.GetTabSprite(selected);
                    if (sprite != null) image.sprite = sprite;
                }
                if (i < tabLabels.Length && tabLabels[i] != null) tabLabels[i].color = selected ? settings.HighlightColor : settings.TextColor;
            }
        }

        private void ScrollToShow(ClassDetailsRowUI row)
        {
            if (scrollRect == null || row == null) return;
            if (InputDeviceManager.Instance == null || !InputDeviceManager.Instance.IsGamepadActive) return;
            ScrollRectMath.ScrollToShow(scrollRect, (RectTransform)row.transform, UISettings.Instance != null ? UISettings.Instance.Spacing : 8f);
        }

        /// <summary>Links the class select by hand (see <see cref="ClassSelectNavigation"/>).</summary>
        public void LinkNavigation()
        {
            var rowSelectables = new List<Selectable>(rows.Count);
            foreach (var row in rows) rowSelectables.Add(row.GetComponent<Selectable>());
            ClassSelectNavigation.Link(cards, chosenCard, tabButtons, (int)CurrentTab, rowSelectables, skillTreeButton, backButton, startRunButton);
        }
    }

    /// <summary>
    /// The class select's gamepad links. Explicit, because the menu's other panels rest in the same spot and stay
    /// interactable while hidden, so Automatic navigation would land on their buttons (as in the Chronicle).
    /// Cards on the left; on the right the tabs, the rows of the current tab, Skill Tree; then Back and Start Run.
    /// </summary>
    public static class ClassSelectNavigation
    {
        public static void Link(IReadOnlyList<Selectable> cards, int chosenCard, IReadOnlyList<Selectable> tabs, int currentTab,
            IReadOnlyList<Selectable> rows, Selectable skillTree, Selectable back, Selectable start)
        {
            Selectable At(IReadOnlyList<Selectable> list, int i) => list != null && i >= 0 && i < list.Count ? list[i] : null;
            int cardCount = cards?.Count ?? 0;
            int rowCount = rows?.Count ?? 0;
            var chosen = At(cards, Mathf.Clamp(chosenCard, 0, Mathf.Max(0, cardCount - 1)));
            var tab = At(tabs, currentTab) ?? At(tabs, 0);
            var firstRow = At(rows, 0);
            var lastRow = At(rows, rowCount - 1);

            for (int i = 0; i < cardCount; i++)
            {
                Set(cards[i], up: At(cards, i - 1), down: i + 1 < cardCount ? cards[i + 1] : back, left: null, right: tab);
            }
            for (int i = 0; i < (tabs?.Count ?? 0); i++)
            {
                Set(tabs[i], up: null, down: firstRow ?? skillTree, left: i > 0 ? tabs[i - 1] : chosen, right: At(tabs, i + 1));
            }
            for (int i = 0; i < rowCount; i++)
            {
                Set(rows[i], up: i > 0 ? rows[i - 1] : tab, down: i + 1 < rowCount ? rows[i + 1] : skillTree, left: chosen, right: null);
            }
            Set(skillTree, up: lastRow ?? tab, down: back, left: chosen, right: null);
            Set(back, up: skillTree, down: start, left: chosen, right: null);
            Set(start, up: back, down: null, left: chosen, right: null);
        }

        private static void Set(Selectable selectable, Selectable up, Selectable down, Selectable left, Selectable right)
        {
            if (selectable == null) return;
            selectable.navigation = new UnityEngine.UI.Navigation
            {
                mode = UnityEngine.UI.Navigation.Mode.Explicit,
                selectOnUp = up,
                selectOnDown = down,
                selectOnLeft = left,
                selectOnRight = right,
            };
        }
    }
}
