using Game.Feel;
using Game.Scene;
using Game.UI;
using Game.UI.Style.Editor;
using NUnit.Framework;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// The low-health warning: the pure rules (thresholds, hysteresis, intensity, pulse), the settings
    /// asset's values, the placeholder art (clear middle) and the vignette's place in UIMain.
    /// </summary>
    public class LowHealthWarningTests
    {
        private const float Enter = 0.30f;
        private const float Exit = 0.35f;
        private const float FullAt = 0.10f;
        private const float MinIntensity = 0.35f;
        private const string SettingsPath = "Assets/Resources/Feel/GameFeelSettings.asset";
        private const string UIMainScene = "Assets/Scenes/UIMain.unity";

        // --- Rules -----------------------------------------------------------------------------------------

        [Test]
        public void TurnsOn_AtOrBelowTheEnterThreshold()
        {
            Assert.IsTrue(LowHealthRules.IsActive(false, 0.30f, Enter, Exit));
            Assert.IsTrue(LowHealthRules.IsActive(false, 0.05f, Enter, Exit));
            Assert.IsFalse(LowHealthRules.IsActive(false, 0.31f, Enter, Exit));
        }

        [Test]
        public void StaysOn_UntilAboveTheExitThreshold()
        {
            // A small heal between the two thresholds doesn't turn it off...
            Assert.IsTrue(LowHealthRules.IsActive(true, 0.33f, Enter, Exit));
            Assert.IsTrue(LowHealthRules.IsActive(true, 0.35f, Enter, Exit));
            // ...and doesn't turn it on either
            Assert.IsFalse(LowHealthRules.IsActive(false, 0.33f, Enter, Exit));
            Assert.IsFalse(LowHealthRules.IsActive(true, 0.36f, Enter, Exit));
        }

        [Test]
        public void NeverOn_AtZeroHealth()
        {
            Assert.IsFalse(LowHealthRules.IsActive(false, 0f, Enter, Exit));
            Assert.IsFalse(LowHealthRules.IsActive(true, 0f, Enter, Exit));
        }

        [Test]
        public void ZeroMaxHealth_CountsAsNoHealth()
        {
            Assert.AreEqual(0f, LowHealthRules.HealthFraction(5f, 0f));
            Assert.AreEqual(0f, LowHealthRules.HealthFraction(0f, 0f));
            Assert.IsFalse(LowHealthRules.IsActive(true, LowHealthRules.HealthFraction(5f, 0f), Enter, Exit));
        }

        [Test]
        public void HealthFraction_IsCurrentOverMax()
        {
            Assert.AreEqual(0.25f, LowHealthRules.HealthFraction(5f, 20f), 1e-5f);
            Assert.AreEqual(1f, LowHealthRules.HealthFraction(30f, 20f), 1e-5f);
        }

        [Test]
        public void Intensity_RisesFromMinAtTheThresholdToOneAtFull()
        {
            Assert.AreEqual(MinIntensity, LowHealthRules.Intensity(Enter, Enter, FullAt, MinIntensity), 1e-5f);
            Assert.AreEqual(1f, LowHealthRules.Intensity(FullAt, Enter, FullAt, MinIntensity), 1e-5f);
            Assert.AreEqual(1f, LowHealthRules.Intensity(0.02f, Enter, FullAt, MinIntensity), 1e-5f);
            // Between the thresholds (held on by hysteresis) it stays at the minimum
            Assert.AreEqual(MinIntensity, LowHealthRules.Intensity(0.33f, Enter, FullAt, MinIntensity), 1e-5f);

            float middle = LowHealthRules.Intensity(0.20f, Enter, FullAt, MinIntensity);
            Assert.Greater(middle, MinIntensity);
            Assert.Less(middle, 1f);
        }

        [Test]
        public void PulseRate_IsFasterWhenStronger()
        {
            Assert.AreEqual(0.8f, LowHealthRules.PulseRate(0f, 0.8f, 1.6f), 1e-5f);
            Assert.AreEqual(1.6f, LowHealthRules.PulseRate(1f, 0.8f, 1.6f), 1e-5f);
            Assert.Greater(LowHealthRules.PulseRate(0.7f, 0.8f, 1.6f), LowHealthRules.PulseRate(0.4f, 0.8f, 1.6f));
        }

        [Test]
        public void PulseAlpha_SwingsBetweenTheFloorAndThePeak()
        {
            Assert.AreEqual(0.8f, LowHealthRules.PulseAlpha(0f, 1f, 0.8f, 0.5f, 1f), 1e-5f);
            Assert.AreEqual(0.4f, LowHealthRules.PulseAlpha(0.5f, 1f, 0.8f, 0.5f, 1f), 1e-5f);
            Assert.AreEqual(0.4f, LowHealthRules.PulseAlpha(0f, 0.5f, 0.8f, 0.5f, 1f), 1e-5f);
            // Fading out takes the whole pulse down with it
            Assert.AreEqual(0f, LowHealthRules.PulseAlpha(0f, 1f, 0.8f, 0.5f, 0f), 1e-5f);
        }

        [TestCase(LevelType.Combat, true)]
        [TestCase(LevelType.Boss, true)]
        [TestCase(LevelType.Store, false)]
        [TestCase(LevelType.Rest, false)]
        [TestCase(LevelType.Event, false)]
        [TestCase(LevelType.Treasure, false)]
        [TestCase(LevelType.CorruptionClean, false)]
        public void ShowsOnlyInFights(LevelType type, bool shows)
        {
            Assert.AreEqual(shows, LowHealthRules.ShowsInLevel(type));
        }

        // --- Settings asset --------------------------------------------------------------------------------

        [Test]
        public void SettingsAsset_HasSaneWarningValues()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GameFeelSettings>(SettingsPath);
            Assert.IsNotNull(settings, $"Missing {SettingsPath}");
            Assert.AreSame(settings, GameFeelSettings.Instance, "GameFeelSettings.Instance doesn't load the asset");

            Assert.Greater(settings.LowHealthEnter, 0f);
            Assert.Less(settings.LowHealthEnter, 1f);
            Assert.GreaterOrEqual(settings.LowHealthExit, settings.LowHealthEnter, "Exit below enter: the warning would flicker");
            Assert.Less(settings.FullIntensityAt, settings.LowHealthEnter);
            Assert.That(settings.MinIntensity, Is.InRange(0f, 1f));
            Assert.Greater(settings.PulseRateMin, 0f);
            Assert.GreaterOrEqual(settings.PulseRateMax, settings.PulseRateMin);
            Assert.Less(settings.PulseRateMax, 2f, "Two pulses a second or more reads as flashing");
            Assert.That(settings.VignetteMaxAlpha, Is.InRange(0.05f, 1f));
            Assert.Greater(settings.VignetteFadeSpeed, 0f);
        }

        // --- Art -------------------------------------------------------------------------------------------

        [Test]
        public void VignetteArt_IsClearInTheMiddleAndRedAtTheEdges()
        {
            var texture = new Texture2D(2, 2);
            Assert.IsTrue(texture.LoadImage(File.ReadAllBytes(HudRestyler.LowHealthVignettePath)), "Can't read the vignette PNG");
            try
            {
                // The player has to see what's killing them: the whole middle third is fully transparent
                for (int x = texture.width / 3; x < texture.width * 2 / 3; x += 8)
                {
                    for (int y = texture.height / 3; y < texture.height * 2 / 3; y += 8)
                    {
                        Assert.AreEqual(0f, texture.GetPixel(x, y).a, 0.001f, $"Vignette not clear at {x},{y}");
                    }
                }
                Assert.Greater(texture.GetPixel(0, 0).a, 0.9f, "Vignette corner not opaque");
                Assert.Greater(texture.GetPixel(0, 0).r, texture.GetPixel(0, 0).g, "Vignette corner not red");
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(HudRestyler.LowHealthVignettePath);
            Assert.AreEqual(TextureImporterType.Sprite, importer.textureType);
        }

        // --- UIMain ----------------------------------------------------------------------------------------

        [Test]
        public void Vignette_IsStretchedFirstAndTakesNoClicks()
        {
            var scene = EditorSceneManager.OpenPreviewScene(UIMainScene);
            try
            {
                var warning = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<LowHealthVignetteUI>(true))
                    .ToList();
                Assert.AreEqual(1, warning.Count, "UIMain needs exactly one LowHealthVignetteUI (run Restyle HUD)");

                var rect = (RectTransform)warning[0].transform;
                Assert.IsNotNull(rect.parent.GetComponent<Canvas>(), "The vignette must sit directly on UIMain's canvas");
                Assert.AreEqual(0, rect.GetSiblingIndex(), "The vignette must be drawn first, under the HUD and every window");
                Assert.AreEqual(Vector2.zero, rect.anchorMin);
                Assert.AreEqual(Vector2.one, rect.anchorMax);
                Assert.AreEqual(Vector2.zero, rect.offsetMin);
                Assert.AreEqual(Vector2.zero, rect.offsetMax);

                var image = warning[0].GetComponent<Image>();
                Assert.IsFalse(image.raycastTarget, "The vignette must never take a click");
                Assert.AreEqual(HudRestyler.LowHealthVignettePath, AssetDatabase.GetAssetPath(image.sprite));
                Assert.IsEmpty(warning[0].GetComponentsInChildren<Selectable>(true));
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}
