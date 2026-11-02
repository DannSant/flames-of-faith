using Game.Control;
using Game.UI.Navigation;
using Game.UI.Style;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Chronicle
{
    /// <summary>A line of the Chronicle's scene list.</summary>
    public readonly struct ChronicleEntry
    {
        public readonly string Id;
        public readonly string Title;

        public ChronicleEntry(string id, string title)
        {
            Id = id;
            Title = title;
        }
    }

    /// <summary>
    /// The Pilgrim's Chronicle, a main menu panel. It only renders what it's given and reports what's pressed. Each tab
    /// is a button and a page, so another section (the compendium) only needs a new entry in tabs. Back and Cancel ask
    /// whoever opened it to close it. Layout: ChronicleRestyler.
    /// </summary>
    public class ChronicleUI : MonoBehaviour
    {
        [Serializable]
        private class Tab
        {
            public Button button;
            public TextMeshProUGUI label;
            public GameObject page;
        }

        [SerializeField] private List<Tab> tabs = new();
        [SerializeField] private ScrollRect scrollRect;
        [Tooltip("The list's rows are copies of this one, which stays hidden.")]
        [SerializeField] private ChronicleRowUI rowTemplate;
        [Tooltip("Shown when no scene has been seen yet.")]
        [SerializeField] private GameObject emptyText;
        [SerializeField] private Button backButton;

        private readonly List<ChronicleRowUI> rows = new();

        public event Action OnCloseRequested;

        private void Awake()
        {
            if (backButton != null) backButton.onClick.AddListener(RequestClose);
            for (int i = 0; i < tabs.Count; i++)
            {
                int index = i;
                if (tabs[i].button != null) tabs[i].button.onClick.AddListener(() => SelectTab(index));
            }
        }

        public void RequestClose() => OnCloseRequested?.Invoke();

        /// <summary>Fills the scene list. Call it before the panel opens, so the gamepad lands on the first row.</summary>
        public void Show(IReadOnlyList<ChronicleEntry> entries, Action<string> onPlay)
        {
            foreach (var row in rows)
            {
                // Destroy waits for the end of the frame; the list has to be right now
                row.gameObject.SetActive(false);
                Destroy(row.gameObject);
            }
            rows.Clear();

            foreach (var entry in entries)
            {
                var row = Instantiate(rowTemplate, rowTemplate.transform.parent);
                row.gameObject.SetActive(true);
                string id = entry.Id;
                row.Setup(entry.Title, () => onPlay?.Invoke(id), HandleRowSelected);
                rows.Add(row);
            }

            if (emptyText != null) emptyText.SetActive(entries.Count == 0);
            SelectTab(0);

            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.StopMovement();
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        // The selected tab uses the picked tab art and a yellow label, like the Stats pane's
        private void SelectTab(int index)
        {
            var settings = UISettings.Instance;
            for (int i = 0; i < tabs.Count; i++)
            {
                bool selected = i == index;
                if (tabs[i].page != null) tabs[i].page.SetActive(selected);
                if (tabs[i].label != null) tabs[i].label.color = selected ? settings.HighlightColor : settings.TextColor;
                if (tabs[i].button != null && tabs[i].button.targetGraphic is Image image) image.sprite = settings.GetTabSprite(selected);
            }
        }

        // With the gamepad, scroll so the whole row is visible (the mouse scrolls by itself)
        private void HandleRowSelected(ChronicleRowUI row)
        {
            if (InputDeviceManager.Instance == null || !InputDeviceManager.Instance.IsGamepadActive) return;
            ScrollRectMath.ScrollToShow(scrollRect, (RectTransform)row.transform, UISettings.Instance.Spacing);
        }
    }
}
