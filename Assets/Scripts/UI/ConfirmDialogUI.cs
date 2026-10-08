using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Generic Yes/No dialog. Callers pass the message and what to do on each answer.
    /// Put a UIWindow on the panel (Primary, AutoFocus, a priority above the window it opens from,
    /// default selectable = Cancel, OnCancel -> ConfirmDialogUI.Cancel) to get gamepad focus.
    /// </summary>
    public class ConfirmDialogUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private Action onConfirm;
        private Action onCancel;

        public bool IsOpen => panel != null && panel.activeSelf;

        private void Awake()
        {
            panel.SetActive(false);
            confirmButton.onClick.AddListener(Confirm);
            cancelButton.onClick.AddListener(Cancel);
        }

        private void OnDestroy()
        {
            confirmButton.onClick.RemoveListener(Confirm);
            cancelButton.onClick.RemoveListener(Cancel);
        }

        public void Show(string message, Action onConfirm, Action onCancel = null)
        {
            this.onConfirm = onConfirm;
            this.onCancel = onCancel;
            messageText.SetText(message);
            panel.SetActive(true);
        }

        public void Confirm()
        {
            var callback = onConfirm;
            Close();
            callback?.Invoke();
        }

        public void Cancel()
        {
            var callback = onCancel;
            Close();
            callback?.Invoke();
        }

        private void Close()
        {
            onConfirm = null;
            onCancel = null;
            panel.SetActive(false);
        }
    }
}
