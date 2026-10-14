using Game.Control;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    // Select/Deselect mirror pointer enter/exit so the class info also updates with gamepad navigation.
    public class ClassSelectHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private CharacterClassData classData;
        [Tooltip("Button tint while the class is locked. It stays selectable, so its info (and unlock cost) can still be read.")]
        [SerializeField] private Color lockedTint = new Color(0.4f, 0.4f, 0.4f, 1f);

        public event Action<CharacterClassData> OnHoverEnter;
        public event Action OnHoverExit;

        public CharacterClassData ClassData => classData;

        private Graphic tintTarget;
        private Color unlockedColor = Color.white;

        private void Awake()
        {
            var button = GetComponent<Button>();
            tintTarget = button != null && button.targetGraphic != null ? button.targetGraphic : GetComponent<Graphic>();
            if (tintTarget != null) unlockedColor = tintTarget.color;
        }

        public void SetLocked(bool locked)
        {
            if (tintTarget != null) tintTarget.color = locked ? lockedTint : unlockedColor;
        }

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
