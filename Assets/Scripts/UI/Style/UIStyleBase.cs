using UnityEngine;

namespace Game.UI.Style
{
    /// <summary>
    /// Base for the UIStyle* components: each one marks an element with its role in the UI standard
    /// (Small/Big text, Medium icon...) and applies the matching UISettings values when it wakes up,
    /// so UISettings stays the single source of truth. The editor's Re-apply UI Styles tool calls
    /// Apply on every prefab so the Editor shows the same values.
    /// </summary>
    public abstract class UIStyleBase : MonoBehaviour
    {
        protected virtual void Awake()
        {
            Apply(UISettings.Instance);
        }

        /// <summary>Applies the settings. Returns true if anything changed.</summary>
        public abstract bool Apply(UISettings settings);
    }
}
