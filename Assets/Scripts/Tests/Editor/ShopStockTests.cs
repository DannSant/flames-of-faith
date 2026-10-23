using Game.RunEncounters;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// How many items the shop offers: its base count plus Luck, between a min and a max. Negative Luck used
    /// to shrink the shop to nothing (10 + Luck items, so none at -10 Luck); it now never goes below the min.
    /// </summary>
    public class ShopStockTests
    {
        private const string ShopkeeperPrefab = "Assets/Prefabs/RunEncounters/Shopkeeper.prefab";

        [TestCase(0, 10)]
        [TestCase(4, 14)]
        [TestCase(-3, 7)]
        [TestCase(-7, 3)]
        [TestCase(-8, 3)]
        [TestCase(-50, 3)]
        [TestCase(10, 20)]
        [TestCase(50, 20)]
        public void DisplayCount_IsBasePlusLuck_BetweenMinAndMax(int luck, int expected)
        {
            Assert.AreEqual(expected, ShopKeeper.GetDisplayCount(10, luck, 1, 3, 20));
        }

        [Test]
        public void DisplayCount_ScalesLuckByItsPerPointBonus()
        {
            Assert.AreEqual(16, ShopKeeper.GetDisplayCount(10, 3, 2, 3, 20));
            Assert.AreEqual(4, ShopKeeper.GetDisplayCount(10, -3, 2, 3, 20));
        }

        [Test]
        public void DisplayCount_MinAboveMax_TheMinWins()
        {
            Assert.AreEqual(5, ShopKeeper.GetDisplayCount(10, 0, 1, 5, 2));
        }

        [Test]
        public void Shopkeeper_CountsAreConsistent()
        {
            var shopKeeper = AssetDatabase.LoadAssetAtPath<GameObject>(ShopkeeperPrefab)?.GetComponentInChildren<ShopKeeper>(true);
            Assert.IsNotNull(shopKeeper, $"{ShopkeeperPrefab} has no ShopKeeper");
            var so = new SerializedObject(shopKeeper);
            int min = so.FindProperty("minItemCount").intValue;
            int baseCount = so.FindProperty("itemCount").intValue;
            int max = so.FindProperty("maxItemCount").intValue;

            Assert.GreaterOrEqual(min, 1, "The shop can end up empty at low Luck");
            Assert.LessOrEqual(min, baseCount, "minItemCount is above the base itemCount");
            Assert.LessOrEqual(baseCount, max, "itemCount is above maxItemCount");
        }
    }
}
