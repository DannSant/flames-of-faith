using Game.GameSettings;
using Game.UI.Style;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// HUD label for the auto-attack setting (toggled with Ctrl / RT or from the settings menu).
    /// Always shows the current state, and briefly brings in a more visible popup when it changes
    /// so the player notices a toggle mid-fight. Fed only by SettingsManager.OnAutoAttackChanged.
    /// OFF shows in the Negative (red) color: testers missed that the character had stopped attacking.
    /// </summary>
    public class AutoAttackIndicatorUI : MonoBehaviour
    {
        [Header("Persistent Label")]
        [SerializeField] private TextMeshProUGUI stateLabel;
        [SerializeField] private string onText = "Auto-attack: ON";
        [SerializeField] private string offText = "Auto-attack: OFF";

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
                    popupLabel.color = GetStateColor(enabled, UISettings.Instance);
                }
                popupTimer = popupHoldDuration + popupFadeDuration;
                popup.alpha = 1f;
            }
        }

        private void Refresh(bool enabled)
        {
            if (stateLabel == null) return;
            stateLabel.text = enabled ? onText : offText;
            stateLabel.color = GetStateColor(enabled, UISettings.Instance);
        }

        /// <summary>ON reads as plain text, OFF as a warning (red).</summary>
        public static Color GetStateColor(bool enabled, UISettings settings)
        {
            return enabled ? settings.TextColor : settings.NegativeColor;
        }
    }
}
