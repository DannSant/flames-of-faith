using Game.Control;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class ClassSelectHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private CharacterClassData classData;

        public event Action<CharacterClassData> OnHoverEnter;
        public event Action OnHoverExit;

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHoverEnter?.Invoke(classData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverExit?.Invoke();
        }
    }
}
