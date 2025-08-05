using Game.Map;
using Game.Scene;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Map
{
    public class LevelNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private RectTransform connectorPoint;

        private MapNode mapNode;

        private Image nodeIcon;
        private Button nodeButton;
        private string displayName;
        private Action<string> SetLevelNameDisplayText;

        public MapNode MapNode => mapNode;

        private void Awake()
        {
            nodeIcon = GetComponent<Image>();
            nodeButton = GetComponent<Button>();
        }

        public void Initialize(Sprite icon,  LevelType levelType, MapNode mapNode, Action<string> SetLevelNameDisplayText)
        {
            nodeIcon.sprite = icon;
            this.mapNode = mapNode;
            this.SetLevelNameDisplayText = SetLevelNameDisplayText;
            //nodeButton.onClick.AddListener(() => onClickAction?.Invoke(levelType));
        }

        public void Setup(LevelData levelData)
        {        
            if(levelData == null)
            {
                Debug.LogError("LevelData is null in LevelNodeUI.Setup");
                return;
            }
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

        public void OnPointerExit(PointerEventData eventData)
        {
           SetLevelNameDisplayText?.Invoke(string.Empty);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetLevelNameDisplayText?.Invoke(displayName);
        }
    }
}
