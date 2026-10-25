using Game.Progression;
using Game.UI;
using NUnit.Framework;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The Stats pane's tooltip must not outlive the pane (tester bug, 2026-10-25). Hidden rows get no pointer
    /// exit, so closing the pane with the mouse on a row left the tooltip up, and the remembered hovered stat
    /// made Shift show it again after the pane was closed.
    /// </summary>
    public class StatsTooltipTests
    {
        private static readonly FieldInfo HoveredStatField =
            typeof(StatsPaneUI).GetField("hoveredStat", BindingFlags.NonPublic | BindingFlags.Instance);

        private GameObject root;
        private StatsPaneUI pane;
        private int hoverEndedCount;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("StatsPane");
            pane = root.AddComponent<StatsPaneUI>();
            var ui = new SerializedObject(pane);
            ui.FindProperty("primaryContentPanel").objectReferenceValue = Child("Primary");
            ui.FindProperty("secondaryContentPanel").objectReferenceValue = Child("Secondary");
            ui.FindProperty("backgroundPanel").objectReferenceValue = Child("Background");
            ui.ApplyModifiedPropertiesWithoutUndo();

            hoverEndedCount = 0;
            pane.OnStatHoverEnded += () => hoverEndedCount++;
            Assert.IsNotNull(HoveredStatField, "StatsPaneUI.hoveredStat was renamed");
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(root);

        private GameObject Child(string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(root.transform);
            return child;
        }

        private void Hover(StatType stat) => HoveredStatField.SetValue(pane, (StatType?)stat);

        private StatType? Hovered => (StatType?)HoveredStatField.GetValue(pane);

        [Test]
        public void HidingThePane_ForgetsTheHoveredStat()
        {
            Hover(StatType.Armor);

            pane.HideStatsWindow(0);

            Assert.IsNull(Hovered, "Shift would show the stat's tooltip again after the pane closed");
            Assert.AreEqual(1, hoverEndedCount, "The Inventory keeps its Shift dimming without the hover end");
        }

        [Test]
        public void HidingThePane_WithNothingHovered_EndsNoHover()
        {
            pane.HideStatsWindow(0);

            Assert.AreEqual(0, hoverEndedCount, "Closing the pane must not hide a tooltip it didn't open");
        }

        [Test]
        public void HidingTheHoveredRow_EndsTheHover()
        {
            Hover(StatType.Armor);

            pane.NotifyRowDisabled(StatType.Armor);

            Assert.IsNull(Hovered);
            Assert.AreEqual(1, hoverEndedCount);
        }

        [Test]
        public void HidingAnotherRow_KeepsTheHover()
        {
            Hover(StatType.Armor);

            pane.NotifyRowDisabled(StatType.Luck);

            Assert.AreEqual(StatType.Armor, Hovered);
            Assert.AreEqual(0, hoverEndedCount);
        }
    }
}
