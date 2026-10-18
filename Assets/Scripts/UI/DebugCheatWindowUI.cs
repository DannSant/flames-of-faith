using Game.Effects;
using Game.GameSettings;
using Game.Metaprogression;
using Game.Overworld;
using Game.Scene;
using Game.Waves;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Developer cheat window (Ctrl+Shift+D, only with Developer Cheats on in the settings). It lives in
    /// the gameplay HUD (UIMain) and on the overworld map (LevelSelector). The level buttons (skip, add
    /// item) only work in a level; the map buttons (reveal, free travel, beat node, go to act) only work
    /// on the map. The map buttons are cloned from the Skip button at runtime, so the prefab needs no
    /// extra layout. Debug-only: it calls gameplay directly, unlike regular UI.
    /// </summary>
    public class DebugCheatWindowUI : MonoBehaviour
    {
        [SerializeField] private GameObject windowPanel;
        [SerializeField] private TMP_Dropdown effectDropdown;
        [SerializeField] private Button skipWithProgressionButton;
        [SerializeField] private Button skipWithoutProgressionButton;
        [SerializeField] private Button addEffectButton;
        [SerializeField] private Button closeButton;

        [Header("Map cheats (buttons cloned from the Skip button)")]
        [Tooltip("Anchored Y of the first map button, below the Add Effect button.")]
        [SerializeField] private float mapButtonsTopY = 110f;
        [SerializeField] private float mapButtonSpacing = 44f;

        private List<Effect> allEffects = new();

        private Button revealMapButton;
        private Button freeTravelButton;
        private Button beatNodeButton;
        private readonly List<Button> actButtons = new();

        private void Awake()
        {
            skipWithProgressionButton.onClick.AddListener(OnSkipWithProgressionClicked);
            skipWithoutProgressionButton.onClick.AddListener(OnSkipWithoutProgressionClicked);
            addEffectButton.onClick.AddListener(OnAddEffectClicked);
            closeButton.onClick.AddListener(CloseWindow);

            revealMapButton = CloneButton("Reveal full map", OnRevealMapClicked, 0);
            freeTravelButton = CloneButton("Free travel: OFF", OnFreeTravelClicked, 1);
            beatNodeButton = CloneButton("Beat current node", OnBeatNodeClicked, 2);

            windowPanel.SetActive(false);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            bool ctrlShift = (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed)
                && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);

            if (ctrlShift && keyboard.dKey.wasPressedThisFrame)
            {
                ToggleWindow();
            }
        }

        private static bool InLevel => FindFirstObjectByType<LevelSettings>() != null;

        private static bool OnMap =>
            MapRunController.Instance != null && MapRunController.Instance.IsInitialized
            && PlayerManager.Instance != null && PlayerManager.Instance.IsPlayerOnMap;

        private void ToggleWindow()
        {
            if (windowPanel.activeSelf)
            {
                CloseWindow();
                return;
            }

            if (SettingsManager.Instance == null || !SettingsManager.Instance.DeveloperCheatsEnabled)
            {
                return;
            }

            bool inLevel = InLevel;
            bool onMap = !inLevel && OnMap;
            if (!inLevel && !onMap)
            {
                return;
            }

            PopulateEffectDropdown();
            RebuildActButtons(onMap);

            // Level-only cheats. An item added on the map would be lost: entering a level reloads the
            // player from the saved snapshot.
            skipWithProgressionButton.interactable = inLevel;
            skipWithoutProgressionButton.interactable = inLevel;
            addEffectButton.interactable = inLevel;
            effectDropdown.interactable = inLevel;

            revealMapButton.interactable = onMap;
            freeTravelButton.interactable = onMap;
            beatNodeButton.interactable = onMap;
            RefreshFreeTravelLabel();

            windowPanel.SetActive(true);
        }

        private void CloseWindow()
        {
            windowPanel.SetActive(false);
        }

        private void PopulateEffectDropdown()
        {
            allEffects = Resources.LoadAll<Effect>("Effects")
                .OrderBy(effect => effect.effectName)
                .ToList();

            effectDropdown.ClearOptions();
            effectDropdown.AddOptions(allEffects.Select(effect => effect.effectName).ToList());
        }

        private void OnSkipWithProgressionClicked()
        {
            PauseManager.Instance?.SetPause(false);
            WaveSpawner.Instance?.GoToNextLevel();
            CloseWindow();
        }

        private void OnSkipWithoutProgressionClicked()
        {
            GameSession.Instance.SetSuppressNextProgressionIncrement(true);
            PauseManager.Instance?.SetPause(false);
            WaveSpawner.Instance?.GoToNextLevel();
            CloseWindow();
        }

        private void OnAddEffectClicked()
        {
            if (effectDropdown.value < 0 || effectDropdown.value >= allEffects.Count) return;

            var selected = allEffects[effectDropdown.value];
            var effectStore = PlayerManager.Instance?.GetPlayerComponent<EffectStore>();
            if (effectStore == null) return;

            // A class kit item (e.g. "Glyph of Whirlwind +2") replaces the owned tier of its line.
            if (!SkillTrees.EquipTier(effectStore, selected))
            {
                effectStore.AddEffect(selected);
            }
        }

        // --- Map cheats ---

        private void OnRevealMapClicked()
        {
            MapRunController.Instance?.DebugRevealAll();
        }

        private void OnFreeTravelClicked()
        {
            var map = MapRunController.Instance;
            if (map == null) return;
            map.FreeTravel = !map.FreeTravel;
            RefreshFreeTravelLabel();
        }

        private void OnBeatNodeClicked()
        {
            MapRunController.Instance?.DebugBeatCurrentNode();
        }

        private void OnGoToActClicked(int act)
        {
            MapRunController.Instance?.DebugGoToAct(act);
        }

        private void RefreshFreeTravelLabel()
        {
            bool on = MapRunController.Instance != null && MapRunController.Instance.FreeTravel;
            SetLabel(freeTravelButton, on ? "Free travel: ON" : "Free travel: OFF");
        }

        // One "Go to Act N" button per act of the run, under the other map buttons.
        private void RebuildActButtons(bool onMap)
        {
            foreach (var button in actButtons)
            {
                if (button != null) Destroy(button.gameObject);
            }
            actButtons.Clear();

            if (!onMap) return;

            var acts = MapRunController.Instance.GetActNumbers();
            for (int i = 0; i < acts.Count; i++)
            {
                int act = acts[i];
                actButtons.Add(CloneButton($"Go to Act {act}", () => OnGoToActClicked(act), 3 + i));
            }
        }

        private Button CloneButton(string label, UnityAction onClick, int row)
        {
            var button = Instantiate(skipWithProgressionButton, skipWithProgressionButton.transform.parent);
            button.name = label;
            // A fresh event: RemoveAllListeners keeps listeners wired in the Inspector.
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(onClick);
            button.interactable = true;

            var rect = (RectTransform)button.transform;
            var source = (RectTransform)skipWithProgressionButton.transform;
            rect.anchoredPosition = new Vector2(source.anchoredPosition.x, mapButtonsTopY - row * mapButtonSpacing);

            SetLabel(button, label);
            return button;
        }

        private static void SetLabel(Button button, string text)
        {
            var label = button != null ? button.GetComponentInChildren<TMP_Text>(true) : null;
            if (label != null) label.text = text;
        }
    }
}
