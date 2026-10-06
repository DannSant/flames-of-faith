using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// World space bar shown next to the player during the Corruptor phase: fills up as the time runs out and shows
    /// the Corruption the Corruptor will grant, pulsing every time it goes up. Lives on a World Space canvas prefab
    /// that CorruptorPhaseBarPresenter attaches to the player.
    /// </summary>
    public class CorruptorPhaseBarUI : MonoBehaviour
    {
        [Tooltip("Filled image that grows as the Corruptor timer runs out.")]
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI corruptionText;
        [Tooltip("Optional. Remaining seconds.")]
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("Pulse when Corruption goes up")]
        [Tooltip("Scaled by the pulse. Defaults to this transform.")]
        [SerializeField] private Transform pulseTarget;
        [SerializeField] private float pulseScale = 1.3f;
        [SerializeField] private float pulseDuration = 0.25f;

        private float duration;
        private Vector3 baseScale;
        private Coroutine pulseRoutine;

        private void Awake()
        {
            if (pulseTarget == null) pulseTarget = transform;
            baseScale = pulseTarget.localScale;
        }

        public void Show(float phaseDuration)
        {
            duration = phaseDuration;
            if (fillImage != null) fillImage.fillAmount = 0f;
            SetRemaining(phaseDuration);
        }

        public void SetRemaining(float remaining)
        {
            if (fillImage != null && duration > 0f)
            {
                fillImage.fillAmount = Mathf.Clamp01(1f - remaining / duration);
            }
            if (timeText != null)
            {
                timeText.text = $"{Mathf.CeilToInt(remaining)}s";
            }
        }

        public void SetCorruption(int corruption)
        {
            if (corruptionText != null)
            {
                corruptionText.text = $"+{corruption}";
            }

            if (isActiveAndEnabled)
            {
                if (pulseRoutine != null) StopCoroutine(pulseRoutine);
                pulseRoutine = StartCoroutine(Pulse());
            }
        }

        private IEnumerator Pulse()
        {
            float half = pulseDuration * 0.5f;
            float elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed < half ? elapsed / half : 1f - (elapsed - half) / half;
                pulseTarget.localScale = baseScale * Mathf.Lerp(1f, pulseScale, Mathf.Clamp01(t));
                yield return null;
            }
            pulseTarget.localScale = baseScale;
            pulseRoutine = null;
        }
    }
}
