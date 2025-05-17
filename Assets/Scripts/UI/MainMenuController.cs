using Game.Scene;
using UnityEngine;

namespace Game.UI
{
    public class MainMenuController : MonoBehaviour
    {
        public void StartGame() 
        { 
            MainSceneController.Instance.LoadGameplay();
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