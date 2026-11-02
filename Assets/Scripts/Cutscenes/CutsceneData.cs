using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cutscenes
{
    /// <summary>One page of a cutscene: a widescreen image and its text. The player advances by hand, so a page has no duration.</summary>
    [Serializable]
    public class CutscenePage
    {
        [Tooltip("Widescreen, 1920 x 1080. The text band covers the bottom quarter.")]
        public Sprite image;
        [TextArea(2, 6)]
        [Tooltip("Line breaks are kept. Player text: Pilgrimage, never \"run\"; no em dash, middle dot or vertical bar (the font lacks them).")]
        public string text;
    }

    /// <summary>
    /// A cutscene: static pages, played by id through CutscenePlayer. Adding a scene only needs a new asset in
    /// Resources/Cutscenes; its id is saved in the meta progression, so never change it.
    /// </summary>
    [CreateAssetMenu(fileName = "Cutscene", menuName = "Flames of Faith/Cutscene")]
    public class CutsceneData : ScriptableObject
    {
        [Tooltip("Stable id: saved in the seen list, and how code starts the scene. Never change it.")]
        [SerializeField] private string cutsceneId;
        [Tooltip("The name in the Pilgrim's Chronicle.")]
        [SerializeField] private string title;
        [SerializeField] private List<CutscenePage> pages = new();

        public string CutsceneId => cutsceneId;
        public string Title => title;
        public IReadOnlyList<CutscenePage> Pages => pages;
    }
}
