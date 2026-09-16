using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI.InputPrompts
{
    // Maps input control paths to glyph sprites for one device family (keyboard/mouse, Xbox, PlayStation...).
    [CreateAssetMenu(fileName = "InputIconSet", menuName = "Input/Input Icon Set")]
    public class InputIconSet : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            [Tooltip("Full binding path (<Gamepad>/buttonSouth) or just the control name (buttonSouth, space, leftButton). Both match.")]
            public string controlPath;
            public Sprite sprite;
        }

        [SerializeField] private List<Entry> entries = new();

        private Dictionary<string, Sprite> lookup;

        private void OnEnable()
        {
            lookup = null;
        }

        private void OnValidate()
        {
            lookup = null;
        }

        public bool TryGetSprite(string controlPath, out Sprite sprite)
        {
            sprite = null;
            if (string.IsNullOrEmpty(controlPath)) return false;

            if (lookup == null)
            {
                lookup = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
                foreach (var entry in entries)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.controlPath) || entry.sprite == null) continue;
                    string key = entry.controlPath.Trim();
                    lookup[key] = entry.sprite;
                    // Also register the bare control name, so "<Gamepad>/buttonSouth" and
                    // "buttonSouth" are interchangeable when authoring the asset.
                    string shortKey = Normalize(key);
                    if (!string.IsNullOrEmpty(shortKey) && !lookup.ContainsKey(shortKey))
                    {
                        lookup[shortKey] = entry.sprite;
                    }
                }
            }

            if (lookup.TryGetValue(controlPath, out sprite)) return true;
            return lookup.TryGetValue(Normalize(controlPath), out sprite);
        }

        // "<Gamepad>/buttonSouth" -> "buttonSouth", "*/{Submit}" -> "Submit", "buttonSouth" -> "buttonSouth".
        public static string Normalize(string controlPath)
        {
            if (string.IsNullOrEmpty(controlPath)) return controlPath;

            int slash = controlPath.LastIndexOf('/');
            string name = slash >= 0 ? controlPath.Substring(slash + 1) : controlPath;
            return name.Trim().Trim('{', '}');
        }
    }
}
