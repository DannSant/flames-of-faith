using Game.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Level
{
    public class UIFade : Singleton<UIFade>
    {
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeSpeed = 1f;

        private const float MAX_ALPHA = .75f;

        private IEnumerator fadeRoutine;

        public void FadeToBlack()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }
            fadeRoutine = FadeRoutine(MAX_ALPHA);
            StartCoroutine(fadeRoutine);
        }

        public void FadeToClear()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }
            fadeRoutine = FadeRoutine(0f);
            StartCoroutine(fadeRoutine);
        }

        private IEnumerator FadeRoutine(float targetAlpha)
        {
            while (!Mathf.Approximately(fadeImage.color.a, targetAlpha))
            {
                float alpha = Mathf.MoveTowards(fadeImage.color.a, targetAlpha, fadeSpeed * Time.deltaTime);
                fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
                yield return null;
            }
        }
    }
}
