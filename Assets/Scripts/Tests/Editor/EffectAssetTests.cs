using Game.Effects;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Game.Tests
{
    /// <summary>
    /// The generated Effect assets (Tools/Effects/Generate ScriptableObjects writes each row to
    /// "Resources/Effects/&lt;name&gt;.asset" and never deletes). Renaming a row without renaming its
    /// asset leaves the old asset behind: two assets with the same EffectID, and references still
    /// pointing at the stale one.
    /// </summary>
    public class EffectAssetTests
    {
        private const string EffectsFolder = "Assets/Resources/Effects";

        private static List<(string path, Effect effect)> LoadAll() =>
            AssetDatabase.FindAssets("t:Effect", new[] { EffectsFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(p => (p, AssetDatabase.LoadAssetAtPath<Effect>(p)))
                .Where(e => e.Item2 != null)
                .ToList();

        [Test]
        public void EveryEffectId_HasOneAsset()
        {
            var effects = LoadAll();
            Assert.IsNotEmpty(effects, $"No Effect assets under {EffectsFolder}");

            var duplicates = effects.Where(e => !string.IsNullOrEmpty(e.effect.EffectID))
                .GroupBy(e => e.effect.EffectID)
                .Where(g => g.Count() > 1)
                .Select(g => $"{g.Key}: {string.Join(", ", g.Select(e => e.path))}")
                .ToList();
            Assert.IsEmpty(duplicates, "Effect assets sharing an EffectID (a renamed row's old asset left behind?):\n" +
                string.Join("\n", duplicates));
        }

        [Test]
        public void EveryAsset_IsNamedAfterItsEffect()
        {
            var mismatched = LoadAll()
                .Where(e => !string.IsNullOrEmpty(e.effect.EffectName)
                    && Path.GetFileNameWithoutExtension(e.path) != e.effect.EffectName)
                .Select(e => $"{e.path} holds \"{e.effect.EffectName}\"")
                .ToList();
            Assert.IsEmpty(mismatched, "Effect assets whose file name isn't their effect's name (the generator " +
                "would write a second asset for them):\n" + string.Join("\n", mismatched));
        }
    }
}
