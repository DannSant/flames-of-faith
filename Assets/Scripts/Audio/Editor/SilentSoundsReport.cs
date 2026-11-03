using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Audio.Editor
{
    /// <summary>
    /// Lists the GameSoundTable entries that have no clips yet, so the owner has a live to-do list of sounds to provide.
    /// A silent entry is not an error: its hook plays nothing until clips are added.
    /// </summary>
    public static class SilentSoundsReport
    {
        public const string TablePath = "Assets/Resources/Audio/GameSoundTable.asset";

        [MenuItem("Tools/Flames of Faith/Audio/Report Silent Sounds")]
        public static void Report()
        {
            var table = AssetDatabase.LoadAssetAtPath<GameSoundTable>(TablePath);
            if (table == null)
            {
                Debug.LogError($"[SilentSounds] No sound table at {TablePath}");
                return;
            }

            var silent = FindSilent(table);
            var missing = FindMissingEntries(table);
            if (silent.Count == 0 && missing.Count == 0)
            {
                Debug.Log("[SilentSounds] Every game sound has a clip.");
                return;
            }

            var lines = new List<string> { $"[SilentSounds] {silent.Count} of {table.Entries.Count} game sounds have no clip yet:" };
            lines.AddRange(silent.Select(s => "  - " + s));
            if (missing.Count > 0)
            {
                lines.Add($"{missing.Count} sounds have no entry in the table at all (add them):");
                lines.AddRange(missing.Select(s => "  - " + s));
            }
            Debug.Log(string.Join("\n", lines), table);
        }

        public static List<GameSound> FindSilent(GameSoundTable table)
        {
            return table.Entries.Where(e => e != null && !e.HasClips).Select(e => e.sound).ToList();
        }

        public static List<GameSound> FindMissingEntries(GameSoundTable table)
        {
            var present = new HashSet<GameSound>(table.Entries.Where(e => e != null).Select(e => e.sound));
            return Enum.GetValues(typeof(GameSound)).Cast<GameSound>().Where(s => !present.Contains(s)).ToList();
        }
    }
}
