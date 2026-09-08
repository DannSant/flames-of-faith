using Game.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Control;
using Game.Scene;
namespace Game.UI
{
    public class IconCooldownDisplay : MonoBehaviour
    {
        [SerializeField] private Image attackBackground;
        [SerializeField] private Image specialAttackBackground;
        [SerializeField] private Image dashBackground;
        [SerializeField] private Image cooldownAttackBackground;
        [SerializeField] private Image cooldownSpecialAttackBackground;
        [SerializeField] private Image cooldownDashBackground;
        [SerializeField] private TextMeshProUGUI dashChargesText;
        [SerializeField] private AbilityIconTooltip dashTooltip;

        private WeaponManager weaponManager;

        private void Start()
        {
            weaponManager = PlayerManager.Instance.GetPlayerComponent<WeaponManager>();
            if (weaponManager != null)
            {
                weaponManager.OnAttackTimerUpdated += UpdateCooldownAttackDisplay;
                weaponManager.OnSpecialAttackTimerUpdated += UpdateCooldownSpecialAttackDisplay;
            }
            var playerDash = PlayerManager.Instance.GetPlayerComponent<DashBase>();
            if (playerDash != null)
            {
                playerDash.OnDashTimerUpdated += UpdateCooldownDashDisplay;
                playerDash.OnChargesUpdated += UpdateDashChargesDisplay;
                UpdateDashChargesDisplay(playerDash.CurrentCharges, playerDash.MaxCharges);
            }
        }

        private void OnDisable()
        {
            if (weaponManager != null)
            {
                weaponManager.OnAttackTimerUpdated -= UpdateCooldownAttackDisplay;
                weaponManager.OnSpecialAttackTimerUpdated -= UpdateCooldownSpecialAttackDisplay;
            }
            var playerDash = PlayerManager.Instance.GetPlayerComponent<DashBase>();
            if (playerDash != null)
            {
                playerDash.OnDashTimerUpdated -= UpdateCooldownDashDisplay;
                playerDash.OnChargesUpdated -= UpdateDashChargesDisplay;
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

        private void UpdateDashChargesDisplay(int current, int max)
        {
            if (dashChargesText == null) return;

            bool showCharges = max > 1;
            dashChargesText.gameObject.SetActive(showCharges);
            if (showCharges)
            {
                dashChargesText.SetText($"{current}");
            }
        }

        public void SetIcons(Sprite attackIcon, Sprite specialAttackIcon, Sprite dashIcon)
        {
            if (attackBackground != null)
            {
                attackBackground.sprite = attackIcon;
            }
            if (specialAttackBackground != null)
            {
                specialAttackBackground.sprite = specialAttackIcon;
            }
            if (dashBackground != null)
            {
                dashBackground.sprite = dashIcon;
            }
        }

        public void SetDashTooltip(string title, string description)
        {
            dashTooltip?.SetTooltip(title, DamageTypeColorHelper.ColorizeMetaTags(description));
        }
    }
}
