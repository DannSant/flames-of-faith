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

        [TestCase(false, true, true)]   // affordable: green
        [TestCase(false, false, false)] // not enough Favors: red
        [TestCase(true, false, true)]   // MAX: green
        public void NodeCost_IsGreenWhenAffordable_RedWhenNot(bool maxed, bool canUpgrade, bool green)
        {
            var settings = Settings();
            Assert.AreEqual(green ? settings.PositiveColor : settings.NegativeColor, SkillTreeNodeUI.GetCostColor(maxed, canUpgrade, settings));
        }

        // Class root: "Unlocked." / "Unlock: N Favors ... Click to unlock." green, not enough Favors red
        [TestCase(true, true, false, false, true)]
        [TestCase(true, false, true, false, true)]
        [TestCase(true, false, false, false, false)]
        // Item node: affordable upgrade green; not enough Favors, or the class still locked, red
        [TestCase(false, true, false, true, true)]
        [TestCase(false, true, false, false, false)]
        [TestCase(false, false, true, true, false)]
        public void DetailsFooter_IsGreenWhenAvailable_RedWhenNot(bool classRoot, bool classUnlocked, bool canUnlockClass, bool canUpgrade, bool green)
        {
            Assert.AreEqual(green, SkillTreeUI.IsFooterPositive(classRoot, classUnlocked, canUnlockClass, canUpgrade));
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
