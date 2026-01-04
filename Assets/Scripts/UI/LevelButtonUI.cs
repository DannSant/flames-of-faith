using Game.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class LevelButtonUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelNameTxt;
        [SerializeField] private Button button;
        [SerializeField] private Image background; // Optional background for highlight
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private Image beatenImage;
        [SerializeField] private Sprite beatenTexture; // Optional icon to indicate cleared level
        [SerializeField] private Sprite notBeatenTexture;

        private LevelData levelData;

        public void Initialize(LevelData levelData, bool isNextLevel)
        {
            this.levelData = levelData;

            // Set level name
            if (levelNameTxt != null)
            {
                levelNameTxt.text = levelData.DisplayName;
            }

            

            // Highlight next level
            if (background != null)
            {
                background.color = isNextLevel ? highlightColor : normalColor;
            }
        }
    }
}

