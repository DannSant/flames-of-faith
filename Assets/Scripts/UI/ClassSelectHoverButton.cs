using Game.Control;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    // A class card in the main menu's class select. Select/Deselect mirror pointer enter/exit, so gamepad navigation
    // previews a class like hovering does; only clicking the card (A on it) chooses it. Shows a "Locked" note while
    // the class is locked and a ring while it's the chosen one.
    public class ClassSelectHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private CharacterClassData classData;
        [Tooltip("Button tint while the class is locked. It stays selectable, so its info (and unlock cost) can still be read.")]
        [SerializeField] private Color lockedTint = new Color(0.4f, 0.4f, 0.4f, 1f);
        [Tooltip("Optional. Shown while the class is locked (e.g. a red \"Locked\" note).")]
        [SerializeField] private GameObject lockedLabel;
        [Tooltip("Optional. Shown while this is the chosen class.")]
        [SerializeField] private GameObject selectedMark;

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
            if (lockedLabel != null) lockedLabel.SetActive(locked);
        }

        public void SetSelected(bool selected)
        {
            if (selectedMark != null) selectedMark.SetActive(selected);
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
