using Game.Effects;
using Game.Progression;
using Game.UI;
using Game.UI.Style;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// The Inventory's Shift view (tester feedback, 2026-10-25): holding Shift on a stat rings the items that
    /// grant it and fades and darkens the others, much more than the shop's dimming. Both the Inventory and
    /// the Stats pane's list decide "grants" with Effect.GrantsStat, stat bonus rules included.
    /// </summary>
    public class StatFilterTests
    {
        private const string SlotPath = "Assets/Prefabs/UI/Inventory/EffectIconUI.prefab";

        private static UISettings Settings() => AssetDatabase.LoadAssetAtPath<UISettings>("Assets/Resources/UI/UISettings.asset");

        // --- Settings and slot ---------------------------------------------------------------------------

        [Test]
        public void Settings_FadeHardAndDarken()
        {
            var settings = Settings();
            Assert.Less(settings.StatFilterAlpha, 0.35f, "The Shift fade must be stronger than the shop's dimming (0.35)");
            Assert.Greater(settings.StatFilterAlpha, 0f, "Faded items must stay faintly visible");
            Assert.Less(settings.StatFilterTint.maxColorComponent, 1f, "The tint must darken the icon and frame");
            Assert.AreEqual(1f, settings.StatFilterTint.a, "The tint darkens; the fade is the opacity's job");
        }

        [Test]
        public void Slot_HasTheRingAndAPlainIcon()
        {
            var slot = AssetDatabase.LoadAssetAtPath<GameObject>(SlotPath);
            Assert.IsNotNull(slot, $"Missing {SlotPath}");
            var ui = new SerializedObject(slot.GetComponent<EffectIconUI>());
            var mark = ui.FindProperty("selectedMark").objectReferenceValue as GameObject;
            Assert.IsNotNull(mark, "EffectIconUI.selectedMark not set: the items granting the stat get no ring");
            Assert.IsFalse(mark.activeSelf, "The ring starts hidden");

            var icon = ui.FindProperty("icon").objectReferenceValue as Image;
            Assert.AreEqual(Color.white, icon.color, "Clearing the filter restores the icon to white");
        }

        // --- Look ----------------------------------------------------------------------------------------

        [Test]
        public void OtherItems_AreFadedAndDarkened()
        {
            var settings = Settings();
            var quality = new Color(0.4f, 0.6f, 1f, 1f);

            Assert.AreEqual(settings.StatFilterAlpha, EffectIconUI.GetStatFilterAlpha(StatFilter.Other, settings));
            Assert.AreEqual(quality * settings.StatFilterTint, EffectIconUI.GetStatFilterColor(quality, StatFilter.Other, settings));
        }

        [TestCase(StatFilter.None)]
        [TestCase(StatFilter.Grants)]
        public void GrantingItems_AndNoFilter_KeepTheirLook(StatFilter filter)
        {
            var settings = Settings();
            var quality = new Color(0.4f, 0.6f, 1f, 1f);

            Assert.AreEqual(1f, EffectIconUI.GetStatFilterAlpha(filter, settings));
            Assert.AreEqual(quality, EffectIconUI.GetStatFilterColor(quality, filter, settings));
        }

        // --- Which items grant the stat ------------------------------------------------------------------

        [Test]
        public void GrantsStat_ByModifierOrBonusRule()
        {
            var effect = ScriptableObject.CreateInstance<Effect>();
            try
            {
                effect.StatModifiers = new List<StatModifier> { new StatModifier { stat = StatType.Armor, value = 2 } };
                effect.StatDependencies = new List<StatDependencyRule>
                {
                    new StatDependencyRule { sourceStat = StatType.Evasion, targetStat = StatType.Pierce },
                };

                Assert.IsTrue(effect.GrantsStat(StatType.Armor), "A modifier grants its stat");
                Assert.IsTrue(effect.GrantsStat(StatType.Pierce), "A bonus rule grants its target stat (e.g. the Archer Emblem)");
                Assert.IsFalse(effect.GrantsStat(StatType.Evasion), "A bonus rule's source stat isn't granted");
                Assert.IsFalse(effect.GrantsStat(StatType.Luck));
            }
            finally
            {
                Object.DestroyImmediate(effect);
            }
        }
    }
}
