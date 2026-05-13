using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Boss
{
    public class FlameBarUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainContainer;
        [SerializeField] private Slider flameSlider;
        private void Start()
        {
            var bossHandler = FindAnyObjectByType<Game.Boss.BossWaveHandler>();
          
            if (bossHandler != null)
            {
                bossHandler.OnBossFightStarted += ShowFlameBar;
                bossHandler.OnFlameProgressChanged += UpdateFlameBar;
                bossHandler.OnBossFightEnded += HideFlameBar;
                ShowFlameBar();
            }
        }

        private void OnDisable()
        {
            var bossHandler = FindAnyObjectByType<Game.Boss.BossWaveHandler>();
            if (bossHandler != null)
            {
                bossHandler.OnBossFightStarted -= ShowFlameBar;
                bossHandler.OnFlameProgressChanged -= UpdateFlameBar;
                bossHandler.OnBossFightEnded -= HideFlameBar;
            }
        }

        private void ShowFlameBar()
        {
            mainContainer.SetActive(true);
            flameSlider.value = 0f;
        }

        private void UpdateFlameBar(float current, float max)
        {
            float progress = Mathf.Clamp01(current / max);
            flameSlider.value = progress;
        }

        private void HideFlameBar()
        {
            mainContainer.SetActive(false);
        }


    }

}