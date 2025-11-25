using Game.Audio;
using Game.Common;
using Game.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Scene
{

    public class LevelSelectionController : Singleton<LevelSelectionController>
    {

        [Header("Level Progression")]
        [SerializeField] private List<ActConfig> acts = new List<ActConfig>();
        
        [Header("Testing")]
        [Tooltip("This level will be always resolved if it is present. This is for testing purposes only")]
        [SerializeField] private LevelData testLevel;

        private Dictionary<int, List<MapLayer>> layersByAct = new Dictionary<int, List<MapLayer>>();

        private MapGenerator mapGenerator = new MapGenerator();

        //state
        private int currentAct = 1;
        private int currentLayer = 0;             
        private MapNode lastVisitedNode = null;
        private int currentLevelNodeIndex = 0;
        private int lastActLoaded = 0;
        private Dictionary<int, int> combatPoolIndexByAct = new();
        private Dictionary<int, int> extraPoolIndexByAct = new();

        protected override void Awake()
        {
            base.Awake();            
        }

        /// <summary>
        /// Initial setup and reset of the state. Called from GameSession when starting a new run.
        /// </summary>
        public void InitialSetup()
        {
            lastVisitedNode = null;
            currentAct = 1;
            currentLayer = 0;          
            currentLevelNodeIndex = 0;
            GenerateMapLevelByAct();          
           
        }

        /// <summary>
        /// Plays the music for the current act if not already playing
        /// </summary>
        public void StartMusicOfCurrentAct()
        {
            if (lastActLoaded == currentAct)
            {
                return; // Music already playing for the current act
            }

            lastActLoaded = currentAct;
            var musicClip = acts.FirstOrDefault(a => a.ActNumber == currentAct).MusicClip;
            if (musicClip != null)
            {
                AudioManager.Instance.PlayMusic(musicClip);
            }
        }

        /// <summary>
        /// Get the list of map layers for the current act, or null if not found
        /// </summary>
        public List<MapLayer> GetCurrentActMap()
        {
            if (layersByAct.ContainsKey(currentAct))
            {
                return layersByAct[currentAct];
            }else
            {
                return null;
            }
        }

        /// <summary>
        /// Generate maps for all acts based on their configurations
        /// </summary>
        public void GenerateMapLevelByAct()
        {

            foreach(var actConfig in acts)
            {
                var map = mapGenerator.GenerateActMap(actConfig);
                layersByAct[actConfig.ActNumber] = map;
            }

            Debug.Log($"[LevelSelectionController] Generated maps for all acts. Number of Acts: {layersByAct.Count}");

        }

        /// <summary>
        /// When a level is beaten, advance to the next layer in the current act. This is called from GameSession.
        /// </summary>
        public void AdvanceToNextLayer()
        {
            var act1Map = GetCurrentActMap();
            if (currentLayer < act1Map.Count - 1)
            {
                currentLayer++;                
            }
            else
            {
                Debug.LogWarning("Already at the last layer of the current act.");
            }
        }

        public List<MapLayer> BuildLevelDataOfCurrentLayer()
        {
            var map = GetCurrentActMap();
            if (map == null)
            {
                Debug.LogError("BuildLevelDataOfCurrentLayer: No map found for current act.");
                return null;
            }

            if (currentLayer >= map.Count)
            {
                Debug.LogError("BuildLevelDataOfCurrentLayer: Layer index out of range.");
                return null;
            }

            ActConfig actConfig = acts.First(a => a.ActNumber == currentAct);

            // Ensure pool indexes exist
            if (!combatPoolIndexByAct.ContainsKey(currentAct))
                combatPoolIndexByAct[currentAct] = 0;

            if (!extraPoolIndexByAct.ContainsKey(currentAct))
                extraPoolIndexByAct[currentAct] = 0;

            int combatIndex = combatPoolIndexByAct[currentAct];
            int extraIndex = extraPoolIndexByAct[currentAct];

            //
            // 1. RESOLVE ALL PREVIOUS LAYERS THAT DO NOT HAVE LEVELDATA YET
            //
            for (int i = 0; i < currentLayer; i++)
            {
                foreach (var node in map[i].Nodes)
                {
                    if (node.levelData != null)
                        continue;   // already resolved before

                    AssignLevelDataToNode(node, actConfig, ref combatIndex, ref extraIndex, i==0);
                }
            }

            //
            // 2. RESOLVE CURRENT LAYER (always)
            //
            var currentNodes = map[currentLayer].Nodes;           
            foreach (var node in currentNodes)
            {
                AssignLevelDataToNode(node, actConfig, ref combatIndex, ref extraIndex, currentLayer==0);
            }

            //
            // 3. CLEAR ALL FUTURE LAYERS
            //
            for (int i = currentLayer + 1; i < map.Count; i++)
            {
                foreach (var node in map[i].Nodes)
                {
                    node.levelData = null; // NOT YET REVEALED
                }
            }

            // Save new pool indexes
            combatPoolIndexByAct[currentAct] = combatIndex;
            extraPoolIndexByAct[currentAct] = extraIndex;

            return map;
        }

        public void SelectNode(MapNode node)
        {
            if(node.Type== LevelType.Combat)
            {
                combatPoolIndexByAct[currentAct] = combatPoolIndexByAct[currentAct]+1;
            }
            SetLastVisitedNode(node);
            MainSceneController.Instance.LoadGameplay(node.levelData);
        }

        private void SetLastVisitedNode(MapNode node)
        {
            lastVisitedNode = node;
        }

        private void AssignLevelDataToNode(MapNode node, ActConfig actConfig, ref int combatIndex, ref int extraIndex, bool isFirstLayer)
        {
            // TEST LEVEL OVERRIDE
            if (testLevel != null)
            {
                node.levelData = testLevel;
                return;
            }

            // --- FIRST LEVEL LOGIC (Layer 0, Combat)
            // Only for the very first level of the act            
            if (isFirstLayer && node.Type == LevelType.Combat)
            {
                node.levelData = actConfig.combatPool[0];
                return;
            }

            // --- EARLY BOSS LOGIC ---
            // If we run out of combat levels: force boss
            if (node.Type == LevelType.Combat && combatIndex >= actConfig.combatPool.Count)
            {
                node.levelData = actConfig.BossLevel;
                return;
            }

            switch (node.Type)
            {
                case LevelType.Combat:
                    if (combatIndex < actConfig.combatPool.Count)
                        node.levelData = actConfig.combatPool[combatIndex];
                    else
                        node.levelData = actConfig.BossLevel;
                    break;

                case LevelType.Store:
                case LevelType.Rest:
                case LevelType.Event:
                    node.levelData = GetRandomExtraLevelOfType(node.Type, actConfig);
                    node.Type = node.levelData.type;
                    /*if (extraIndex < actConfig.extraPool.Count)
                        node.levelData = actConfig.extraPool[extraIndex++];
                    else
                        node.levelData = actConfig.extraPool.LastOrDefault();*/
                    break;

                case LevelType.Boss:
                    node.levelData = actConfig.BossLevel;
                    break;
            }
        }

        private LevelData GetRandomExtraLevelOfType(LevelType levelType, ActConfig actConfig)
        {
            var filteredPool = actConfig.extraPool.FindAll(levelData => levelData.type == levelType);
            if (filteredPool.Count == 0)
            {
                return null;
            }
            int index = UnityEngine.Random.Range(0, filteredPool.Count);
            return filteredPool[index];            
        }

        public int GetCurrentLayer()
        {
            return currentLayer;
        }
    }

}