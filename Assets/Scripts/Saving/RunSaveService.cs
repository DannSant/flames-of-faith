using Game.Overworld;
using Game.Scene;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Game.Saving
{
    /// <summary>
    /// Single-slot persistence for the current run. The run is autosaved whenever the player is on the
    /// overworld map, where GameSession.PlayerData is a clean snapshot (player components only write to it
    /// when a level is finished). So the file never holds mid-level progress: closing the game inside a
    /// level brings the player back to the map as it was before entering that level.
    /// </summary>
    public static class RunSaveService
    {
        private const string FileName = "run.sav";
        private const string EncryptionKey = "FlamesOfFaith2025!";

        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, FileName);
        private static string TempFilePath => SaveFilePath + ".tmp";

        public static event Action OnSaveChanged;

        public static bool HasSave => TryLoad(out _);

        /// <summary>
        /// Captures the session and the overworld map and writes them to disk.
        /// Only call while the player is on the map.
        /// </summary>
        public static void SaveCurrentRun()
        {
            var session = GameSession.Instance;
            var map = MapRunController.Instance;
            if (session == null || map == null || !map.IsInitialized)
            {
                Debug.LogWarning("[RunSave] Can't save: the session or the map isn't ready.");
                return;
            }

            if (map.IsRunComplete)
            {
                return;
            }

            var data = session.ToSaveData();
            data.map = map.CaptureSaveData();
            Write(data);
        }

        public static bool TryLoad(out RunSaveData data)
        {
            data = null;
            if (!File.Exists(SaveFilePath))
            {
                return false;
            }

            try
            {
                string contents = File.ReadAllText(SaveFilePath);
                // Editor saves are plain JSON so they can be inspected; builds are obfuscated
                string json = contents.TrimStart().StartsWith("{") ? contents : XOR(contents, EncryptionKey);
                data = JsonUtility.FromJson<RunSaveData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RunSave] Failed to read the save, ignoring it: {ex.Message}");
                data = null;
                return false;
            }

            if (data == null || data.saveVersion != RunSaveData.CurrentVersion || data.map == null)
            {
                Debug.LogWarning("[RunSave] The save is from an incompatible version, ignoring it.");
                data = null;
                return false;
            }

            return true;
        }

        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                    OnSaveChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RunSave] Failed to delete the save: {ex}");
            }
        }

        private static void Write(RunSaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, Application.isEditor);
                string contents = Application.isEditor ? json : XOR(json, EncryptionKey);

                // Write to a temp file first so a crash mid-write can't corrupt the existing save
                File.WriteAllText(TempFilePath, contents, Encoding.UTF8);
                if (File.Exists(SaveFilePath))
                {
                    File.Replace(TempFilePath, SaveFilePath, null);
                }
                else
                {
                    File.Move(TempFilePath, SaveFilePath);
                }
                OnSaveChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RunSave] Failed to save the run: {ex}");
            }
        }

        private static string XOR(string input, string key)
        {
            var output = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                output.Append((char)(input[i] ^ key[i % key.Length]));
            }
            return output.ToString();
        }
    }
}
