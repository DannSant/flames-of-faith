using Game.Combat;
using UnityEngine;
using UnityEngine.UI;
using Game.Control;
namespace Game.UI
{
    public class IconCooldownDisplay : MonoBehaviour
    {
        [SerializeField] private Image cooldownAttackBackground;
        [SerializeField] private Image cooldownSpecialAttackBackground;
        [SerializeField] private Image cooldownDashBackground;

        private void Start()
        {
            if(WeaponManager.Instance != null)
            {
                WeaponManager.Instance.OnAttackTimerUpdated += UpdateCooldownAttackDisplay;
                WeaponManager.Instance.OnSpecialAttackTimerUpdated += UpdateCooldownSpecialAttackDisplay;
            }
            if(Dash.Instance != null)
            {
                Dash.Instance.OnDashTimerUpdated += UpdateCooldownDashDisplay;
            }
        }

        private void OnDisable()
        {
            if (WeaponManager.Instance != null)
            {
                WeaponManager.Instance.OnAttackTimerUpdated -= UpdateCooldownAttackDisplay;
                WeaponManager.Instance.OnSpecialAttackTimerUpdated -= UpdateCooldownSpecialAttackDisplay;
            }

            if (Dash.Instance != null)
            {
                Dash.Instance.OnDashTimerUpdated -= UpdateCooldownDashDisplay;
            }
        }

        private void UpdateCooldownAttackDisplay(float timeLeft, float cooldownDuration)
        {
            float fill = Mathf.Clamp01(timeLeft / cooldownDuration);
            cooldownAttackBackground.fillAmount = fill;
        }

        private void UpdateCooldownSpecialAttackDisplay(float timeLeft, float cooldownDuration)
        {
            float fill = Mathf.Clamp01(timeLeft / cooldownDuration);
            cooldownSpecialAttackBackground.fillAmount = fill;
        }

        private void UpdateCooldownDashDisplay(float timeLeft, float cooldownDuration)
        {
            float fill = Mathf.Clamp01(timeLeft / cooldownDuration);
            cooldownDashBackground.fillAmount = fill;
        }
    }
}
