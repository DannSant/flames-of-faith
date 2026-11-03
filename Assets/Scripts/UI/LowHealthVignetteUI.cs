using Game.Audio;
using Game.Combat;
using Game.Feel;
using Game.GameSettings;
using Game.Scene;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// The low-health warning: a full-screen red vignette that pulses while health is low in a fight.
    /// Health comes from PlayerHealth.onHealthChanged (subscribed once, like HealthBar); whether this
    /// level is a fight comes from MainSceneController.OnGameplayUISetupRequested. The rules are in
    /// <see cref="LowHealthRules"/>, the tunables on <see cref="GameFeelSettings"/>.
    /// Placed in UIMain by HudRestyler, under the HUD and every window.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class LowHealthVignetteUI : MonoBehaviour
    {
        private Image image;
        private PlayerHealth playerHealth;

        private bool inFight;
        private bool active;
        private float intensity;
        // 0..1, eased toward 1 while active and 0 otherwise, so the warning fades instead of popping
        private float visibility;
        // 0..1 through the current beat; the top of a beat is at 0
        private float phase;

        /// <summary>Raised at the top of each pulse with its intensity (the heartbeat hooks in here).</summary>
        public event System.Action<float> OnPulse;

        public bool IsActive => active;

        private void Awake()
        {
            image = GetComponent<Image>();
            image.raycastTarget = false;
            ApplyAlpha(0f);
        }

        private void OnEnable()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayUISetupRequested += HandleLevelUISetup;
            }
        }

        private void Start()
        {
            if (PlayerManager.Instance == null) return;
            playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onHealthChanged += HandleHealthChanged;
                HandleHealthChanged(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
            }
        }

        private void OnDisable()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayUISetupRequested -= HandleLevelUISetup;
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.onHealthChanged -= HandleHealthChanged;
            }
        }

        private void HandleLevelUISetup(LevelData level)
        {
            inFight = level != null && LowHealthRules.ShowsInLevel(level.type);
        }

        private void HandleHealthChanged(float current, float max)
        {
            var settings = GameFeelSettings.Instance;
            float fraction = LowHealthRules.HealthFraction(current, max);
            bool wasActive = active;
            active = LowHealthRules.IsActive(active, fraction, settings.LowHealthEnter, settings.LowHealthExit);
            if (active)
            {
                intensity = LowHealthRules.Intensity(fraction, settings.LowHealthEnter, settings.FullIntensityAt, settings.MinIntensity);
                // Start a new warning at the top of a pulse, so it reads at once
                if (!wasActive && visibility <= 0f) phase = 0f;
            }
        }

        private void Update()
        {
            var settings = GameFeelSettings.Instance;
            bool show = active && inFight && settings.LowHealthWarningEnabled
                && (playerHealth == null || !playerHealth.IsDead());

            // Unscaled time: it keeps pulsing behind a pause or level-up window
            float dt = Time.unscaledDeltaTime;
            visibility = Mathf.MoveTowards(visibility, show ? 1f : 0f, settings.VignetteFadeSpeed * dt);

            if (visibility <= 0f)
            {
                ApplyAlpha(0f);
                return;
            }

            phase += LowHealthRules.PulseRate(intensity, settings.PulseRateMin, settings.PulseRateMax) * dt;
            if (phase >= 1f)
            {
                phase -= Mathf.Floor(phase);
                if (show)
                {
                    OnPulse?.Invoke(intensity);
                    bool paused = PauseManager.Instance != null && PauseManager.Instance.IsPaused;
                    if (LowHealthRules.PlaysHeartbeat(intensity, settings.HeartbeatFrom, paused))
                    {
                        AudioManager.TryPlay(GameSound.LowHealthHeartbeat);
                    }
                }
            }

            ApplyAlpha(LowHealthRules.PulseAlpha(phase, intensity, settings.VignetteMaxAlpha, settings.PulseFloor, visibility));
        }

        private void ApplyAlpha(float alpha)
        {
            var tint = GameFeelSettings.Instance.VignetteTint;
            tint.a = alpha;
            image.color = tint;
            // Nothing to draw: skip the full-screen overdraw
            image.enabled = alpha > 0.001f;
        }
    }
}
