using Game.Cutscenes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Cutscenes
{
    /// <summary>
    /// Shows a cutscene's pages: the image fills the screen and the text sits in a band across its bottom. Click, Enter
    /// or A advances; holding Esc, B or the Skip bar skips (SkipHold). A tap of Esc or B does nothing, so it can't leave
    /// a half-played scene. Reads its own copy of the UI actions, because the player's only exist once a player does.
    /// Everything runs on unscaled time. Layout: CutsceneRestyler.
    /// </summary>
    public class CutsceneWindowUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup windowGroup;
        [Tooltip("The image, the band and the text: faded between pages.")]
        [SerializeField] private CanvasGroup pageGroup;
        [SerializeField] private Image pageImage;
        [SerializeField] private TextMeshProUGUI pageText;
        [Tooltip("Covers the screen behind the page: a click anywhere advances.")]
        [SerializeField] private Button clickArea;
        [SerializeField] private Button nextButton;
        [SerializeField] private TextMeshProUGUI nextLabel;
        [SerializeField] private HoldButtonUI skipBar;
        [SerializeField] private Image skipFill;

        [Header("Tuning")]
        [Tooltip("How long the window takes to fade in and out (unscaled seconds).")]
        [SerializeField] private float windowFadeDuration = 0.3f;
        [Tooltip("How long a page takes to fade out, and the next one in (unscaled seconds each).")]
        [SerializeField] private float pageFadeDuration = 0.25f;
        [Tooltip("How long skip has to be held (unscaled seconds).")]
        [SerializeField] private float skipHoldTime = 1f;
        [Tooltip("Longest frame counted toward the hold, so a hitch can't complete it.")]
        [SerializeField] private float maxHoldStep = 0.1f;
        [SerializeField] private string nextText = "Next";
        [SerializeField] private string lastPageText = "Continue";

        private InputSystem_Actions actions;
        private IReadOnlyList<CutscenePage> pages;
        private Action onClosed;
        private SkipHold skipHold;
        private int pageIndex;
        // A fade is running, so advancing does nothing. It also makes one press count once: A reaches both the Submit
        // action here and the selected Next button.
        private bool busy;
        private bool closing;

        private bool IsOpen => onClosed != null;

        private void Awake()
        {
            if (clickArea != null) clickArea.onClick.AddListener(Advance);
            if (nextButton != null) nextButton.onClick.AddListener(Advance);
        }

        /// <summary>Opens the window on the first page. onClosed fires once, when the scene has ended or been skipped.</summary>
        public void Show(IReadOnlyList<CutscenePage> scenePages, Action onClosed)
        {
            pages = scenePages;
            this.onClosed = onClosed;
            pageIndex = 0;
            closing = false;
            busy = true;
            skipHold = new SkipHold(skipHoldTime);

            actions ??= new InputSystem_Actions();
            actions.UI.Enable();

            gameObject.SetActive(true);
            windowGroup.alpha = 0f;
            pageGroup.alpha = 0f;
            skipFill.fillAmount = 0f;
            ShowPage(0);
            StartCoroutine(OpenRoutine());
        }

        /// <summary>Closes at once, without reporting an end (the scene failed to start).</summary>
        public void Abort()
        {
            onClosed = null;
            CloseNow();
        }

        private void OnDisable()
        {
            // Whatever turned the window off, whoever waits for this scene must not wait forever
            if (IsOpen) EndScene();
        }

        private void OnDestroy()
        {
            actions?.Dispose();
        }

        private void Update()
        {
            if (!IsOpen || closing) return;

            bool held = actions.UI.Cancel.IsPressed() || (skipBar != null && skipBar.IsHeld);
            if (skipHold.Tick(held, Mathf.Min(Time.unscaledDeltaTime, maxHoldStep)))
            {
                BeginClosing();
                return;
            }
            skipFill.fillAmount = skipHold.Progress;

            if (actions.UI.Submit.WasPressedThisFrame()) Advance();
        }

        private void Advance()
        {
            if (!IsOpen || busy || closing) return;

            if (pageIndex + 1 < pages.Count) StartCoroutine(ChangePageRoutine(pageIndex + 1));
            else BeginClosing();
        }

        private void ShowPage(int index)
        {
            pageIndex = index;
            var page = pages[index];
            pageImage.sprite = page.image;
            pageImage.enabled = page.image != null;
            pageText.text = page.text;
            nextLabel.text = index + 1 < pages.Count ? nextText : lastPageText;
        }

        private void BeginClosing()
        {
            StopAllCoroutines();
            closing = true;
            StartCoroutine(CloseRoutine());
        }

        private IEnumerator OpenRoutine()
        {
            yield return Fade(windowGroup, windowGroup.alpha, 1f, windowFadeDuration);
            yield return Fade(pageGroup, pageGroup.alpha, 1f, pageFadeDuration);
            busy = false;
        }

        private IEnumerator ChangePageRoutine(int index)
        {
            busy = true;
            yield return Fade(pageGroup, 1f, 0f, pageFadeDuration);
            ShowPage(index);
            yield return Fade(pageGroup, 0f, 1f, pageFadeDuration);
            busy = false;
        }

        private IEnumerator CloseRoutine()
        {
            yield return Fade(pageGroup, pageGroup.alpha, 0f, pageFadeDuration);
            yield return Fade(windowGroup, windowGroup.alpha, 0f, windowFadeDuration);
            EndScene();
        }

        private void EndScene()
        {
            var ended = onClosed;
            onClosed = null;
            CloseNow();
            ended?.Invoke();
        }

        private void CloseNow()
        {
            actions?.UI.Disable();
            gameObject.SetActive(false);
        }

        private static IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            group.alpha = to;
        }
    }
}
