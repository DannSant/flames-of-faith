using Game.Scene;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class MainMenuController : MonoBehaviour
    {       

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }
        public void StartGame() 
        { 
            //MainSceneController.Instance.LoadGameplay();
            MainSceneController.Instance.LoadLevelSelectorScene();
        }

        public void SelectWarrior() 
        {
            GameSession.Instance.SelectedPlayerIndex = 0;
            StartGame();
        }

        public void SelectArcher()
        {
            GameSession.Instance.SelectedPlayerIndex = 1;
            StartGame();
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