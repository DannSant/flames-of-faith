using Game.UI.SkillTree;
using Game.UI.Style;
using NUnit.Framework;
using UnityEditor;

namespace Game.Tests
{
    /// <summary>
    /// The skill tree keeps its own layout (SkillTreePanelBuilder) but takes the standard colors from UISettings:
    /// Favors in gold (set by SkillTreeUI at runtime), and class tabs like the Stats pane's (yellow when selected).
    /// </summary>
    public class SkillTreeStyleTests
    {
        private static UISettings Settings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<UISettings>("Assets/Resources/UI/UISettings.asset");
            Assert.IsNotNull(settings, "Missing UISettings");
            return settings;
        }

        [Test]
        public void SelectedTab_IsYellow_EvenWhenLocked()
        {
            var settings = Settings();
            Assert.AreEqual(settings.HighlightColor, SkillTreeUI.GetTabLabelColor(true, true, settings));
            Assert.AreEqual(settings.HighlightColor, SkillTreeUI.GetTabLabelColor(true, false, settings), "A locked class can be selected to unlock it");
        }

        [Test]
        public void OtherTabs_AreWhite_OrMutedWhenLocked()
        {
            var settings = Settings();
            Assert.AreEqual(settings.TextColor, SkillTreeUI.GetTabLabelColor(false, true, settings));
            Assert.AreEqual(settings.MutedTextColor, SkillTreeUI.GetTabLabelColor(false, false, settings));
        }
    }
}
