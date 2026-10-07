using Game.GameSettings;
using UnityEngine;

namespace Game.UI.Navigation
{
    /// <summary>
    /// For HUD panels that are always on screen: registers their UIWindow with the focus system only while the game is
    /// paused, so gamepad players can reach them from the pause menu (LB/RB) without them taking part in gameplay.
    /// Set the UIWindow's Open Mode to Manual (and usually its Role to Secondary).
    /// </summary>
    [RequireComponent(typeof(UIWindow))]
    public class UIWindowOpenWhilePaused : MonoBehaviour
    {
        private UIWindow window;
        private bool subscribed;

        private void Awake()
        {
            window = GetComponent<UIWindow>();
        }

        private void OnEnable() => TrySubscribe();

        // PauseManager may not exist yet when this first enables, depending on scene load order
        private void Start() => TrySubscribe();

        private void OnDisable()
        {
            if (subscribed && PauseManager.Instance != null)
            {
                PauseManager.Instance.onPauseToggled -= HandlePauseToggled;
            }
            subscribed = false;
            window.SetOpen(false);
        }

        private void TrySubscribe()
        {
            if (subscribed || PauseManager.Instance == null) return;
            PauseManager.Instance.onPauseToggled += HandlePauseToggled;
            subscribed = true;
            window.SetOpen(PauseManager.Instance.IsPaused);
        }

        private void HandlePauseToggled(bool isPaused)
        {
            window.SetOpen(isPaused);
        }
    }
}
