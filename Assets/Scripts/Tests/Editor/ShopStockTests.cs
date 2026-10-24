using Game.Loot;
using NUnit.Framework;
using UnityEditor;

namespace Game.Tests
{
    /// <summary>
    /// The shop's size settings on the real LootSettings asset (the count math is in LootRollerTests). Negative Luck
    /// used to shrink the shop to nothing (10 + Luck items, so none at -10 Luck); it never goes below the min.
    /// </summary>
    public class ShopStockTests
    {
        [Test]
        public void ShopCounts_AreConsistent()
        {
            var settings = AssetDatabase.LoadAssetAtPath<LootSettings>("Assets/Resources/Loot/LootSettings.asset");
            Assert.IsNotNull(settings, "Missing LootSettings");
            var so = new SerializedObject(settings);
            int min = so.FindProperty("shopMinItemCount").intValue;
            int baseCount = so.FindProperty("shopItemCount").intValue;
            int max = so.FindProperty("shopMaxItemCount").intValue;

            Assert.GreaterOrEqual(min, 1, "The shop can end up empty at low Luck");
            Assert.LessOrEqual(min, baseCount, "Shop Min Item Count is above Shop Item Count");
            Assert.LessOrEqual(baseCount, max, "Shop Item Count is above Shop Max Item Count");
            Assert.LessOrEqual(max, 20, "The shop window fits 20 items (ShopStyleTests)");
        }
    }
}
