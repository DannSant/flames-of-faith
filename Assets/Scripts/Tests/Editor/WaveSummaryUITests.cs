using Game.UI;
using Game.UI.Navigation;
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
    /// The end-of-wave summary (WaveCorruptionSummaryUI prefab), restyled as a center window (its shared layout
    /// is in CenterWindowStyleTests): Small receipt rows, the Big formula and gold Favors, the Grace bar built like
    /// the HUD's, and a Continue button instead of the old "click / press A" prompt. Plus the pure formatting and
    /// the flight path the flying numbers use.
    /// </summary>
    public class WaveSummaryUITests
    {
        private const string SummaryPrefab = "Assets/Prefabs/UI/Waves/WaveCorruptionSummaryUI.prefab";

        private static UISettings Settings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<UISettings>("Assets/Resources/UI/UISettings.asset");
            Assert.IsNotNull(settings, "Missing UISettings");
            return settings;
        }

        private static WaveCorruptionSummaryUI LoadSummary()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SummaryPrefab);
            Assert.IsNotNull(prefab, $"Missing prefab {SummaryPrefab}");
            var summary = prefab.GetComponentInChildren<WaveCorruptionSummaryUI>(true);
            Assert.IsNotNull(summary, "The prefab has no WaveCorruptionSummaryUI");
            return summary;
        }

        private static T Ref<T>(Object owner, string field) where T : Object
        {
            var property = new SerializedObject(owner).FindProperty(field);
            Assert.IsNotNull(property, $"{owner.name} has no field {field}");
            var value = property.objectReferenceValue as T;
            Assert.IsNotNull(value, $"{owner.name}.{field} is not set");
            return value;
        }

        private static readonly string[] RowsInRevealOrder = { "corruptorRowText", "corruptedDamageRowText", "totalCorruptionRowText", "graceAffinityRowText" };

        [Test]
        public void SourceRows_AreSmall_ResultRowsAreBig()
        {
            var settings = Settings();
            var summary = LoadSummary();
            foreach (string row in RowsInRevealOrder)
            {
                var text = Ref<TextMeshProUGUI>(summary, row);
                Assert.AreEqual(UIFontSize.Small, text.GetComponent<UIStyleText>()?.Size, $"{row} size role");
                Assert.AreEqual(settings.SmallFontSize, text.fontSize, row);
            }
            foreach (string row in new[] { "formulaText", "favorsRowText" })
            {
                var text = Ref<TextMeshProUGUI>(summary, row);
                Assert.AreEqual(UIFontSize.Big, text.GetComponent<UIStyleText>()?.Size, $"{row} size role");
                Assert.AreEqual(settings.BigFontSize, text.fontSize, row);
            }
        }

        [Test]
        public void Favors_AreTheStandardGold()
        {
            var settings = Settings();
            var favors = Ref<TextMeshProUGUI>(LoadSummary(), "favorsRowText");
            Assert.AreEqual(UIColorRole.Favors, favors.GetComponent<UIStyleText>()?.ColorRole);
            Assert.AreEqual((Vector3)(Vector4)settings.FavorsColor, (Vector3)(Vector4)favors.color, "Baked Favors color");
        }

        [Test]
        public void Rows_SitInAColumnInRevealOrder()
        {
            var summary = LoadSummary();
            var content = Ref<TextMeshProUGUI>(summary, RowsInRevealOrder[0]).transform.parent;
            Assert.AreEqual("Content", content.name);
            Assert.IsNotNull(content.GetComponent<VerticalLayoutGroup>(), "Hidden rows collapse in a column");

            var order = RowsInRevealOrder.Concat(new[] { "formulaText" })
                .Select(f => Ref<TextMeshProUGUI>(summary, f).transform)
                .ToList();
            Assert.IsTrue(order.All(t => t.parent == content), "Every row is in the column");
            for (int i = 1; i < order.Count; i++)
            {
                Assert.Greater(order[i].GetSiblingIndex(), order[i - 1].GetSiblingIndex(), $"{order[i].name} comes after {order[i - 1].name}");
            }
            Assert.Greater(Ref<Image>(summary, "graceMeterPositive").transform.parent.GetSiblingIndex(),
                Ref<TextMeshProUGUI>(summary, "formulaText").transform.GetSiblingIndex(), "The Grace bar is under the formula");
        }

        [Test]
        public void Continue_IsAButtonNotAPrompt()
        {
            var summary = LoadSummary();
            var root = summary.gameObject;
            Assert.IsFalse(root.GetComponentsInChildren<Transform>(true).Any(t => t.name == "InputPromptRow"), "Leftover click / press A prompt");
            Assert.IsNull(new SerializedObject(summary).FindProperty("continuePrompt"), "Leftover continuePrompt field");

            var button = Ref<Button>(summary, "continueButton");
            Assert.AreEqual(0, button.onClick.GetPersistentEventCount(), "Wired in code (Skip, then Continue)");
            Assert.AreSame(button.GetComponentInChildren<TextMeshProUGUI>(true), Ref<TextMeshProUGUI>(summary, "continueLabel"));

            var window = root.GetComponentInChildren<UIWindow>(true);
            Assert.IsNotNull(window);
            Assert.AreSame(button, new SerializedObject(window).FindProperty("defaultSelectable").objectReferenceValue, "The gamepad starts on the button");
        }

        [Test]
        public void GraceMeter_IsSplitAtZeroLikeTheHudBar()
        {
            var settings = Settings();
            var summary = LoadSummary();
            var positive = Ref<Image>(summary, "graceMeterPositive");
            var negative = Ref<Image>(summary, "graceMeterNegative");

            foreach (var fill in new[] { positive, negative })
            {
                Assert.AreEqual(Image.Type.Filled, fill.type, fill.name);
                Assert.AreEqual(Image.FillMethod.Horizontal, fill.fillMethod, fill.name);
            }
            Assert.AreEqual((int)Image.OriginHorizontal.Left, positive.fillOrigin, "Grace grows right from the middle");
            Assert.AreEqual((int)Image.OriginHorizontal.Right, negative.fillOrigin, "Corrupted grows left from the middle");
            Assert.AreEqual(settings.GraceColor, positive.color);
            Assert.AreEqual(settings.CorruptionColor, negative.color);
            Assert.AreEqual(0.5f, positive.rectTransform.anchorMin.x, "Split at the middle");
            Assert.AreEqual(0.5f, negative.rectTransform.anchorMax.x, "Split at the middle");
            Assert.IsNotNull(positive.transform.parent.Find("Tick"), "A tick marks 0");
        }

        [Test]
        public void OnlyTheRowsThatApplyAreShown()
        {
            // Reported: Corruptor killed fast, no damage taken, no Grace Affinity - yet the damage and Affinity rows
            // showed the prefab's sample texts ("Corrupted damage taken: 40 -3", "Grace Affinity +1")
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(LoadSummary().gameObject);
            try
            {
                var summary = instance.GetComponent<WaveCorruptionSummaryUI>();
                summary.ShowFinalState(new Game.Waves.WaveCorruptionResult
                {
                    corruptor = new Game.Waves.CorruptorResult { spawned = true, killed = true, killTime = 13.2f, corruption = 1 },
                    totalCorruption = 1,
                    graceBefore = 8f,
                    graceAfter = 7f,
                    graceMax = 50f,
                    favorsEarned = 0,
                });

                Assert.IsTrue(Ref<TextMeshProUGUI>(summary, "corruptorRowText").gameObject.activeSelf, "Corruptor row");
                StringAssert.Contains("Corruptor killed in 13.2s", Ref<TextMeshProUGUI>(summary, "corruptorRowText").text);
                Assert.IsTrue(Ref<TextMeshProUGUI>(summary, "totalCorruptionRowText").gameObject.activeSelf, "Total row");
                Assert.IsFalse(Ref<TextMeshProUGUI>(summary, "corruptedDamageRowText").gameObject.activeSelf, "No damage taken: no damage row");
                Assert.IsFalse(Ref<TextMeshProUGUI>(summary, "graceAffinityRowText").gameObject.activeSelf, "No Grace Affinity: no Affinity row");
                Assert.IsFalse(Ref<TextMeshProUGUI>(summary, "favorsRowText").gameObject.activeSelf, "No Favors: no Favors row");
                StringAssert.StartsWith("Grace +8 ", Ref<TextMeshProUGUI>(summary, "formulaText").text);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void UIMainInstance_HasNoLayoutOverridesOnTheWindowsChildren()
        {
            // The old layout group left the children's anchors as scene overrides; one pinned the title to the bottom
            SceneInstanceLayout.AssertNoChildLayoutOverrides("Assets/Scenes/UIMain.unity", SummaryPrefab);
        }

        // --- Formatting ---------------------------------------------------------------------------------------

        [Test]
        public void Row_PutsTheValueOnTheRight()
        {
            Assert.AreEqual("Grace Affinity<line-height=0>\n<align=right><color=#00FFFF>+1</color>",
                WaveCorruptionSummaryUI.FormatRow("Grace Affinity", "+1", "00FFFF"));
        }

        [TestCase(3, "+3")]
        [TestCase(-5, "-5")]
        [TestCase(0, "0")]
        public void Delta_IsSigned(int value, string expected)
        {
            Assert.AreEqual(expected, WaveCorruptionSummaryUI.FormatDelta(value));
        }

        [Test]
        public void FormulaLeft_ColorsAffinityAndCorruption()
        {
            Assert.AreEqual("Grace +5 <color=#0FF>+ 1</color> <color=#F0F>- 6</color>",
                WaveCorruptionSummaryUI.FormatFormulaLeft(5f, 1f, 6, "0FF", "F0F"));
            Assert.AreEqual("Grace -2 <color=#F0F>- 3</color>", WaveCorruptionSummaryUI.FormatFormulaLeft(-2f, 0f, 3, "0FF", "F0F"));
            Assert.AreEqual("Grace 0 <color=#0FF>+ 2</color>", WaveCorruptionSummaryUI.FormatFormulaLeft(0f, 2f, 0, "0FF", "F0F"));
        }

        [TestCase(1, "Favor earned +1")]
        [TestCase(3, "Favors earned +3")]
        public void Favors_Line(int favors, string expected)
        {
            Assert.AreEqual(expected, WaveCorruptionSummaryUI.FormatFavors(favors));
        }

        [TestCase(25f, 50f, 0.5f, 0f)]
        [TestCase(-10f, 50f, 0f, 0.2f)]
        [TestCase(80f, 50f, 1f, 0f)]
        [TestCase(-80f, 50f, 0f, 1f)]
        [TestCase(10f, 0f, 0f, 0f)]
        public void GraceBarFills_SplitAtZero(float grace, float max, float positive, float negative)
        {
            Assert.AreEqual(new Vector2(positive, negative), GraceBar.GetFills(grace, max));
        }

        [Test]
        public void Flight_StartsAndEndsOnItsPointsAndArcsUp()
        {
            var from = new Vector3(0f, 0f, 0f);
            var to = new Vector3(100f, 0f, 0f);
            Assert.AreEqual(from, UIFlight.Evaluate(from, to, 0f, 40f));
            Assert.AreEqual(to, UIFlight.Evaluate(from, to, 1f, 40f));
            Assert.AreEqual(to, UIFlight.Evaluate(from, to, 3f, 40f), "Clamped past the end");

            Vector3 middle = UIFlight.Evaluate(from, to, 0.5f, 40f);
            Assert.Greater(middle.y, 0f, "Arcs up along the way");
            Assert.Greater(middle.x, 50f, "Eased out: past halfway at half time");
        }
    }
}
