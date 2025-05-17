using Game.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scene
{
    public class LoadingScreen : Singleton<LoadingScreen>
    {
       
        private Image fadeImage;
        protected override void Awake()
        {
            base.Awake();            
            fadeImage = transform.GetChild(0).gameObject.GetComponent<Image>();
        }

        public void Show(float alpha = 1f)
        {
            fadeImage.gameObject.SetActive(true);
            SetAlpha(alpha);
        }

        public void Hide()
        {
            fadeImage.gameObject.SetActive(false);
        }

        public void SetAlpha(float alpha)
        {
            if (fadeImage != null)
            {
                Color color = fadeImage.color;
                fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            }
        }

        public Image GetFadeImage() => fadeImage;
    }
}
