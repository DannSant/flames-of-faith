using Game.UI;
using Game.UI.Style;
using NUnit.Framework;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// Guards the gameplay HUD against drifting from the UI standard (UISettings): every text is a
    /// styled Small/Big text in the standard font, the framed panes use the sliced panel frame, ability
    /// icons are Medium, and the Grace bar is split at its middle. Also the Grace label's colors.
    /// </summary>
    public class HudStyleTests
    {
        private const string MainUI = "Assets/Prefabs/UI/MainUI/";
        private static readonly string[] HudPrefabs =
        {
            MainUI + "HealthPane.prefab", MainUI + "IconsPane.prefab", MainUI + "ExperiencePane.prefab",
            MainUI + "FlameProgressionBar.prefab", MainUI + "WaveHintUI.prefab",
        };

        private static UISettings Settings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<UISettings>("Assets/Resources/UI/UISettings.asset");
            Assert.IsNotNull(settings, "Missing UISettings");
            return settings;
        }

        private static GameObject Load(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, $"Missing {path}");
            return prefab;
        }

        private static RectTransform Child(GameObject root, string name)
        {
            var child = root.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(t => t.name == name);
            Assert.IsNotNull(child, $"{root.name} has no {name}");
            return child;
        }

        [Test]
        public void HudTexts_UseTheStandardSizesAndFonts()
        {
            var settings = Settings();
            foreach (string path in HudPrefabs)
            {
                foreach (var text in Load(path).GetComponentsInChildren<TMP_Text>(true))
                {
                    string label = $"{path} > {text.name}";
                    var style = text.GetComponent<UIStyleText>();
                    Assert.IsNotNull(style, $"{label} has no UIStyleText");
                    Assert.AreEqual(settings.GetFontSize(style.Size), text.fontSize, $"{label} font size");
                    Assert.IsFalse(text.enableAutoSizing, $"{label} auto-sizes");
                    Assert.IsFalse(text.enableVertexGradient, $"{label} has a vertex gradient that tints its color");
                    Assert.AreEqual(style.Outlined ? settings.OutlinedFont : settings.Font, text.font, $"{label} font");
                }
            }
        }

        [Test]
        public void HudTexts_UseOnlyTheTwoStandardSizes()
        {
            var settings = Settings();
            var sizes = HudPrefabs.SelectMany(p => Load(p).GetComponentsInChildren<TMP_Text>(true)).Select(t => t.fontSize).Distinct();
            CollectionAssert.IsSubsetOf(sizes, new[] { settings.SmallFontSize, settings.BigFontSize });
        }

        [TestCase("HealthPane.prefab", "HealthPane")]
        [TestCase("IconsPane.prefab", "Panel")]
        [TestCase("WaveHintUI.prefab", "MainPanel")]
        public void FramedPanes_UseTheSlicedPanelFrame(string prefab, string frameObject)
        {
            var settings = Settings();
            var frame = Child(Load(MainUI + prefab), frameObject);
            var style = frame.GetComponent<UIStyleFrame>();
            Assert.IsNotNull(style, $"{prefab} > {frameObject} has no UIStyleFrame");
            Assert.AreEqual(UIFrameKind.Panel, style.Kind);

            var image = frame.GetComponent<Image>();
            Assert.AreEqual(settings.PanelFrame, image.sprite, $"{prefab} frame sprite");
            Assert.AreEqual(Image.Type.Sliced, image.type, $"{prefab} frame should be 9-sliced");
        }

        [Test]
        public void AbilityIcons_AreMediumWithAMatchingCooldownSweep()
        {
            var settings = Settings();
            var icons = Load(MainUI + "IconsPane.prefab");
            foreach (var (slot, overlay) in new[]
                     {
                         ("MainAttackButton", "AttackBG"), ("SecondaryAttackButton", "SecondaryAttackCooldownBG"), ("DashButton", "DashCooldownBg")
                     })
            {
                var slotRect = Child(icons, slot);
                var style = slotRect.GetComponent<UIStyleIcon>();
                Assert.IsNotNull(style, $"{slot} has no UIStyleIcon");
                Assert.AreEqual(UIIconSize.Medium, style.Size, slot);
                Assert.AreEqual(new Vector2(settings.MediumIconSize, settings.MediumIconSize), slotRect.sizeDelta, $"{slot} size");

                var sweep = slotRect.Find(overlay) as RectTransform;
                Assert.IsNotNull(sweep, $"{slot} has no {overlay}");
                Assert.AreEqual(Vector2.zero, sweep.anchorMin, $"{overlay} should cover the icon");
                Assert.AreEqual(Vector2.one, sweep.anchorMax, $"{overlay} should cover the icon");
                Assert.AreEqual(Vector2.zero, sweep.sizeDelta, $"{overlay} should cover the icon exactly");
                // Unity's built-in UISprite has rounded, see-through edges: the sweep looked smaller than the icon
                var sweepImage = sweep.GetComponent<Image>();
                Assert.AreEqual(settings.SolidFill, sweepImage.sprite, $"{overlay} should use the solid square sprite");
                Assert.AreEqual(Image.Type.Filled, sweepImage.type, $"{overlay} type");
            }
        }

        [Test]
        public void InputGlyphs_FollowTheirSlots()
        {
            var icons = Load(MainUI + "IconsPane.prefab");
            Assert.AreEqual("MainAttackButton", Child(icons, "AttackInputIcon").parent.name);
            Assert.AreEqual("SecondaryAttackButton", Child(icons, "SpecialAttackInputIcon").parent.name);
            Assert.AreEqual("DashButton", Child(icons, "DashInputIcon").parent.name);
            foreach (string glyph in new[] { "AttackInputIcon", "SpecialAttackInputIcon", "DashInputIcon", "AutoAttackInput", "LockonInput" })
            {
                Assert.AreEqual(UIIconSize.Prompt, Child(icons, glyph).GetComponent<UIStyleIcon>()?.Size, $"{glyph} size role");
                Assert.IsTrue(Child(icons, glyph).GetComponent<Image>().preserveAspect, $"{glyph} would squash wide keys like SPACE");
            }
        }

        [Test]
        public void GraceBar_IsSplitAtItsMiddle()
        {
            var health = Load(MainUI + "HealthPane.prefab");
            var negative = Child(health, "FillCorruption");
            var positive = Child(health, "FillGrace");
            Assert.AreEqual(0.5f, negative.anchorMax.x, "Corruption fill should end at the middle");
            Assert.AreEqual(0.5f, positive.anchorMin.x, "Grace fill should start at the middle");
            Assert.AreEqual(Image.OriginHorizontal.Right, (Image.OriginHorizontal)negative.GetComponent<Image>().fillOrigin);
            Assert.AreEqual(Image.OriginHorizontal.Left, (Image.OriginHorizontal)positive.GetComponent<Image>().fillOrigin);
            Assert.IsNull(Child(health, "GraceCorruptionBars").GetComponent<Slider>(), "Leftover disabled Slider");
        }

        [Test]
        public void GraceLabel_IsCyanAtZeroAndAbove_PurpleWhenCorrupted()
        {
            var settings = Settings();
            Assert.AreEqual(settings.GraceColor, GraceBar.GetLabelColor(0f, settings));
            Assert.AreEqual(settings.GraceColor, GraceBar.GetLabelColor(12f, settings));
            Assert.AreEqual(settings.CorruptionColor, GraceBar.GetLabelColor(-0.5f, settings));
            Assert.AreEqual(settings.CorruptionColor, GraceBar.GetLabelColor(-50f, settings));
        }

        [Test]
        public void HudPanes_SitAtTheMarginFromTheScreenEdges()
        {
            float m = Settings().Margin;
            var icons = (RectTransform)Load(MainUI + "IconsPane.prefab").transform;
            Assert.AreEqual(Vector2.zero, icons.anchorMin, "IconsPane: bottom-left");
            Assert.AreEqual(new Vector2(m, m), icons.anchoredPosition, "IconsPane margin");

            var experience = (RectTransform)Load(MainUI + "ExperiencePane.prefab").transform;
            Assert.AreEqual(Vector2.one, experience.anchorMin, "ExperiencePane: top-right");
            Assert.AreEqual(new Vector2(-m, -m), experience.anchoredPosition, "ExperiencePane margin");

            var flame = (RectTransform)Load(MainUI + "FlameProgressionBar.prefab").transform;
            Assert.AreEqual(m, flame.anchoredPosition.y, "Flame bar margin");
        }
    }
}
