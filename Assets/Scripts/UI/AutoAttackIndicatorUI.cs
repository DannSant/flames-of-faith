using Game.GameSettings;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// HUD label for the auto-attack setting (toggled with Ctrl / RT or from the settings menu).
    /// Always shows the current state, and briefly brings in a more visible popup when it changes
    /// so the player notices a toggle mid-fight. Fed only by SettingsManager.OnAutoAttackChanged.
    /// </summary>
    public class AutoAttackIndicatorUI : MonoBehaviour
    {
        [Header("Persistent Label")]
        [SerializeField] private TextMeshProUGUI stateLabel;
        [SerializeField] private string onText = "Auto-attack: ON";
        [SerializeField] private string offText = "Auto-attack: OFF";
        [SerializeField] private Color onColor = Color.white;
        [SerializeField] private Color offColor = new Color(1f, 0.6f, 0.3f);

        [Header("Change Popup (optional)")]
        [Tooltip("Faded in when the setting changes, then out. Must be a child object, not this one.")]
        [SerializeField] private CanvasGroup popup;
        [SerializeField] private TextMeshProUGUI popupLabel;
        [SerializeField] private float popupHoldDuration = 1f;
        [SerializeField] private float popupFadeDuration = 0.4f;

        private float popupTimer;

        private void Start()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnAutoAttackChanged += HandleAutoAttackChanged;
            }

            if (popup != null)
            {
                popup.alpha = 0f;
            }

            Refresh(SettingsManager.Instance == null || SettingsManager.Instance.AutoAttackEnabled);
        }

        private void OnDestroy()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnAutoAttackChanged -= HandleAutoAttackChanged;
            }
        }

        private void Update()
        {
            if (popup == null || popupTimer <= 0f) return;

            // Unscaled: the toggle also works from places where time may be slowed or stopped.
            popupTimer -= Time.unscaledDeltaTime;
            popup.alpha = popupFadeDuration > 0f ? Mathf.Clamp01(popupTimer / popupFadeDuration) : 0f;
        }

        private void HandleAutoAttackChanged(bool enabled)
        {
            Refresh(enabled);

            if (popup != null)
            {
                if (popupLabel != null)
                {
                    popupLabel.text = enabled ? onText : offText;
                    popupLabel.color = enabled ? onColor : offColor;
                }
                popupTimer = popupHoldDuration + popupFadeDuration;
                popup.alpha = 1f;
            }
        }

        private void Refresh(bool enabled)
        {
            if (stateLabel == null) return;
            stateLabel.text = enabled ? onText : offText;
            stateLabel.color = enabled ? onColor : offColor;
        }
    }
}
