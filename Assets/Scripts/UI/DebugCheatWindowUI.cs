using Game.Effects;
using Game.GameSettings;
using Game.Scene;
using Game.Waves;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.UI
{
    public class DebugCheatWindowUI : MonoBehaviour
    {
        [SerializeField] private GameObject windowPanel;
        [SerializeField] private TMP_Dropdown effectDropdown;
        [SerializeField] private Button skipWithProgressionButton;
        [SerializeField] private Button skipWithoutProgressionButton;
        [SerializeField] private Button addEffectButton;
        [SerializeField] private Button closeButton;

        private List<Effect> allEffects = new();

        private void Awake()
        {
            skipWithProgressionButton.onClick.AddListener(OnSkipWithProgressionClicked);
            skipWithoutProgressionButton.onClick.AddListener(OnSkipWithoutProgressionClicked);
            addEffectButton.onClick.AddListener(OnAddEffectClicked);
            closeButton.onClick.AddListener(CloseWindow);
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

            if (FindFirstObjectByType<LevelSettings>() == null)
            {
                return;
            }

            PopulateEffectDropdown();
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
            effectStore?.AddEffect(selected);
        }
    }
}
