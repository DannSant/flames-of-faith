using Game.Progression;
using Game.Scene;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class ExperienceBar : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelText;

        private Slider experienceSlider;
        private Coroutine fillCoroutine;

        [SerializeField] private float fillSpeed = 5f;


        private void Awake()
        {
            experienceSlider = GetComponent<Slider>();            
        }

        private void Start()
        {
            
        }

        private void OnEnable()
        {
            var mainSceneController = MainSceneController.Instance;
            if (mainSceneController != null)
            {
                mainSceneController.OnGameplayUISetupRequested += SetupEvents;
            }
        }

        private void OnDisable()
        {
            var mainSceneController = MainSceneController.Instance;
            if (mainSceneController != null)
            {
                mainSceneController.OnGameplayUISetupRequested -= SetupEvents;
            }
        }

        private void SetupEvents()
        {
           
            var playerExperience = PlayerManager.Instance.GetPlayerComponent<PlayerExperience>();
            if (playerExperience != null)
            {
                playerExperience.OnPlayerExperienceGainEvent += OnPlayerExperienceGain;
                playerExperience.onLevelUp += OnPlayerLevelUp;
            }            
        }

        

        private void OnDestroy()
        {
            var playerExperience = PlayerManager.Instance.GetPlayerComponent<PlayerExperience>();
            if (playerExperience != null)
            {
                playerExperience.OnPlayerExperienceGainEvent -= OnPlayerExperienceGain;
                playerExperience.onLevelUp -= OnPlayerLevelUp;
            }
        }

        private void OnPlayerExperienceGain(int currentExperience, int maxExperience)
        {
           
            if (fillCoroutine != null)
                StopCoroutine(fillCoroutine);

            fillCoroutine = StartCoroutine(AnimateFill(currentExperience, maxExperience));
        }

        private IEnumerator AnimateFill(int targetXP, int maxXP)
        {
            experienceSlider.maxValue = maxXP;
            float targetValue = targetXP;
            float startValue = experienceSlider.value;

            while (Mathf.Abs(experienceSlider.value - targetValue) > 0.01f)
            {
                experienceSlider.value = Mathf.Lerp(experienceSlider.value, targetValue, Time.deltaTime * fillSpeed);
                yield return null;
            }

            experienceSlider.value = targetValue; // Snap to exact value at the end
        }

        private void OnPlayerLevelUp(int newLevel, int newMaxXP) 
        {
            //Debug.Log($"ExperienceBar: OnPlayerLevelUp newLevel: {newLevel},newMaxXP: {newMaxXP}");
            levelText.SetText($"Level {newLevel}");           
        }
    }
}