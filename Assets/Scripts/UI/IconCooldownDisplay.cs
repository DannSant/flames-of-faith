using Game.Combat;
using UnityEngine;
using UnityEngine.UI;
using Game.Control;
using Game.Scene;
namespace Game.UI
{
    public class IconCooldownDisplay : MonoBehaviour
    {
        [SerializeField] private Image cooldownAttackBackground;
        [SerializeField] private Image cooldownSpecialAttackBackground;
        [SerializeField] private Image cooldownDashBackground;

        private WeaponManager weaponManager;

        private void Start()
        {
            weaponManager = PlayerManager.Instance.GetPlayerComponent<WeaponManager>();
            if (weaponManager != null)
            {
                weaponManager.OnAttackTimerUpdated += UpdateCooldownAttackDisplay;
                weaponManager.OnSpecialAttackTimerUpdated += UpdateCooldownSpecialAttackDisplay;
            }
            /*if(Dash.Instance != null)
            {
                Dash.Instance.OnDashTimerUpdated += UpdateCooldownDashDisplay;
            }*/
            var playerDash = PlayerManager.Instance.GetPlayerComponent<Dash>();
            if (playerDash != null)
            {
                playerDash.OnDashTimerUpdated += UpdateCooldownDashDisplay;
                
            }
        }

        private void OnDisable()
        {
            if (weaponManager != null)
            {
                weaponManager.OnAttackTimerUpdated -= UpdateCooldownAttackDisplay;
                weaponManager.OnSpecialAttackTimerUpdated -= UpdateCooldownSpecialAttackDisplay;
            }
            var playerDash = PlayerManager.Instance.GetPlayerComponent<Dash>();
            if (playerDash != null)
            {
                playerDash.OnDashTimerUpdated -= UpdateCooldownDashDisplay;

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
