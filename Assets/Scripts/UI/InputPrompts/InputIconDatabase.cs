using Game.Control;
using UnityEngine;

namespace Game.UI.InputPrompts
{
    // Groups the glyph sets per device family. Adding PlayStation glyphs later only requires
    // assigning a set here - until then PlayStation falls back to the Xbox set.
    [CreateAssetMenu(fileName = "InputIconDatabase", menuName = "Input/Input Icon Database")]
    public class InputIconDatabase : ScriptableObject
    {
        public const string DefaultResourcePath = "Input/InputIconDatabase";

        [SerializeField] private InputIconSet keyboardMouse;
        [SerializeField] private InputIconSet xbox;
        [SerializeField] private InputIconSet playStation;

        private static InputIconDatabase defaultDatabase;

        public static InputIconDatabase LoadDefault()
        {
            if (defaultDatabase == null)
            {
                defaultDatabase = Resources.Load<InputIconDatabase>(DefaultResourcePath);
            }
            return defaultDatabase;
        }

        public InputIconSet GetSet(InputDeviceFamily family)
        {
            switch (family)
            {
                case InputDeviceFamily.PlayStation:
                    return playStation != null ? playStation : xbox;
                case InputDeviceFamily.Xbox:
                    return xbox;
                default:
                    return keyboardMouse;
            }
        }

        // Falls back to the Xbox set per icon, so a partially filled PlayStation set still shows
        // a glyph for the buttons it doesn't cover yet.
        public bool TryGetSprite(InputDeviceFamily family, string controlPath, out Sprite sprite)
        {
            sprite = null;
            if (string.IsNullOrEmpty(controlPath)) return false;

            InputIconSet set = GetSet(family);
            if (set != null && set.TryGetSprite(controlPath, out sprite)) return true;

            if (family == InputDeviceFamily.PlayStation && xbox != null && xbox != set)
            {
                return xbox.TryGetSprite(controlPath, out sprite);
            }
            return false;
        }
    }
}
