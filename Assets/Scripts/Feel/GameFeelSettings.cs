using UnityEngine;

namespace Game.Feel
{
    /// <summary>
    /// Look-and-feel tunables: the low-health warning (and, later, sound and hit feel).
    /// Not a balance table: these stay in Unity. Loaded from Resources/Feel/GameFeelSettings.
    /// </summary>
    [CreateAssetMenu(fileName = "GameFeelSettings", menuName = "Feel/Game Feel Settings")]
    public class GameFeelSettings : ScriptableObject
    {
        private const string ResourcePath = "Feel/GameFeelSettings";

        [Header("Low-health warning")]
        [Tooltip("Master switch for the pulsing red vignette at low health.")]
        [SerializeField] private bool lowHealthWarningEnabled = true;
        [Tooltip("The warning turns on at or below this share of max health (0.30 = 30%).")]
        [Range(0f, 1f)]
        [SerializeField] private float lowHealthEnter = 0.30f;
        [Tooltip("Once on, the warning turns off only above this share of max health, so a small heal doesn't make it flicker. Must be at least Low Health Enter.")]
        [Range(0f, 1f)]
        [SerializeField] private float lowHealthExit = 0.35f;
        [Tooltip("At or below this share of max health the warning is at full intensity.")]
        [Range(0f, 1f)]
        [SerializeField] private float fullIntensityAt = 0.10f;
        [Tooltip("Intensity at the Low Health Enter threshold (and between it and the exit threshold).")]
        [Range(0f, 1f)]
        [SerializeField] private float minIntensity = 0.35f;
        [Tooltip("Pulses per second at the lowest intensity.")]
        [Min(0f)]
        [SerializeField] private float pulseRateMin = 0.8f;
        [Tooltip("Pulses per second at full intensity. Keep it under 2, or it reads as flashing.")]
        [Min(0f)]
        [SerializeField] private float pulseRateMax = 1.6f;
        [Tooltip("Share of the alpha kept at the bottom of a pulse (0 = fades out completely between beats).")]
        [Range(0f, 1f)]
        [SerializeField] private float pulseFloor = 0.45f;
        [Tooltip("Tint of the vignette image (its alpha is ignored: Max Alpha sets it).")]
        [SerializeField] private Color vignetteTint = new Color(0.75f, 0.05f, 0.05f, 1f);
        [Tooltip("Alpha of the vignette at full intensity, at the top of a pulse.")]
        [Range(0f, 1f)]
        [SerializeField] private float vignetteMaxAlpha = 0.85f;
        [Tooltip("How fast the warning fades in and out, in full fades per second (real time).")]
        [Min(0.01f)]
        [SerializeField] private float vignetteFadeSpeed = 2.5f;
        [Tooltip("The heartbeat sound plays on each pulse above this intensity (sound comes with the sound chunk).")]
        [Range(0f, 1f)]
        [SerializeField] private float heartbeatFrom = 0.6f;

        [Header("Sound: XP pickup chain")]
        [Tooltip("XP pickups closer together than this (seconds, real time) chain, each one a little higher.")]
        [Min(0f)]
        [SerializeField] private float xpChainWindow = 0.6f;
        [Tooltip("Pitch added per chained XP pickup.")]
        [Min(0f)]
        [SerializeField] private float xpChainStep = 0.03f;
        [Tooltip("Highest pitch of the chain; the pickup after it starts over at 1.")]
        [Min(1f)]
        [SerializeField] private float xpChainMaxPitch = 1.5f;

        public bool LowHealthWarningEnabled => lowHealthWarningEnabled;
        public float LowHealthEnter => lowHealthEnter;
        public float LowHealthExit => Mathf.Max(lowHealthEnter, lowHealthExit);
        public float FullIntensityAt => Mathf.Min(fullIntensityAt, lowHealthEnter);
        public float MinIntensity => minIntensity;
        public float PulseRateMin => pulseRateMin;
        public float PulseRateMax => pulseRateMax;
        public float PulseFloor => pulseFloor;
        public Color VignetteTint => vignetteTint;
        public float VignetteMaxAlpha => vignetteMaxAlpha;
        public float VignetteFadeSpeed => vignetteFadeSpeed;
        public float HeartbeatFrom => heartbeatFrom;
        public float XpChainWindow => xpChainWindow;
        public float XpChainStep => xpChainStep;
        public float XpChainMaxPitch => xpChainMaxPitch;

        private static GameFeelSettings instance;

        public static GameFeelSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<GameFeelSettings>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning($"GameFeelSettings not found at Resources/{ResourcePath}. Falling back to default values.");
                        instance = CreateInstance<GameFeelSettings>();
                    }
                }
                return instance;
            }
        }
    }
}
