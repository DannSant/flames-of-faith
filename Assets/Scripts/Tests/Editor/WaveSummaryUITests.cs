using Game.UI;
using Game.UI.InputPrompts;
using NUnit.Framework;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// The end-of-wave summary (WaveCorruptionSummaryUI prefab): Favors read as blue, not as more golden
    /// Grace; the final Grace and the Favors are the biggest rows; keyboard/mouse is told to click.
    /// </summary>
    public class WaveSummaryUITests
    {
        private const string SummaryPrefab = "Assets/Prefabs/UI/Waves/WaveCorruptionSummaryUI .prefab";
        private const string MouseLeftSprite = "Assets/Sprites/UI/KeyboardAndMouseIcons/mouse_left.png";

        private static WaveCorruptionSummaryUI LoadSummary()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SummaryPrefab);
            Assert.IsNotNull(prefab, $"Missing prefab {SummaryPrefab}");
            var summary = prefab.GetComponentInChildren<WaveCorruptionSummaryUI>(true);
            Assert.IsNotNull(summary, "The prefab has no WaveCorruptionSummaryUI");
            return summary;
        }

        private static SerializedProperty Property(Object owner, string field)
        {
            var property = new SerializedObject(owner).FindProperty(field);
            Assert.IsNotNull(property, $"{owner.name} has no field {field}");
            return property;
        }

        private static TextMeshProUGUI Text(Object owner, string field)
        {
            var text = Property(owner, field).objectReferenceValue as TextMeshProUGUI;
            Assert.IsNotNull(text, $"{owner.name}.{field} is not set");
            return text;
        }

        [Test]
        public void Favors_AreBlue_NotGraceGold()
        {
            var summary = LoadSummary();
            Color favors = Property(summary, "favorsColor").colorValue;
            Color grace = Property(summary, "positiveColor").colorValue;

            Assert.Greater(favors.b, favors.r, "Favors color should be blue");
            Assert.Greater(favors.b, favors.g, "Favors color should be blue");
            Assert.Greater(Mathf.Abs(favors.b - grace.b) + Mathf.Abs(favors.r - grace.r), 0.3f,
                "Favors must not look like the positive Grace color");
        }

        [Test]
        public void FinalGraceAndFavors_AreTheBiggestRows()
        {
            var summary = LoadSummary();
            float formula = Text(summary, "formulaText").fontSize;
            float favors = Text(summary, "favorsRowText").fontSize;
            string[] otherRows = { "corruptorRowText", "corruptedDamageRowText", "graceAffinityRowText", "totalCorruptionRowText" };

            foreach (string row in otherRows)
            {
                float size = Text(summary, row).fontSize;
                Assert.Greater(formula, size, $"The final Grace should be bigger than {row}");
                Assert.Greater(favors, size, $"The Favors should be bigger than {row}");
            }
        }

        [Test]
        public void ContinuePrompt_ShowsTheMouseAndSaysClickOnKeyboardMouse()
        {
            var prompt = Property(LoadSummary(), "continuePrompt").objectReferenceValue as GameObject;
            Assert.IsNotNull(prompt, "continuePrompt is not set");

            var mouseLeft = AssetDatabase.LoadAssetAtPath<Sprite>(MouseLeftSprite);
            Assert.IsNotNull(mouseLeft, $"Missing sprite {MouseLeftSprite}");

            // The icon shown on keyboard/mouse is the one an InputPromptVisibilityUI shows only there.
            var keyboardMouseIcons = prompt.GetComponentsInChildren<InputPromptVisibilityUI>(true)
                .Select(v => new SerializedObject(v))
                .Where(s => s.FindProperty("showOnKeyboardMouse").boolValue && !s.FindProperty("showOnGamepad").boolValue)
                .SelectMany(s => Enumerable.Range(0, s.FindProperty("targets").arraySize)
                    .Select(i => s.FindProperty("targets").GetArrayElementAtIndex(i).objectReferenceValue as GameObject))
                .Where(go => go != null)
                .Select(go => go.GetComponent<Image>())
                .ToList();
            Assert.IsNotEmpty(keyboardMouseIcons, "The continue prompt has no keyboard/mouse-only icon");
            Assert.IsTrue(keyboardMouseIcons.All(i => i != null && i.sprite == mouseLeft),
                "The keyboard/mouse continue icon should be the left mouse button");

            var schemeText = prompt.GetComponentInChildren<InputSchemeTextUI>(true);
            Assert.IsNotNull(schemeText, "The continue prompt has no InputSchemeTextUI (\"Click\" / \"Press\")");
            StringAssert.Contains("Click", Property(schemeText, "keyboardMouseText").stringValue);
        }
    }
}
