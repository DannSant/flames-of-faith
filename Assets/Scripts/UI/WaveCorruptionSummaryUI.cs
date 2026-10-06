using Game.Control;
using Game.Scene;
using Game.Waves;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Game.UI
{
    /// <summary>
    /// End of wave summary: lists where this wave's Corruption came from, then animates
    /// Grace + Grace Affinity - Corruption = new Grace. Waits for the player to press Submit (Enter / A) before
    /// closing; pressing it during the animation skips straight to the result. Tells the wave sequence when it's done.
    /// </summary>
    public class WaveCorruptionSummaryUI : MonoBehaviour
    {
        [Tooltip("Panel shown while the summary plays. Must be a child object, not this one.")]
        [SerializeField] private GameObject panel;
        [Tooltip("Optional. Used to fade the panel in and out.")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Rows (hidden when they don't apply)")]
        [SerializeField] private TextMeshProUGUI corruptorRowText;
        [SerializeField] private TextMeshProUGUI corruptedDamageRowText;
        [FormerlySerializedAs("gracePerWaveRowText")]
        [SerializeField] private TextMeshProUGUI graceAffinityRowText;
        [SerializeField] private TextMeshProUGUI totalCorruptionRowText;

        [Header("Result")]
        [Tooltip("Shows the subtraction, e.g. 'Grace 5 + 1 - 6 = 0'.")]
        [SerializeField] private TextMeshProUGUI formulaText;
        [SerializeField] private Color positiveColor = new Color(1f, 0.9f, 0.4f);
        [SerializeField] private Color negativeColor = new Color(0.75f, 0.45f, 1f);
        [SerializeField] private Color corruptionColor = new Color(0.75f, 0.45f, 1f);

        [Header("Continue")]
        [Tooltip("Wait for Submit (Enter / A) before closing. When off, the summary closes after Hold Duration.")]
        [SerializeField] private bool waitForConfirm = true;
        [Tooltip("Shown once the result is on screen, e.g. a Submit button icon + 'Continue'.")]
        [SerializeField] private GameObject continuePrompt;
        [Tooltip("Only used when Wait For Confirm is off.")]
        [SerializeField] private float holdDuration = 1.5f;

        [Header("Timing")]
        [SerializeField] private float fadeDuration = 0.2f;
        [SerializeField] private float rowRevealDelay = 0.45f;
        [SerializeField] private float countDuration = 0.8f;
        [Tooltip("Skip the summary when nothing changed this wave (no Corruption and no Grace Affinity).")]
        [SerializeField] private bool skipWhenNothingChanged = true;

        private Coroutine playRoutine;
        private Action pendingCallback;
        private PlayerInputHandler inputHandler;
        private bool submitPressed;

        private void Start()
        {
            if (panel != null && panel == gameObject)
            {
                Debug.LogError("WaveCorruptionSummaryUI: panel must be a child object, otherwise hiding it also disables this script.");
            }
            Hide();

            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveCorruptionResolved += Show;
            }

            if (PlayerManager.Instance != null)
            {
                inputHandler = PlayerManager.Instance.GetPlayerComponent<PlayerInputHandler>();
            }
        }

        private void OnDestroy()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveCorruptionResolved -= Show;
            }
            ListenForSubmit(false);
            CompletePending();
        }

        public void Show(WaveCorruptionResult result, Action onFinished)
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                ListenForSubmit(false);
                CompletePending();
            }

            pendingCallback = onFinished;

            bool nothingChanged = result.totalCorruption == 0 && Mathf.Approximately(result.graceAffinity, 0f);
            if (panel == null || (skipWhenNothingChanged && nothingChanged))
            {
                CompletePending();
                return;
            }

            playRoutine = StartCoroutine(Play(result));
        }

        private IEnumerator Play(WaveCorruptionResult result)
        {
            HideRow(corruptorRowText);
            HideRow(corruptedDamageRowText);
            HideRow(graceAffinityRowText);
            HideRow(totalCorruptionRowText);
            if (formulaText != null) formulaText.text = string.Empty;
            if (continuePrompt != null) continuePrompt.SetActive(false);

            submitPressed = false;
            ListenForSubmit(true);

            panel.SetActive(true);
            yield return Fade(0f, 1f);

            // 1. Where the Corruption came from
            if (result.corruptor.spawned)
            {
                string label = result.corruptor.killed
                    ? $"Corruptor killed in {result.corruptor.killTime:0.0}s"
                    : "Corruptor escaped";
                yield return RevealRow(corruptorRowText, label, result.corruptor.corruption, corruptionColor);
            }

            if (result.corruptedDamageTaken > 0f)
            {
                yield return RevealRow(corruptedDamageRowText,
                    $"Corrupted damage taken: {result.corruptedDamageTaken:0}", result.corruptionFromDamage, corruptionColor);
            }

            if (result.totalCorruption > 0)
            {
                yield return RevealRow(totalCorruptionRowText, "Corruption this wave", result.totalCorruption, corruptionColor);
            }

            // 2. What offsets it
            if (!Mathf.Approximately(result.graceAffinity, 0f))
            {
                yield return RevealRow(graceAffinityRowText, "Grace Affinity", Mathf.RoundToInt(result.graceAffinity), positiveColor);
            }

            // 3. The subtraction, counting from the old Grace to the new one (a press jumps to the result)
            if (formulaText != null)
            {
                string left = $"Grace {Signed(result.graceBefore)}";
                if (!Mathf.Approximately(result.graceAffinity, 0f)) left += $" + {result.graceAffinity:0}";
                if (result.totalCorruption > 0) left += $" - <color=#{ColorUtility.ToHtmlStringRGB(corruptionColor)}>{result.totalCorruption}</color>";

                float elapsed = 0f;
                while (elapsed < countDuration && !submitPressed)
                {
                    elapsed += Time.deltaTime;
                    float value = Mathf.Lerp(result.graceBefore, result.graceAfter, Mathf.Clamp01(elapsed / countDuration));
                    SetFormula(left, value);
                    yield return null;
                }
                SetFormula(left, result.graceAfter);
            }

            // 4. Wait for the player (a press used to skip the animation doesn't also close it)
            if (waitForConfirm)
            {
                submitPressed = false;
                if (continuePrompt != null) continuePrompt.SetActive(true);
                while (!submitPressed)
                {
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(holdDuration);
            }

            ListenForSubmit(false);
            if (continuePrompt != null) continuePrompt.SetActive(false);
            yield return Fade(1f, 0f);

            Hide();
            playRoutine = null;
            CompletePending();
        }

        private IEnumerator RevealRow(TextMeshProUGUI row, string label, int amount, Color amountColor)
        {
            if (row == null) yield break;

            row.text = $"{label}  <color=#{ColorUtility.ToHtmlStringRGB(amountColor)}>+{amount}</color>";
            row.gameObject.SetActive(true);

            float waited = 0f;
            while (waited < rowRevealDelay && !submitPressed)
            {
                waited += Time.deltaTime;
                yield return null;
            }
        }

        private void ListenForSubmit(bool listen)
        {
            if (inputHandler == null) return;

            inputHandler.UI.Submit.performed -= HandleSubmit;
            if (listen)
            {
                inputHandler.UI.Submit.performed += HandleSubmit;
            }
        }

        private void HandleSubmit(InputAction.CallbackContext _)
        {
            submitPressed = true;
        }

        private void SetFormula(string left, float value)
        {
            Color color = value < 0f ? negativeColor : positiveColor;
            formulaText.text = $"{left} = <color=#{ColorUtility.ToHtmlStringRGB(color)}>{Signed(Mathf.Round(value))}</color>";
        }

        private IEnumerator Fade(float from, float to)
        {
            if (canvasGroup == null || fadeDuration <= 0f)
            {
                if (canvasGroup != null) canvasGroup.alpha = to;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }
            canvasGroup.alpha = to;
        }

        private void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        private static void HideRow(TextMeshProUGUI row)
        {
            if (row != null) row.gameObject.SetActive(false);
        }

        private void CompletePending()
        {
            var callback = pendingCallback;
            pendingCallback = null;
            callback?.Invoke();
        }

        private static string Signed(float value)
        {
            return value > 0f ? $"+{value:0}" : value.ToString("0");
        }
    }
}
