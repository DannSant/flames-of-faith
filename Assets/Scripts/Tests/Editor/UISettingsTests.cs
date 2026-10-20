using Game.UI.Navigation;
using Game.UI.Style;
using NUnit.Framework;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The UI standard (UISettings): the real asset holds the agreed sizes and colors, its frame art is
    /// 9-sliced, the screen-space canvases scale from its reference resolution, and the gamepad
    /// selection frame wraps elements correctly.
    /// </summary>
    public class UISettingsTests
    {
        private const string SettingsPath = "Assets/Resources/UI/UISettings.asset";
        private const string MarkerPrefab = "Assets/Prefabs/UI/Misc/UISelectionMarkerUI.prefab";

        private static UISettings LoadSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<UISettings>(SettingsPath);
            Assert.IsNotNull(settings, $"Missing {SettingsPath}");
            return settings;
        }

        [Test]
        public void Settings_LoadFromResources()
        {
            Assert.AreSame(LoadSettings(), Resources.Load<UISettings>("UI/UISettings"));
        }

        [Test]
        public void Settings_MatchTheAgreedStandard()
        {
            var settings = LoadSettings();
            Assert.AreEqual(12f, settings.SmallFontSize, "Small font");
            Assert.AreEqual(20f, settings.BigFontSize, "Big font");
            Assert.AreEqual(50f, settings.SmallIconSize, "Small icon");
            Assert.AreEqual(80f, settings.MediumIconSize, "Medium icon");
            Assert.AreEqual(15f, settings.Margin, "Margin");
            Assert.AreEqual(new Vector2(1920f, 1080f), settings.ReferenceResolution, "Reference resolution");
        }

        [Test]
        public void Settings_SizesAreOrdered()
        {
            var settings = LoadSettings();
            Assert.Less(settings.SmallFontSize, settings.BigFontSize);
            Assert.Less(settings.SmallIconSize, settings.MediumIconSize);
            Assert.Less(settings.GetButtonHeight(UIButtonSize.Small), settings.GetButtonHeight(UIButtonSize.Big));
            Assert.AreEqual(UIFontSize.Small, settings.GetButtonFontSize(UIButtonSize.Small));
            Assert.AreEqual(UIFontSize.Big, settings.GetButtonFontSize(UIButtonSize.Big));
        }

        [Test]
        public void Settings_SharedColorsAreDistinctAndReadAsTheirMeaning()
        {
            var settings = LoadSettings();
            Color grace = settings.GraceColor;
            Color corruption = settings.CorruptionColor;
            Color favors = settings.FavorsColor;

            Assert.AreNotEqual(grace, corruption);
            Assert.AreNotEqual(grace, favors);
            Assert.AreNotEqual(corruption, favors);

            // Cyan: blue and green high, red low
            Assert.Greater(grace.g, 0.8f, "Grace should be cyan");
            Assert.Greater(grace.b, 0.8f, "Grace should be cyan");
            Assert.Less(grace.r, 0.6f, "Grace should be cyan");
            // Purple: blue and red above green
            Assert.Greater(corruption.b, corruption.g, "Corruption should be purple");
            Assert.Greater(corruption.r, corruption.g, "Corruption should be purple");
            // Gold: red and green above blue
            Assert.Greater(favors.r, favors.b, "Favors should be gold");
            Assert.Greater(favors.g, favors.b, "Favors should be gold");

            Assert.AreEqual(grace, settings.GetColor(UIColorRole.Grace));
            Assert.AreEqual(corruption, settings.GetColor(UIColorRole.Corruption));
            Assert.AreEqual(favors, settings.GetColor(UIColorRole.Favors));
        }

        [Test]
        public void Settings_FontsAreSet()
        {
            var settings = LoadSettings();
            Assert.IsNotNull(settings.Font, "Font");
            Assert.IsNotNull(settings.OutlinedFont, "Outlined font");
        }

        [Test]
        public void Settings_FrameArtIsNineSliced()
        {
            var settings = LoadSettings();
            AssertSliced(settings.PanelFrame, "Panel frame");
            AssertSliced(settings.BarFrame, "Bar frame");
            AssertSliced(settings.SelectionFrame, "Selection frame");
            Assert.IsNotNull(settings.BarFill, "Bar fill");
            Assert.IsNotNull(settings.SolidFill, "Solid fill");
        }

        private static void AssertSliced(Sprite sprite, string label)
        {
            Assert.IsNotNull(sprite, $"{label} is not set");
            Assert.Greater(sprite.border.sqrMagnitude, 0f, $"{label} ({sprite.name}) has no 9-slice border");
        }

        [Test]
        public void ScreenSpaceCanvases_ScaleWithTheScreen()
        {
            var settings = LoadSettings();
            foreach (string scene in new[]
                     {
                         "Assets/Scenes/Bootstrapper.unity", "Assets/Scenes/MainMenu.unity",
                         "Assets/Scenes/UIMain.unity", "Assets/Scenes/LevelSelector.unity"
                     })
            {
                string yaml = File.ReadAllText(scene);
                Assert.IsTrue(yaml.Contains("m_UiScaleMode: 1"), $"{scene}: the canvas should Scale With Screen Size");
                Assert.IsFalse(yaml.Contains("m_UiScaleMode: 0"), $"{scene}: a canvas still uses Constant Pixel Size");

                var resolution = Regex.Match(yaml, @"m_ReferenceResolution: \{x: ([\d.]+), y: ([\d.]+)\}");
                Assert.IsTrue(resolution.Success, $"{scene}: no reference resolution");
                Assert.AreEqual(settings.ReferenceResolution.x, float.Parse(resolution.Groups[1].Value), scene);
                Assert.AreEqual(settings.ReferenceResolution.y, float.Parse(resolution.Groups[2].Value), scene);
            }
        }

        [Test]
        public void SelectionMarker_IsAWrappingFrameDrawnOnTop()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefab);
            Assert.IsNotNull(prefab, $"Missing {MarkerPrefab}");

            var canvas = prefab.GetComponent<Canvas>();
            Assert.IsNotNull(canvas, "The marker needs its own Canvas to draw above every window");
            // Read serialized: the property getter reports false for a canvas with no parent canvas
            var serializedCanvas = new SerializedObject(canvas);
            Assert.IsTrue(serializedCanvas.FindProperty("m_OverrideSorting").boolValue, "The marker's Canvas should override sorting");
            Assert.Greater(serializedCanvas.FindProperty("m_SortingOrder").intValue, 0, "The marker should sort above the windows");

            var marker = new SerializedObject(prefab.GetComponent<UISelectionMarkerUI>());
            var frame = marker.FindProperty("markerImage").objectReferenceValue as UnityEngine.UI.Image;
            Assert.IsNotNull(frame, "markerImage is not set");
            Assert.AreEqual(UnityEngine.UI.Image.Type.Sliced, frame.type);
            Assert.IsFalse(frame.raycastTarget, "The frame must not block clicks");
            Assert.AreEqual(Quaternion.identity, frame.rectTransform.localRotation, "The old arrow was rotated");
        }

        [Test]
        public void SelectionMarker_ExistsInEveryNavigableScene()
        {
            string guid = AssetDatabase.AssetPathToGUID(MarkerPrefab);
            foreach (string scene in new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/UIMain.unity", "Assets/Scenes/LevelSelector.unity" })
            {
                Assert.IsTrue(File.ReadAllText(scene).Contains(guid), $"{scene} has no selection marker");
            }
        }

        [Test]
        public void SelectionFrame_WrapsTheCornersWithPadding()
        {
            var corners = new[] { new Vector2(10f, 20f), new Vector2(10f, 60f), new Vector2(110f, 60f), new Vector2(110f, 20f) };
            Rect rect = SelectionFrameMath.ComputeRect(corners, new Vector2(10f, 6f));

            Assert.AreEqual(new Vector2(60f, 40f), rect.center);
            Assert.AreEqual(new Vector2(110f, 46f), rect.size);
        }

        [Test]
        public void SelectionFrame_CornerOrderDoesNotMatter()
        {
            var corners = new[] { new Vector2(110f, 60f), new Vector2(10f, 20f), new Vector2(110f, 20f), new Vector2(10f, 60f) };
            Rect rect = SelectionFrameMath.ComputeRect(corners, Vector2.zero);

            Assert.AreEqual(new Vector2(60f, 40f), rect.center);
            Assert.AreEqual(new Vector2(100f, 40f), rect.size);
        }

        [Test]
        public void SelectionFrame_PulseStaysBetweenMinAndOne()
        {
            Assert.AreEqual(1f, SelectionFrameMath.PulseAlpha(0.3f, 0f, 0.2f), "Speed 0 = no pulse");
            for (float t = 0f; t < 2f; t += 0.05f)
            {
                float alpha = SelectionFrameMath.PulseAlpha(t, 1.5f, 0.6f);
                Assert.GreaterOrEqual(alpha, 0.6f - 1e-4f);
                Assert.LessOrEqual(alpha, 1f + 1e-4f);
            }
            Assert.AreEqual(1f, SelectionFrameMath.PulseAlpha(0f, 1.5f, 0.6f), 1e-4f, "Starts fully visible");
        }
    }
}
