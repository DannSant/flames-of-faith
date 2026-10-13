using Game.Overworld;
using Game.Scene;
using System;
using System.IO;
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
        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, FileName);

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
            try
            {
                if (!SaveFileIO.TryReadJson(SaveFilePath, out string json))
                {
                    return false;
                }
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
                SaveFileIO.WriteAtomically(SaveFilePath, JsonUtility.ToJson(data, Application.isEditor));
                OnSaveChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RunSave] Failed to save the run: {ex}");
            }
        }
    }
}
