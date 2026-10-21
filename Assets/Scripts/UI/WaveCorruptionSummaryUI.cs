using Game.UI.Style;
using Game.Waves;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// End of wave summary, as a receipt of what happened to Grace: where this wave's Corruption came from (-N),
    /// the Grace Affinity (+N), then Grace + Grace Affinity - Corruption = new Grace, counted on a Grace bar.
    /// Grace is already applied when this opens, so it takes over the HUD Grace bar (GraceBar presentation):
    /// the HUD shows the old values, the Corruption number flies from the HUD into its row, Grace flies into the
    /// formula, and the result flies back to the HUD, which only then changes. The button reads Skip while it
    /// plays (jumps to the end) and Continue after (closes). Tells the wave sequence when it's done.
    /// Layout: WaveSummaryRestyler.
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
        [Tooltip("Optional. Favors earned this wave (shown after the Grace result).")]
        [SerializeField] private TextMeshProUGUI favorsRowText;

        [Header("Result")]
        [Tooltip("Shows the subtraction, e.g. 'Grace +5 + 1 - 6 = 0'.")]
        [SerializeField] private TextMeshProUGUI formulaText;
        [Tooltip("The summary's Grace bar, counted with the formula: fill for Grace above 0...")]
        [SerializeField] private Image graceMeterPositive;
        [Tooltip("...and fill for Grace below 0 (Corrupted).")]
        [SerializeField] private Image graceMeterNegative;

        [Header("Continue")]
        [SerializeField] private Button continueButton;
        [SerializeField] private TextMeshProUGUI continueLabel;
        [SerializeField] private string skipText = "Skip";
        [SerializeField] private string continueText = "Continue";
        [Tooltip("Wait for Continue before closing. When off, the summary closes after Hold Duration.")]
        [SerializeField] private bool waitForConfirm = true;
        [Tooltip("Only used when Wait For Confirm is off.")]
        [SerializeField] private float holdDuration = 1.5f;

        [Header("Flying numbers")]
        [Tooltip("The Corruption and Grace numbers fly between the HUD Grace bar and this window. When off, the HUD bar follows the count instead.")]
        [SerializeField] private bool flyingNumbers = true;
        [SerializeField] private float flightDuration = 0.5f;
        [Tooltip("How far the flight path bulges up at its middle (canvas units).")]
        [SerializeField] private float flightArc = 40f;

        [Header("Timing")]
        [SerializeField] private float fadeDuration = 0.2f;
        [SerializeField] private float rowRevealDelay = 0.45f;
        [SerializeField] private float countDuration = 2f;
        [Tooltip("Skip the summary when nothing changed this wave (no Corruption and no Grace Affinity).")]
        [SerializeField] private bool skipWhenNothingChanged = true;

        private struct Row
        {
            public TextMeshProUGUI text;
            public string label;
            public string value;
            public string hex;
            public bool fliesFromHud;
        }

        private Coroutine playRoutine;
        private Action pendingCallback;
        private GraceBar hudBar;
        private WaveCorruptionResult current;
        private bool animating;
        private bool closeRequested;
        private readonly List<GameObject> flights = new List<GameObject>();

        private void Awake()
        {
            if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
        }

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
        }

        private void OnDisable()
        {
            ClearFlights();
            if (hudBar != null) hudBar.EndPresentation();
        }

        private void OnDestroy()
        {
            if (continueButton != null) continueButton.onClick.RemoveListener(OnContinueClicked);
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveCorruptionResolved -= Show;
            }
            CompletePending();
        }

        public void Show(WaveCorruptionResult result, Action onFinished)
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                ClearFlights();
                CompletePending();
            }

            pendingCallback = onFinished;

            bool nothingChanged = result.totalCorruption == 0 && Mathf.Approximately(result.graceAffinity, 0f);
            if (panel == null || (skipWhenNothingChanged && nothingChanged))
            {
                CompletePending();
                return;
            }

            current = result;
            // UI-to-UI lookup only: the HUD bar is fed by gameplay events of its own
            if (hudBar == null) hudBar = FindAnyObjectByType<GraceBar>();
            // Same frame as the resolution: the HUD never shows the new values before the summary explains them
            if (hudBar != null) hudBar.BeginPresentation(result.graceBefore, result.totalCorruption);

            playRoutine = StartCoroutine(Play(result));
        }

        // --- The sequence ----------------------------------------------------------------------------------

        private IEnumerator Play(WaveCorruptionResult result)
        {
            var settings = UISettings.Instance;
            foreach (var row in BuildRows(result, settings)) row.text.gameObject.SetActive(false);
            HideRow(favorsRowText);
            if (formulaText != null) formulaText.text = string.Empty;
            SetMeter(result.graceBefore);
            closeRequested = false;
            animating = true;
            SetContinueLabel(skipText);

            panel.SetActive(true);
            yield return Fade(0f, 1f);

            // 1. Where the Corruption came from (the total flies in from the HUD), and what offsets it
            foreach (var row in BuildRows(result, settings))
            {
                if (row.fliesFromHud)
                {
                    row.text.text = FormatRow(row.label, string.Empty, row.hex);
                    row.text.gameObject.SetActive(true);
                    yield return Fly(hudBar != null ? hudBar.PendingCorruptionLabel : null, RowValuePoint(row.text), row.value,
                        settings.CorruptionColor, () => { if (hudBar != null) hudBar.PresentPendingCorruption(0f); });
                }
                row.text.text = FormatRow(row.label, row.value, row.hex);
                row.text.gameObject.SetActive(true);
                yield return new WaitForSeconds(rowRevealDelay);
            }

            // 2. The subtraction: Grace flies in from the HUD, then counts to the new Grace on the bar
            if (formulaText != null)
            {
                string left = FormatFormulaLeft(result.graceBefore, result.graceAffinity, result.totalCorruption,
                    settings.ToHex(UIColorRole.Grace), settings.ToHex(UIColorRole.Corruption));

                if (flyingNumbers)
                {
                    yield return Fly(hudBar != null ? hudBar.GraceLabel : null, CenterPoint(formulaText.rectTransform),
                        $"Grace {Signed(result.graceBefore)}", GraceBar.GetLabelColor(result.graceBefore, settings), null);
                }

                float elapsed = 0f;
                while (elapsed < countDuration)
                {
                    elapsed += Time.deltaTime;
                    float value = Mathf.Lerp(result.graceBefore, result.graceAfter, Mathf.Clamp01(elapsed / countDuration));
                    SetFormula(left, value, settings);
                    SetMeter(value);
                    if (!flyingNumbers && hudBar != null) hudBar.PresentGrace(value, false);
                    yield return null;
                }
                SetFormula(left, result.graceAfter, settings);
                SetMeter(result.graceAfter);

                // 3. The result goes back to the HUD, which changes when it lands
                if (flyingNumbers)
                {
                    yield return Fly(formulaText.rectTransform, CenterPoint(hudBar != null ? hudBar.GraceLabel : null), Signed(Mathf.Round(result.graceAfter)),
                        GraceBar.GetLabelColor(result.graceAfter, settings), null);
                }
            }

            // Favors only come from a wave that ends with Grace above 0
            ShowFinalState(result);
            if (result.favorsEarned > 0) yield return new WaitForSeconds(rowRevealDelay);

            yield return WaitAndClose();
        }

        private void OnContinueClicked()
        {
            if (animating) SkipToEnd();
            else closeRequested = true;
        }

        /// <summary>Everything at once: the rows, the formula, the bar and the HUD at their final values.</summary>
        private void SkipToEnd()
        {
            if (playRoutine != null) StopCoroutine(playRoutine);
            ClearFlights();
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            ShowFinalState(current);
            playRoutine = StartCoroutine(WaitAndClose());
        }

        private void ShowFinalState(WaveCorruptionResult result)
        {
            var settings = UISettings.Instance;
            foreach (var row in BuildRows(result, settings))
            {
                row.text.text = FormatRow(row.label, row.value, row.hex);
                row.text.gameObject.SetActive(true);
            }

            if (formulaText != null)
            {
                SetFormula(FormatFormulaLeft(result.graceBefore, result.graceAffinity, result.totalCorruption,
                    settings.ToHex(UIColorRole.Grace), settings.ToHex(UIColorRole.Corruption)), result.graceAfter, settings);
            }
            SetMeter(result.graceAfter);

            if (favorsRowText != null && result.favorsEarned > 0)
            {
                favorsRowText.text = FormatFavors(result.favorsEarned);
                favorsRowText.gameObject.SetActive(true);
            }

            if (hudBar != null)
            {
                hudBar.PresentPendingCorruption(0f);
                hudBar.PresentGrace(result.graceAfter, true);
            }

            animating = false;
            SetContinueLabel(continueText);
        }

        private IEnumerator WaitAndClose()
        {
            float waited = 0f;
            while (!closeRequested && (waitForConfirm || waited < holdDuration))
            {
                waited += Time.deltaTime;
                yield return null;
            }

            if (hudBar != null) hudBar.EndPresentation();
            yield return Fade(1f, 0f);

            Hide();
            playRoutine = null;
            CompletePending();
        }

        private List<Row> BuildRows(WaveCorruptionResult result, UISettings settings)
        {
            string corruptionHex = settings.ToHex(UIColorRole.Corruption);
            var rows = new List<Row>();

            if (result.corruptor.spawned && corruptorRowText != null)
            {
                string label = result.corruptor.killed ? $"Corruptor killed in {result.corruptor.killTime:0.0}s" : "Corruptor escaped";
                rows.Add(new Row { text = corruptorRowText, label = label, value = FormatDelta(-result.corruptor.corruption), hex = corruptionHex });
            }
            if (result.corruptedDamageTaken > 0f && corruptedDamageRowText != null)
            {
                rows.Add(new Row { text = corruptedDamageRowText, label = $"Corrupted damage taken: {result.corruptedDamageTaken:0}",
                    value = FormatDelta(-result.corruptionFromDamage), hex = corruptionHex });
            }
            if (result.totalCorruption > 0 && totalCorruptionRowText != null)
            {
                rows.Add(new Row { text = totalCorruptionRowText, label = "Corruption this wave", value = FormatDelta(-result.totalCorruption),
                    hex = corruptionHex, fliesFromHud = flyingNumbers });
            }
            if (!Mathf.Approximately(result.graceAffinity, 0f) && graceAffinityRowText != null)
            {
                rows.Add(new Row { text = graceAffinityRowText, label = "Grace Affinity", value = FormatDelta(Mathf.RoundToInt(result.graceAffinity)),
                    hex = settings.ToHex(UIColorRole.Grace) });
            }
            return rows;
        }

        // --- Flying numbers ----------------------------------------------------------------------------------

        private IEnumerator Fly(RectTransform from, Vector3? to, string text, Color color, Action onStart)
        {
            onStart?.Invoke();
            var canvas = panel.GetComponentInParent<Canvas>();
            if (!flyingNumbers || from == null || to == null || canvas == null || flightDuration <= 0f) yield break;

            canvas = canvas.rootCanvas;
            var flight = CreateFlightLabel(canvas.transform, text, color);
            Vector3 start = CenterPoint(from).Value;
            float arc = flightArc * canvas.scaleFactor;
            float elapsed = 0f;
            while (elapsed < flightDuration)
            {
                elapsed += Time.deltaTime;
                flight.position = UIFlight.Evaluate(start, to.Value, elapsed / flightDuration, arc);
                yield return null;
            }

            flights.Remove(flight.gameObject);
            Destroy(flight.gameObject);
        }

        private RectTransform CreateFlightLabel(Transform canvas, string text, Color color)
        {
            var settings = UISettings.Instance;
            var go = new GameObject("FlyingNumber", typeof(RectTransform));
            go.layer = gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(canvas, false);
            rect.SetAsLastSibling();
            rect.sizeDelta = new Vector2(240f, 40f);

            var label = go.AddComponent<TextMeshProUGUI>();
            if (settings != null)
            {
                label.font = settings.OutlinedFont;
                label.fontSize = settings.BigFontSize;
            }
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = false;
            label.color = color;
            label.text = text;

            flights.Add(go);
            return rect;
        }

        private void ClearFlights()
        {
            foreach (var flight in flights)
            {
                if (flight != null) Destroy(flight);
            }
            flights.Clear();
        }

        private static Vector3? CenterPoint(RectTransform rect)
        {
            return rect != null ? rect.TransformPoint(rect.rect.center) : (Vector3?)null;
        }

        // Where a receipt row's value sits: its right end. The row was just shown, so lay it out first.
        private static Vector3? RowValuePoint(TextMeshProUGUI row)
        {
            if (row == null) return null;
            if (row.transform.parent is RectTransform column) LayoutRebuilder.ForceRebuildLayoutImmediate(column);
            Rect r = row.rectTransform.rect;
            return row.rectTransform.TransformPoint(new Vector3(r.xMax - 16f, r.center.y));
        }

        // --- Display -----------------------------------------------------------------------------------------

        private void SetFormula(string left, float value, UISettings settings)
        {
            string resultHex = ColorUtility.ToHtmlStringRGB(GraceBar.GetLabelColor(value, settings));
            formulaText.text = $"{left} = <color=#{resultHex}>{Signed(Mathf.Round(value))}</color>";
        }

        private void SetMeter(float grace)
        {
            Vector2 fills = GraceBar.GetFills(grace, current.graceMax);
            if (graceMeterPositive != null) graceMeterPositive.fillAmount = fills.x;
            if (graceMeterNegative != null) graceMeterNegative.fillAmount = fills.y;
        }

        private void SetContinueLabel(string text)
        {
            if (continueLabel != null) continueLabel.text = text;
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

        // --- Formatting (pure) -------------------------------------------------------------------------------

        /// <summary>A receipt row: the label on the left and the colored value on the right, on one line.</summary>
        public static string FormatRow(string label, string value, string hex)
        {
            return $"{label}<line-height=0>\n<align=right><color=#{hex}>{value}</color>";
        }

        /// <summary>A change to Grace: "+1", "-3", "0".</summary>
        public static string FormatDelta(int value)
        {
            return value > 0 ? $"+{value}" : value.ToString();
        }

        /// <summary>"Grace +5 + 1 - 6": the old Grace, then the Grace Affinity and the Corruption, each in its color.</summary>
        public static string FormatFormulaLeft(float graceBefore, float graceAffinity, int corruption, string graceHex, string corruptionHex)
        {
            string left = $"Grace {Signed(graceBefore)}";
            if (!Mathf.Approximately(graceAffinity, 0f))
            {
                left += $" <color=#{graceHex}>{(graceAffinity > 0f ? "+" : "-")} {Mathf.Abs(graceAffinity):0}</color>";
            }
            if (corruption > 0)
            {
                left += $" <color=#{corruptionHex}>- {corruption}</color>";
            }
            return left;
        }

        public static string FormatFavors(int favors)
        {
            return $"{(favors == 1 ? "Favor" : "Favors")} earned +{favors}";
        }

        private static string Signed(float value)
        {
            return value > 0f ? $"+{value:0}" : value.ToString("0");
        }
    }
}
