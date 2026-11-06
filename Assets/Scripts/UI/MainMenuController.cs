using Game.Metaprogression;
using Game.Combat;
using Game.Control;
using Game.Cutscenes;
using Game.Effects;
using Game.Progression;
using Game.Saving;
using Game.Scene;
using Game.UI.Chronicle;
using Game.UI.Navigation;
using Game.UI.Overworld;
using Game.UI.Style;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// The main menu: the menu panel, the class select, the Settings panel and the Pilgrim's Chronicle, all resting at their spot on the right.
    /// Switching panels fades and slides the old one out and the new one in (in code, on unscaled time).
    /// Class select: clicking a card (A with the gamepad) chooses the class; hovering or moving onto one previews it; Start Run starts.
    /// Layout: MainMenuRestyler.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {

        [Header("Gamepad navigation")]
        [Tooltip("UIWindows on the three panels, set to Manual open mode - the panels stay active and " +
                 "fade in and out in place, so the focus system is told which one is showing.")]
        [SerializeField] private UIWindow mainPanelWindow;
        [SerializeField] private UIWindow characterSelectWindow;
        [SerializeField] private UIWindow settingsWindow;
        [SerializeField] private UIWindow chronicleWindow;

        [Header("Transitions")]
        [Tooltip("How long the panel being left takes to fade out (unscaled seconds).")]
        [SerializeField] private float hideDuration = 0.15f;
        [Tooltip("How long the panel being opened takes to fade in (unscaled seconds).")]
        [SerializeField] private float showDuration = 0.25f;
        [Tooltip("How far (canvas units) a panel slides to the right as it fades out, and from where it slides in.")]
        [SerializeField] private float slideDistance = 60f;

        [Header("Class select")]
        [Tooltip("The classes in GameSession.SelectedPlayerIndex order (PlayerManager's player prefabs).")]
        [SerializeField] private CharacterClassData[] classesByPlayerIndex;
        [SerializeField] private Button startRunButton;
        [SerializeField] private SkillTreeController skillTree;
        [SerializeField] private TextMeshProUGUI classNameText;
        [SerializeField] private TextMeshProUGUI classDescriptionText;
        [Tooltip("The Abilities / Lore / Playstyle tabs under the description.")]
        [SerializeField] private ClassDetailsTabsUI detailsTabs;
        [Tooltip("Green when the class can be played, red with the unlock cost while it's locked.")]
        [SerializeField] private TextMeshProUGUI classFooterText;
        [SerializeField] private string unlockedText = "Unlocked";
        [Tooltip("Footer of a locked class. {0} = unlock cost in Favors.")]
        [SerializeField] private string classLockedInfo = "Locked. Unlock it in the skill tree for {0} Favors.";
        [Tooltip("Shown when Start Run is pressed on a locked class. {0} = class name, {1} = unlock cost in Favors.")]
        [SerializeField] private string classLockedMessage = "{0} is locked. Unlock it in the skill tree for {1} Favors.";

        [Header("Saved Run")]
        [Tooltip("Continues the saved run. Disabled when there is no save.")]
        [SerializeField] private Button loadGameButton;
        [Tooltip("Tint of the Load Game button's image while there is no save, so it reads as grayed out.")]
        [SerializeField] private Color loadGameDisabledTint = new Color32(82, 57, 57, 255);
        [Tooltip("Optional. The saved run under Load Game.")]
        [SerializeField] private TextMeshProUGUI saveInfoText;
        [Tooltip("{0} = class, {1} = act in Roman numerals, {2} = character level.")]
        [SerializeField] private string saveInfoFormat = "{0} - Act {1} - Level {2}";
        [SerializeField] private string noSaveText = "No saved run";
        [SerializeField] private ConfirmDialogUI confirmDialog;
        [TextArea]
        [SerializeField] private string newGameOverwriteWarning = "Starting a new game will delete your current run. Continue?";
        [Tooltip("Plays after Start Run, right before the map, until the player has seen it once.")]
        [SerializeField] private string openingCutsceneId = "opening";

        private SettingsPanelController settingsPanel;
        private ChronicleUI chronicle;
        private readonly Dictionary<UIWindow, Vector2> shownPositions = new();
        private readonly List<ClassSelectHoverButton> cards = new();
        private Coroutine transition;
        private int selectedIndex = -1;
        private CharacterClassData shownClass;

        private bool Transitioning => transition != null;

        private void Awake()
        {
            // The panels rest at their shown spot; remember it before hiding the ones that start closed
            foreach (var window in new[] { mainPanelWindow, characterSelectWindow, settingsWindow, chronicleWindow })
            {
                if (window != null) shownPositions[window] = ((RectTransform)window.transform).anchoredPosition;
            }
        }

        private void Start()
        {
            cards.AddRange(FindObjectsByType<ClassSelectHoverButton>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            foreach (var card in cards)
            {
                card.OnHoverEnter += ShowClassInfo;
                card.OnHoverExit += HideClassInfo;
            }
            if (startRunButton != null) startRunButton.onClick.AddListener(StartRun);

            // The menu opens on the main panel; the other two are hidden in place
            SetShown(mainPanelWindow, true);
            SetShown(characterSelectWindow, false);
            SetShown(settingsWindow, false);
            SetShown(chronicleWindow, false);

            // The settings panel's Back button / Cancel ask whoever opened it to close it
            settingsPanel = settingsWindow != null ? settingsWindow.GetComponentInChildren<SettingsPanelController>(true) : null;
            if (settingsPanel != null) settingsPanel.OnCloseRequested += CloseSettings;

            chronicle = chronicleWindow != null ? chronicleWindow.GetComponent<ChronicleUI>() : null;
            if (chronicle != null) chronicle.OnCloseRequested += CloseChronicle;

            RefreshLoadGameButton();
            RefreshClassLocks();

            if (MetaProgressionManager.Instance != null)
            {
                MetaProgressionManager.Instance.OnClassUnlocked += HandleClassUnlocked;
                MetaProgressionManager.Instance.OnNodeTierChanged += HandleNodeTierChanged;
            }
        }

        private void OnEnable()
        {
            RunSaveService.OnSaveChanged += RefreshLoadGameButton;
        }

        private void OnDisable()
        {
            RunSaveService.OnSaveChanged -= RefreshLoadGameButton;
            if (settingsPanel != null) settingsPanel.OnCloseRequested -= CloseSettings;
            if (chronicle != null) chronicle.OnCloseRequested -= CloseChronicle;
            if (startRunButton != null) startRunButton.onClick.RemoveListener(StartRun);

            if (MetaProgressionManager.Instance != null)
            {
                MetaProgressionManager.Instance.OnClassUnlocked -= HandleClassUnlocked;
                MetaProgressionManager.Instance.OnNodeTierChanged -= HandleNodeTierChanged;
            }

            foreach (var card in cards)
            {
                if (card == null) continue;
                card.OnHoverEnter -= ShowClassInfo;
                card.OnHoverExit -= HideClassInfo;
            }
        }

        // --- Class select ---------------------------------------------------------------------------------

        private CharacterClassData SelectedClass =>
            classesByPlayerIndex != null && selectedIndex >= 0 && selectedIndex < classesByPlayerIndex.Length ? classesByPlayerIndex[selectedIndex] : null;

        /// <summary>The class the details show: the one hovered (or gamepad-selected), else the chosen one.</summary>
        public static CharacterClassData ClassToShow(CharacterClassData hovered, CharacterClassData chosen) => hovered != null ? hovered : chosen;

        private void ShowClassInfo(CharacterClassData classData) => ShowClassInfo(classData, false);

        private void ShowClassInfo(CharacterClassData classData, bool force)
        {
            var shown = ClassToShow(classData, SelectedClass);
            if (shown == null) return;
            // Hover and selection events repeat the same class; rebuilding would recreate the tab's rows under the pointer
            if (shown == shownClass && !force) return;
            shownClass = shown;

            var settings = UISettings.Instance;
            bool playable = IsClassPlayable(shown.characterClass);
            classNameText.text = shown.characterName;
            classDescriptionText.text = DamageTypeColorHelper.ColorizeMetaTags(shown.classDescription);

            if (detailsTabs != null) detailsTabs.Show(BuildDetails(shown));

            if (classFooterText != null)
            {
                classFooterText.text = playable ? unlockedText : string.Format(classLockedInfo, GetUnlockCost(shown.characterClass));
                if (settings != null) classFooterText.color = playable ? settings.PositiveColor : settings.NegativeColor;
            }
        }

        /// <summary>
        /// What the tabs show for a class: its starting kit at the tiers bought in its skill tree (ClassKits, the
        /// same items a new run starts with), its lore and playstyle texts, and its stat guide.
        /// </summary>
        public static ClassDetailsView BuildDetails(CharacterClassData classData)
        {
            var view = new ClassDetailsView();
            if (classData == null) return view;

            foreach (var item in ClassKits.Resolve(classData.characterClass))
            {
                if (item == null) continue;
                view.kit.Add(new ClassKitEntry
                {
                    icon = item.EffectIcon,
                    frameColor = EffectQualityDisplayHelper.GetQualityColor(item.Quality),
                    name = EffectQualityDisplayHelper.ColorizeName(item),
                    description = item.GetDescriptionText(),
                    statLines = item.GetStatLines(),
                });
            }

            view.lore = classData.lore;
            view.playstyle = classData.playstyle;
            if (classData.statPriorities != null)
            {
                foreach (var stat in ClassStatGuide.SortForDisplay(ListedStats(classData), classData.statPriorities))
                {
                    var priority = ClassStatGuide.Find(classData.statPriorities, stat);
                    view.stats.Add(new ClassStatEntry
                    {
                        statName = StatDisplayNameHelper.GetDisplayName(stat),
                        role = priority.role,
                        justification = priority.justification,
                    });
                }
            }
            return view;
        }

        private static List<StatType> ListedStats(CharacterClassData classData)
        {
            var stats = new List<StatType>();
            foreach (var priority in classData.statPriorities)
            {
                if (priority != null && priority.role != StatRole.None && !stats.Contains(priority.stat)) stats.Add(priority.stat);
            }
            return stats;
        }

        // Moving off a card shows the chosen class again (like the item bag's hover preview)
        private void HideClassInfo() => ShowClassInfo(null);

        // Clicking a card (A on it with the gamepad) chooses its class; hovering or moving onto it only previews it,
        // so walking the gamepad past the other cards to Start Run doesn't change the class. Start Run starts the run.
        public void SelectCrusader() => ChooseFromCard(0);

        public void SelectSentinel() => ChooseFromCard(1);

        public void SelectAugur() => ChooseFromCard(2);

        // With the gamepad, choosing a class moves the selection straight to Start Run (a locked class's
        // Start Run shows the unlock dialog); the mouse stays where it is.
        private void ChooseFromCard(int playerIndex)
        {
            SelectClass(playerIndex);
            bool gamepad = InputDeviceManager.Instance != null && InputDeviceManager.Instance.IsGamepadActive;
            var next = SelectionAfterChoosing(gamepad, startRunButton != null ? startRunButton.gameObject : null);
            if (next != null && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(next);
        }

        /// <summary>Where the gamepad selection goes after a card is pressed: Start Run; nothing moves with the mouse.</summary>
        public static GameObject SelectionAfterChoosing(bool gamepadActive, GameObject startRun) =>
            gamepadActive && startRun != null && startRun.activeInHierarchy ? startRun : null;

        private void SelectClass(int playerIndex)
        {
            selectedIndex = playerIndex;
            var chosen = SelectedClass;
            foreach (var card in cards)
            {
                card.SetSelected(chosen != null && card.ClassData == chosen);
            }
            if (detailsTabs != null) detailsTabs.SetChosenClass(chosen);
            ShowClassInfo(null);
        }

        public void StartRun()
        {
            var chosen = SelectedClass;
            if (Transitioning || chosen == null) return;

            if (!IsClassPlayable(chosen.characterClass))
            {
                if (confirmDialog != null)
                {
                    confirmDialog.ShowInfo(string.Format(classLockedMessage, chosen.characterName, GetUnlockCost(chosen.characterClass)));
                }
                return;
            }

            GameSession.Instance.SelectedPlayerIndex = selectedIndex;
            StartNewGame();
        }

        /// <summary>Opens the skill tree on the chosen class (e.g. to unlock it).</summary>
        public void OpenSkillTreeForSelected()
        {
            var chosen = SelectedClass;
            if (skillTree == null || Transitioning) return;
            if (chosen != null) skillTree.Open(chosen.characterClass);
            else skillTree.Open();
        }

        private int FirstPlayableIndex()
        {
            if (classesByPlayerIndex == null) return 0;
            for (int i = 0; i < classesByPlayerIndex.Length; i++)
            {
                if (classesByPlayerIndex[i] != null && IsClassPlayable(classesByPlayerIndex[i].characterClass)) return i;
            }
            return 0;
        }

        // Without the meta manager (e.g. the menu scene played on its own) every class is playable.
        private static bool IsClassPlayable(CharacterClass characterClass) =>
            MetaProgressionManager.Instance == null || MetaProgressionManager.Instance.IsClassUnlocked(characterClass);

        private static int GetUnlockCost(CharacterClass characterClass) =>
            SkillTrees.Get(characterClass)?.UnlockCost ?? 0;

        private void HandleClassUnlocked(CharacterClass characterClass)
        {
            RefreshClassLocks();
            ShowClassInfo(null, true);
        }

        // A tier bought in the skill tree (opened from here) changes the Abilities tab's items
        private void HandleNodeTierChanged(CharacterClass characterClass, string nodeId) => ShowClassInfo(null, true);

        private void RefreshClassLocks()
        {
            foreach (var card in cards)
            {
                if (card.ClassData != null)
                {
                    card.SetLocked(!IsClassPlayable(card.ClassData.characterClass));
                }
            }
        }

        // --- New game / Load game -------------------------------------------------------------------------

        public void StartNewGame()
        {
            RunSaveService.DeleteSave();
            MainSceneController.Instance.LoadLevelSelectorScene(true, openingCutsceneId);
        }

        public void LoadGame()
        {
            if (Transitioning) return;
            if (!MainSceneController.Instance.ContinueRun())
            {
                // The save couldn't be used and was deleted
                RefreshLoadGameButton();
            }
        }

        private void RefreshLoadGameButton()
        {
            bool hasSave = RunSaveService.TryLoad(out var save);
            if (loadGameButton != null)
            {
                loadGameButton.interactable = hasSave;

                if (loadGameButton.targetGraphic != null)
                {
                    loadGameButton.targetGraphic.color = hasSave ? Color.white : loadGameDisabledTint;
                }
            }

            if (saveInfoText != null)
            {
                var savedClass = hasSave && classesByPlayerIndex != null && save.selectedPlayerIndex >= 0 && save.selectedPlayerIndex < classesByPlayerIndex.Length
                    ? classesByPlayerIndex[save.selectedPlayerIndex]
                    : null;
                saveInfoText.text = hasSave
                    ? FormatSaveInfo(saveInfoFormat, savedClass != null ? savedClass.characterName : "?", save.map != null ? save.map.actNumber : 1, save.experience.CurrentLevel)
                    : noSaveText;
            }
        }

        /// <summary>The saved run under Load Game: "Crusader - Act I - Level 7".</summary>
        public static string FormatSaveInfo(string format, string className, int act, int level)
        {
            return string.Format(format, className, OverworldPlayerStatePresenter.ToRoman(Mathf.Max(1, act)), Mathf.Max(1, level));
        }

        // --- Panel switching ------------------------------------------------------------------------------

        public void SwitchFromMainPanelToCharacterSelect()
        {
            if (Transitioning) return;

            // The save is only deleted once a run starts, so backing out of class select keeps it
            if (RunSaveService.HasSave && confirmDialog != null)
            {
                confirmDialog.Show(newGameOverwriteWarning, OpenCharacterSelect);
                return;
            }

            OpenCharacterSelect();
        }

        private void OpenCharacterSelect()
        {
            SelectClass(FirstPlayableIndex());
            Switch(mainPanelWindow, characterSelectWindow);
        }

        public void SwitchFromCharacterSelectToMainPanel() => Switch(characterSelectWindow, mainPanelWindow);

        public void OpenSettings() => Switch(mainPanelWindow, settingsWindow);

        public void CloseSettings() => Switch(settingsWindow, mainPanelWindow);

        public void OpenChronicle()
        {
            if (Transitioning || chronicle == null) return;
            chronicle.Show(BuildChronicleEntries(), PlayFromChronicle);
            Switch(mainPanelWindow, chronicleWindow);
        }

        public void CloseChronicle() => Switch(chronicleWindow, mainPanelWindow);

        private static List<ChronicleEntry> BuildChronicleEntries()
        {
            var entries = new List<ChronicleEntry>();
            if (MetaProgressionManager.Instance == null || CutsceneDatabaseProvider.Instance == null) return entries;

            foreach (var cutscene in CutsceneRules.SelectSeen(CutsceneDatabaseProvider.Instance.Cutscenes, MetaProgressionManager.Instance.SeenCutscenes))
            {
                entries.Add(new ChronicleEntry(cutscene.CutsceneId, cutscene.Title));
            }
            return entries;
        }

        // The scene plays over the panel, and focus returns to it when the window closes
        private void PlayFromChronicle(string cutsceneId)
        {
            if (Transitioning || CutscenePlayer.Instance == null) return;
            CutscenePlayer.Instance.Play(cutsceneId, null);
        }

        private void Switch(UIWindow from, UIWindow to)
        {
            if (Transitioning) return;
            transition = StartCoroutine(TransitionRoutine(from, to));
        }

        // The old panel fades out sliding right, then the new one fades in sliding into place. Unscaled time, so a
        // paused time scale can't freeze the menu.
        private IEnumerator TransitionRoutine(UIWindow from, UIWindow to)
        {
            if (from != null)
            {
                from.SetOpen(false);
                yield return Animate(from, 1f, 0f, 0f, slideDistance, hideDuration);
                SetShown(from, false);
            }
            if (to != null)
            {
                to.SetOpen(true);
                yield return Animate(to, 0f, 1f, slideDistance, 0f, showDuration);
                SetShown(to, true);
            }
            transition = null;
        }

        private IEnumerator Animate(UIWindow window, float fromAlpha, float toAlpha, float fromOffset, float toOffset, float duration)
        {
            var rect = (RectTransform)window.transform;
            var group = window.GetComponent<CanvasGroup>();
            Vector2 shown = shownPositions.TryGetValue(window, out var position) ? position : rect.anchoredPosition;
            if (group != null) group.blocksRaycasts = false;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = UIFlight.EaseOutCubic(Mathf.Clamp01(elapsed / duration));
                if (group != null) group.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
                rect.anchoredPosition = shown + Vector2.right * Mathf.Lerp(fromOffset, toOffset, t);
                yield return null;
            }
        }

        // Shown: in place, opaque, clickable, open for the gamepad. Hidden: transparent, not clickable, closed.
        private void SetShown(UIWindow window, bool shown)
        {
            if (window == null) return;
            var rect = (RectTransform)window.transform;
            var group = window.GetComponent<CanvasGroup>();
            Vector2 position = shownPositions.TryGetValue(window, out var p) ? p : rect.anchoredPosition;
            rect.anchoredPosition = shown ? position : position + Vector2.right * slideDistance;
            if (group != null)
            {
                group.alpha = shown ? 1f : 0f;
                group.blocksRaycasts = shown;
            }
            window.SetOpen(shown);
        }

        public void ExitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

}
