using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Generic Yes/No dialog. Callers pass the message and what to do on each answer; ShowInfo is the
    /// OK-only variant for messages that ask nothing. The panel is a modal backdrop (it blocks clicks on
    /// the windows behind) holding the framed window; its UIWindow (Primary, AutoFocus, a priority above
    /// the window it opens from, default selectable = Cancel, OnCancel -> ConfirmDialogUI.Cancel) gives
    /// it gamepad focus. Layout: ConfirmEndscreenRestyler.
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
            Open(message, onConfirm, onCancel, showCancel: true);
        }

        /// <summary>A message with only OK. OK and Cancel (B) both close it and run onClose.</summary>
        public void ShowInfo(string message, Action onClose = null)
        {
            Open(message, onClose, onClose, showCancel: false);
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

        private void Open(string message, Action onConfirm, Action onCancel, bool showCancel)
        {
            this.onConfirm = onConfirm;
            this.onCancel = onCancel;
            messageText.SetText(message);
            // Before the panel opens: with Cancel hidden, the window's gamepad focus falls back to OK
            cancelButton.gameObject.SetActive(showCancel);
            panel.SetActive(true);
        }

        private void Close()
        {
            onConfirm = null;
            onCancel = null;
            panel.SetActive(false);
        }
    }
}
