using Game.Scene;
using UnityEngine;

namespace Game.UI
{
    public class LevelSelectorControllerUI : MonoBehaviour
    {
        [SerializeField] private GameObject container;
        [SerializeField] LevelButtonUI levelButtonPrefab;

        private void Awake()
        {
            
        }

        private void Start()
        {
            BuildLevelButtons();
        }

        private void BuildLevelButtons() 
        {          
            // Clear all stat rows
            for (int i = container.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(container.transform.GetChild(i).gameObject);
            }

            var availableLevels = GameSession.Instance.GetAvailableLevels();

            for (int i = 0; i < availableLevels.Count; i++)
            {
                var level = availableLevels[i];
                bool isNextLevel = (i == availableLevels.Count - 1 && !level.IsBeaten);

                var levelButton = Instantiate(levelButtonPrefab, container.transform);
                levelButton.Initialize(level, isNextLevel);
            }
        }
    }

}