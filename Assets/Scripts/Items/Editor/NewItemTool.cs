using Game.Effects;
using Game.Loot.Editor;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Items.Editor
{
    /// <summary>
    /// Creates a new item (an Effect asset in Resources/Effects with a fresh effectID) and gives it a drop table row.
    /// Everything else about it (text, prices, icon, stats, behaviors, where it drops) is edited in the balance sheet:
    /// Push All so the new row reaches it. Replaces the retired Effect Database window (Oct 2026).
    /// </summary>
    public class NewItemTool : EditorWindow
    {
        public const string EffectsFolder = "Assets/Resources/Effects";

        private string itemName = "";
        private EffectQuality quality = EffectQuality.Common;

        [MenuItem("Tools/Flames of Faith/Items/New Item")]
        public static void Open() => GetWindow<NewItemTool>(true, "New Item");

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Creates the item's asset and its drop table row (shop only, from the start). Then Push All " +
                "to Sheet and fill in its text, prices, icon, stats and behaviors there.", MessageType.Info);
            itemName = EditorGUILayout.TextField("Name", itemName);
            quality = (EffectQuality)EditorGUILayout.EnumPopup("Quality", quality);

            string error = Validate(itemName, EffectsFolder);
            if (error != null) EditorGUILayout.HelpBox(error, MessageType.Warning);
            using (new EditorGUI.DisabledScope(error != null))
            {
                if (GUILayout.Button("Create"))
                {
                    var effect = Create(itemName, quality, EffectsFolder);
                    ItemDropTableSync.Sync();
                    Selection.activeObject = effect;
                    EditorUtility.DisplayDialog("New Item", $"Created {effect.name}.\n\nPush All to Sheet (Balance Data menu) so it shows " +
                        "in the Items and ItemDrops tabs, then fill it in there.", "OK");
                    Close();
                }
            }
        }

        /// <summary>Why <paramref name="name"/> can't be a new item in <paramref name="folder"/>, or null when it can.</summary>
        public static string Validate(string name, string folder)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Give the item a name.";
            if (name != name.Trim()) return "The name can't start or end with spaces.";
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return "The name has characters a file name can't have.";
            if (File.Exists(Path.Combine(folder, name + ".asset"))) return $"An item named {name} already exists.";
            return null;
        }

        /// <summary>
        /// Creates the Effect asset: named after the item (EffectAssetTests), a new effectID (saves and unlocks find
        /// items by it), unlocked from the start, no stats or behaviors yet.
        /// </summary>
        public static Effect Create(string name, EffectQuality quality, string folder)
        {
            string error = Validate(name, folder);
            if (error != null) throw new ArgumentException(error);

            Directory.CreateDirectory(folder);
            var effect = CreateInstance<Effect>();
            effect.EffectName = name;
            effect.EffectID = Guid.NewGuid().ToString();
            effect.Quality = quality;
            effect.UnlockedByDefault = true;
            effect.Description = string.Empty;
            AssetDatabase.CreateAsset(effect, Path.Combine(folder, name + ".asset").Replace('\\', '/'));
            AssetDatabase.SaveAssets();
            return effect;
        }
    }
}
