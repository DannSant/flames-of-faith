using Game.Common;
using Game.Saving;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game.Metaprogression
{
    [System.Serializable]
    public class MetaState
    {
        public string playerName = "Player";
        public List<string> unlockedEffects = null;
    }

    public class MetaProgressionManager : Singleton<MetaProgressionManager>
    {
        [SerializeField] private List<GameObject> gameObjectsWithMetaProgression = new List<GameObject>();

        private List<IMetaProgressionStateLoader> metaProgressioncomponentsList = new List<IMetaProgressionStateLoader>();

        private MetaState m_MetaState;

        private string SaveFilePath =>
             Path.Combine(Application.persistentDataPath, $"{GetPlayerName()}_meta.dat");

        private const string EncryptionKey = "FlamesOfFaith2025!";

        protected override void Awake()
        {
            base.Awake();
            BuildMetaProgressionList();
            LoadMetaProgression();
        }

        private void BuildMetaProgressionList()
        {
            metaProgressioncomponentsList.Clear();
            foreach (var go in gameObjectsWithMetaProgression)
            {
                var loader = go.GetComponent<IMetaProgressionStateLoader>();
                if (loader != null && !metaProgressioncomponentsList.Contains(loader))
                {
                    metaProgressioncomponentsList.Add(loader);
                }
            }
        }

        private void LoadMetaProgression()
        {
            // Load the meta state from file, if failed or not found, return a new MetaState with null properties
            m_MetaState = LoadStateFromFile();

            if (metaProgressioncomponentsList == null || metaProgressioncomponentsList.Count == 0)
            {
                Debug.LogWarning("No MetaProgression components registered in GameSession.");
                return;
            }

            //For each component with metaprogression state, load the state and initialize it
            foreach (var loader in metaProgressioncomponentsList)
            {
                loader.LoadState(m_MetaState);
                loader.Initialize();
            }            
        }

        private void SaveMetaProgression()
        {
            try
            {
                string json = JsonUtility.ToJson(m_MetaState);
                string encrypted = XOR(json, EncryptionKey);

                File.WriteAllText(SaveFilePath, encrypted);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to save meta progression: " + ex);
            }
        }

        private MetaState LoadStateFromFile()
        {
            //Check if the file exists, if not return a new MetaState
            //The new metastate will have null properties, this will indicate it is a fresh start
            if (!File.Exists(SaveFilePath))
                return new MetaState();

            try
            {
                string encrypted = File.ReadAllText(SaveFilePath);
                string json = XOR(encrypted, EncryptionKey);
                return JsonUtility.FromJson<MetaState>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed loading meta progression: " + ex);
                return new MetaState();
            }
        }

        public string GetPlayerName()
        {
            if (m_MetaState == null)
                return "Player";
            return m_MetaState.playerName;
        }

        public void SaveUnlockedEffects(List<string> unlockedEffectIDs)
        {
            m_MetaState.unlockedEffects = unlockedEffectIDs;
            SaveMetaProgression();
        }

        private static string XOR(string input, string key)
        {
            var result = new char[input.Length];
            for (int i = 0; i < input.Length; i++)
                result[i] = (char)(input[i] ^ key[i % key.Length]);
            return new string(result);
        }

    }

}