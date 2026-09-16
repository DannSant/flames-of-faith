using Game.Control;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    // Select/Deselect mirror pointer enter/exit so the class info also updates with gamepad navigation.
    public class ClassSelectHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
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

        public void OnSelect(BaseEventData eventData)
        {
            OnHoverEnter?.Invoke(classData);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            OnHoverExit?.Invoke();
        }
    }
}
