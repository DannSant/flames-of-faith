using Game.Effects;
using Game.Items.Editor;
using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The New Item tool: a new item is named after itself (EffectAssetTests), gets its own effectID (saves and
    /// unlocks find items by it) and can't overwrite an existing item.
    /// </summary>
    public class NewItemToolTests
    {
        private const string Folder = "Assets/__NewItemToolTests";

        [TearDown]
        public void TearDown() => AssetDatabase.DeleteAsset(Folder);

        [Test]
        public void Create_MakesANamedItemWithAFreshId()
        {
            var effect = NewItemTool.Create("Test Ring", EffectQuality.Rare, Folder);

            Assert.AreEqual("Test Ring", effect.name);
            Assert.AreEqual("Test Ring", effect.EffectName);
            Assert.AreEqual(EffectQuality.Rare, effect.Quality);
            Assert.IsTrue(effect.UnlockedByDefault);
            Assert.IsFalse(string.IsNullOrEmpty(effect.EffectID));
            var existingIds = Resources.LoadAll<Effect>("Effects").Select(e => e.EffectID);
            CollectionAssert.DoesNotContain(existingIds, effect.EffectID);
        }

        [Test]
        public void Validate_RefusesDuplicatesAndBadNames()
        {
            NewItemTool.Create("Test Ring", EffectQuality.Common, Folder);
            Assert.IsNotNull(NewItemTool.Validate("Test Ring", Folder));
            Assert.IsNotNull(NewItemTool.Validate("", Folder));
            Assert.IsNotNull(NewItemTool.Validate(" Padded", Folder));
            Assert.IsNotNull(NewItemTool.Validate("Bad/Name", Folder));
            Assert.IsNull(NewItemTool.Validate("Another Ring", Folder));
        }
    }
}
