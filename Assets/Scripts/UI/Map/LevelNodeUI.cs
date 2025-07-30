using Game.Map;
using Game.Scene;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Map
{
    public class LevelNodeUI : MonoBehaviour
    {
        [SerializeField]
        private RectTransform connectorPoint;

        private MapNode mapNode;

        private Image nodeIcon;
        private Button nodeButton;
        private string displayName;

        public MapNode MapNode => mapNode;

        private void Awake()
        {
            nodeIcon = GetComponent<Image>();
            nodeButton = GetComponent<Button>();
        }

        public void Initialize(Sprite icon,  LevelType levelType, MapNode mapNode)
        {
            nodeIcon.sprite = icon;
            this.mapNode = mapNode;
            //nodeButton.onClick.AddListener(() => onClickAction?.Invoke(levelType));
        }

        public void Setup(LevelData levelData)
        {           
            displayName = levelData.DisplayName;
            nodeButton.onClick.RemoveAllListeners();
            nodeButton.onClick.AddListener(() =>
            {
                LevelSelectionController.Instance.SetLastVisitedNode(mapNode);
                MainSceneController.Instance.LoadGameplay(levelData);
            });
            nodeIcon.color = new Color(1, 1, 1, 1);
        }

        public RectTransform GetConnectorPoint()
        {
            return connectorPoint;
        }
    }
}
