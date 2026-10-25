using Game.Effects;
using Game.UI;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// An item slot's tooltip contract, shared by the Inventory and the item bag (tester feedback, 2026-10-25:
    /// the bag's tooltip used to stay up for the selected item). Hover or gamepad focus shows it, leaving hides
    /// it, and a slot only hides a tooltip it opened.
    /// </summary>
    public class SlotTooltipTests
    {
        private GameObject root;
        private EffectIconUI slot;
        private Effect effect;
        private int shown;
        private int hidden;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Slot");
            slot = root.AddComponent<EffectIconUI>();
            var icon = new GameObject("Icon").AddComponent<Image>();
            icon.transform.SetParent(root.transform);
            var count = new GameObject("QtyText").AddComponent<TextMeshProUGUI>();
            count.transform.SetParent(root.transform);

            var ui = new SerializedObject(slot);
            ui.FindProperty("icon").objectReferenceValue = icon;
            ui.FindProperty("quantitytext").objectReferenceValue = count;
            ui.ApplyModifiedPropertiesWithoutUndo();

            effect = ScriptableObject.CreateInstance<Effect>();
            shown = hidden = 0;
            slot.Setup(effect, null, 1, _ => shown++, () => hidden++);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(effect);
        }

        [Test]
        public void Hover_ShowsThenHides()
        {
            slot.OnPointerEnter(null);
            Assert.AreEqual(1, shown);

            slot.OnPointerExit(null);
            Assert.AreEqual(1, hidden, "Leaving the slot must hide its tooltip");
        }

        [Test]
        public void GamepadFocus_ShowsThenHides()
        {
            slot.OnSelect(null);
            Assert.AreEqual(1, shown);

            slot.OnDeselect(null);
            Assert.AreEqual(1, hidden, "Moving focus off the slot must hide its tooltip");
        }

        [Test]
        public void LeavingWithoutShowing_HidesNothing()
        {
            slot.OnPointerExit(null);
            slot.OnDeselect(null);

            Assert.AreEqual(0, hidden, "A slot only hides a tooltip it opened");
        }
    }
}
