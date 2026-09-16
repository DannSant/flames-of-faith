using Game.Combat;
using Game.Control;
using Game.Effects;
using Game.Scene;
using Game.UI.Navigation;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class MainMenuController : MonoBehaviour
    {

        [Header("Gamepad navigation")]
        [Tooltip("UIWindows on the three panels, set to Manual open mode - the panels stay active and " +
                 "animate off screen, so the focus system is told which one is showing.")]
        [SerializeField] private UIWindow mainPanelWindow;
        [SerializeField] private UIWindow characterSelectWindow;
        [SerializeField] private UIWindow settingsWindow;

        [Header("Class Info Display")]
        [SerializeField] private TextMeshProUGUI classNameText;
        [SerializeField] private TextMeshProUGUI classDescriptionText;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            foreach (var hoverButton in FindObjectsByType<ClassSelectHoverButton>(FindObjectsSortMode.None))
            {
                hoverButton.OnHoverEnter += ShowClassInfo;
                hoverButton.OnHoverExit += HideClassInfo;
            }

            // The menu opens on the main panel; the other two are off screen.
            if (mainPanelWindow != null) mainPanelWindow.SetOpen(true);
            if (characterSelectWindow != null) characterSelectWindow.SetOpen(false);
            if (settingsWindow != null) settingsWindow.SetOpen(false);
        }

        private void OnDisable()
        {
            foreach (var hoverButton in FindObjectsByType<ClassSelectHoverButton>(FindObjectsSortMode.None))
            {
                hoverButton.OnHoverEnter -= ShowClassInfo;
                hoverButton.OnHoverExit -= HideClassInfo;
            }
        }

        private void ShowClassInfo(CharacterClassData classData)
        {
            if (classData == null) return;

            classNameText.text = classData.characterName;
            classDescriptionText.text = DamageTypeColorHelper.ColorizeMetaTags(classData.classDescription);

            if (classData.startingItem != null)
            {
                itemNameText.text = classData.startingItem.EffectName;
                itemNameText.color = EffectQualityDisplayHelper.GetQualityColor(classData.startingItem.Quality);
                itemDescriptionText.text = classData.startingItem.GetFormattedDescription();
            }
        }

        private void HideClassInfo()
        {
            classNameText.text = "";
            classDescriptionText.text = "";
            itemNameText.text = "";
            itemDescriptionText.text = "";
        }
        public void StartNewGame() 
        { 
            //MainSceneController.Instance.LoadGameplay();
            MainSceneController.Instance.LoadLevelSelectorScene(true);
        }

        public void SelectWarrior() 
        {
            GameSession.Instance.SelectedPlayerIndex = 0;
            StartNewGame();
        }

        public void SelectArcher()
        {
            GameSession.Instance.SelectedPlayerIndex = 1;
            StartNewGame();
        }

        public void SelectAugur()
        {
            GameSession.Instance.SelectedPlayerIndex = 2;
            StartNewGame();
        }

        public void SwitchFromMainPanelToCharacterSelect()
        {
            StartCoroutine(SwitchFromMainPanelToCharacterSelectRoutine());
        }

        public IEnumerator SwitchFromMainPanelToCharacterSelectRoutine()
        {
            ToggleMainPanel(false);
            yield return new WaitForSeconds(1f);
            ToggleCharacterSelect(true);
        }

        public void SwitchFromCharacterSelectToMainPanel()
        {
            StartCoroutine(SwitchFromCharacterSelectToMainPanelRoutine());
        }

        public IEnumerator SwitchFromCharacterSelectToMainPanelRoutine()
        {
            ToggleCharacterSelect(false);          
            yield return new WaitForSeconds(1f);
            ToggleMainPanel(true);
        }

        private void ToggleMainPanel(bool show)
        {
            if (show)
            {
                animator.SetTrigger("MainPanelShow");
            }
            else
            {
                animator.SetTrigger("MainPanelHide");
            }

            if (mainPanelWindow != null) mainPanelWindow.SetOpen(show);
        }

        private void ToggleCharacterSelect(bool show)
        {
            if (show)
            {
                animator.SetTrigger("CharacterSelectShow");
            }
            else
            {
                animator.SetTrigger("CharacterSelectHide");
            }

            if (characterSelectWindow != null) characterSelectWindow.SetOpen(show);
        }

        public void OpenSettings()
        {
            StartCoroutine(OpenSettingsRoutine());
        }

        public IEnumerator OpenSettingsRoutine()
        {
            ToggleMainPanel(false);
            yield return new WaitForSeconds(1f);
            ToggleSettingsPanel(true);
        }

        public void CloseSettings()
        {
            StartCoroutine(CloseSettingsRoutine());
        }

        public IEnumerator CloseSettingsRoutine()
        {
            ToggleSettingsPanel(false);
            yield return new WaitForSeconds(1f);
            ToggleMainPanel(true);
        }

        private void ToggleSettingsPanel(bool show)
        {
            if (show)
            {
                animator.SetTrigger("SettingsShow");
            }
            else
            {
                animator.SetTrigger("SettingsHide");
            }

            if (settingsWindow != null) settingsWindow.SetOpen(show);
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