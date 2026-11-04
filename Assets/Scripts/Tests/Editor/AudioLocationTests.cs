using NUnit.Framework;
using System.Linq;
using UnityEditor;

namespace Game.Tests
{
    /// <summary>
    /// Everything under a Resources folder ships in every build, used or not. Three sound packs (736 clips, 124 MB)
    /// once sat in Resources/Audio/SFX by mistake; clips belong under Assets/Sounds and are reached through
    /// references (the GameSoundTable, prefabs, data assets), never loaded by path.
    /// </summary>
    public class AudioLocationTests
    {
        [Test]
        public void NoAudioClip_IsUnderAResourcesFolder()
        {
            var offenders = AssetDatabase.FindAssets("t:AudioClip")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.StartsWith("Assets/") && IsUnderResources(path))
                .ToList();

            Assert.IsEmpty(offenders,
                "Audio clips under a Resources folder ship in every build. Move them (with their .meta) under Assets/Sounds:\n"
                + string.Join("\n", offenders.Take(20)));
        }

        private static bool IsUnderResources(string path) =>
            path.Split('/').Any(part => part == "Resources");
    }
}
