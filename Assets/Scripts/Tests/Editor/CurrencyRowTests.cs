using Game.UI;
using Game.UI.Overworld;
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
    /// The Favors next to the coins, with a tooltip on each, in the Inventory and the map window (tester feedback,
    /// 2026-10-26): the formatting, the player-facing words (Pilgrimage, Golden Chronos) and the two prefabs.
    /// </summary>
    public class CurrencyRowTests
    {
        private const string InventoryPath = "Assets/Prefabs/UI/Inventory/InventoryPane.prefab";
        private const string MapWindowPath = "Assets/Prefabs/UI/LevelSelector/LevelSelectorUI.prefab";

        // --- Text ---------------------------------------------------------------------------------------

        [TestCase(42, 0, "42")]
        [TestCase(42, 5, "42 (+5)")]
        [TestCase(0, 3, "0 (+3)")]
        [TestCase(10, -2, "10 (-2)")]
        public void FavorsAmount_ShowsBankedThenThisPilgrimage(int banked, int earned, string expected)
        {
            Assert.AreEqual(expected, CurrencyTooltipTexts.FormatFavorsAmount(banked, earned));
        }

        [Test]
        public void Tooltips_UseThePlayerFacingTerms()
        {
            var texts = new CurrencyTooltipTexts();
            string favors = texts.FormatFavors(42, 5);
            StringAssert.Contains("42", favors);
            StringAssert.Contains("+5", favors);

            foreach (string text in new[] { texts.CoinsTitle, texts.CoinsText, texts.FavorsTitle, favors })
            {
                StringAssert.DoesNotContain(" run", text.ToLowerInvariant(), "Players see 'Pilgrimage', not 'run'");
                StringAssert.DoesNotContain("coin", text.ToLowerInvariant(), "Players see 'Golden Chronos', not 'coins'");
            }
            StringAssert.Contains("Pilgrimage", texts.CoinsText);
            Assert.AreEqual("Golden Chronos", texts.CoinsTitle);
        }

        // --- Prefabs ------------------------------------------------------------------------------------

        private static GameObject Load(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, $"Missing {path}");
            return prefab;
        }

        private static Transform Child(GameObject root, string name)
        {
            var child = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
            Assert.IsNotNull(child, $"{root.name} has no {name}");
            return child;
        }

        private static void AssertWallet(GameObject root, SerializedObject controller)
        {
            var settings = AssetDatabase.LoadAssetAtPath<UISettings>("Assets/Resources/UI/UISettings.asset");
            var wallet = Child(root, "Wallet");
            var favors = Child(root, "Favors");
            var coins = Child(root, "Coins");
            Assert.AreSame(wallet, favors.parent, "Favors sit in the Wallet row");
            Assert.AreSame(wallet, coins.parent, "Coins sit in the Wallet row");
            Assert.Less(favors.GetSiblingIndex(), coins.GetSiblingIndex(), "Favors come before the coins");

            Assert.AreEqual(UIIconSize.Tiny, Child(root, "FavorsIcon").GetComponent<UIStyleIcon>()?.Size, "Favor icon size role");
            var favorsText = Child(root, "FavorsText").GetComponent<TMP_Text>();
            var style = favorsText.GetComponent<UIStyleText>();
            Assert.IsNotNull(style, "FavorsText has no UIStyleText");
            Assert.AreEqual(UIFontSize.Small, style.Size);
            Assert.AreEqual(UIColorRole.Favors, style.ColorRole);

            foreach (var group in new[] { favors, coins })
            {
                Assert.IsNotNull(group.GetComponent<TooltipTriggerUI>(), $"{group.name} has no tooltip");
                var hitArea = group.GetComponent<Image>();
                Assert.IsTrue(hitArea != null && hitArea.raycastTarget, $"{group.name} can't be hovered");
            }

            Assert.AreSame(favorsText, controller.FindProperty("favorsText").objectReferenceValue, "favorsText not wired");
            Assert.AreSame(coins.GetComponent<TooltipTriggerUI>(), controller.FindProperty("coinsTooltip").objectReferenceValue, "coinsTooltip not wired");
            Assert.AreSame(favors.GetComponent<TooltipTriggerUI>(), controller.FindProperty("favorsTooltip").objectReferenceValue, "favorsTooltip not wired");
            Assert.IsNotNull(settings);
        }

        [Test]
        public void Inventory_ShowsFavorsBeforeTheCoins_WithTooltips()
        {
            var root = Load(InventoryPath);
            AssertWallet(root, new SerializedObject(root.GetComponentInChildren<InventoryControllerUI>(true)));
        }

        [Test]
        public void MapWindow_ShowsFavorsBeforeTheCoins_WithTooltips()
        {
            var root = Load(MapWindowPath);
            AssertWallet(root, new SerializedObject(root.GetComponentInChildren<OverworldMapInfoUI>(true)));
        }
    }
}
