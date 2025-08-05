using Game.Common;
using Game.Map;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Scene
{

    

    [System.Serializable]
    public struct ActConfig
    {
        public int ActNumber;
        public List<LevelData> Levels;
        public List<LevelData> ExtraLevels;
        public int LayerCount;
        public LevelData BossLevel;
       
        public ActConfig(int actNumber, List<LevelData> levels, List<LevelData> extraLevels, int layerCount, LevelData bossLevel)
        {
            ActNumber = actNumber;
            Levels = levels;
            ExtraLevels = extraLevels;
            LayerCount = layerCount;
            BossLevel = bossLevel;
        }
    }

    public class LevelSelectionController : Singleton<LevelSelectionController>
    {

        [Header("Level Progression")]
        [SerializeField] private List<ActConfig> levels = new List<ActConfig>();
        [SerializeField] private int minLevelsPerLayer = 2;
        [SerializeField] private int maxLevelsPerLayer = 4;

        [Header("Testing")]
        [SerializeField] private LevelData testLevel;

        private Dictionary<int, List<LevelData>> levelsByAct = new Dictionary<int, List<LevelData>>();
        private Dictionary<int, List<LevelData>> extraLevelsByAct = new Dictionary<int, List<LevelData>>();

       
        private MapGenerator mapGenerator = new MapGenerator();

        //state
        private int currentAct = 1;
        private int currentLayer = 0;
        private int currentNode = 0;
        private List<List<MapNode>> act1Map;
        private MapNode lastVisitedNode = null;

        private int currentLevelNodeIndex = 0;

        protected override void Awake()
        {
            base.Awake();
            InitializeLevels();
        }    

        private void InitializeLevels()
        {
            levelsByAct.Clear();
            extraLevelsByAct.Clear();
            foreach (var levelByAct in levels)
            {
                if (!levelsByAct.ContainsKey(levelByAct.ActNumber))
                {
                    levelsByAct[levelByAct.ActNumber] = new List<LevelData>();
                    extraLevelsByAct[levelByAct.ActNumber] = new List<LevelData>();
                }
                levelsByAct[levelByAct.ActNumber].AddRange(levelByAct.Levels);
                extraLevelsByAct[levelByAct.ActNumber].AddRange(levelByAct.ExtraLevels);
            }
        }

        public void InitialSetup()
        {
            currentAct = 1;
            currentLayer = 0;
            currentNode = 0;
            act1Map = GenerateMapLevelByAct(currentAct);
        }

        public List<List<MapNode>> GetCurrentActMap()
        {
            if (currentAct == 1)
            {
                return act1Map;
            }

            return null;
        }

        public List<MapNode> BuildLevelDataOfNodesInCurrentLayer()
        {
            var map = GetCurrentActMap();
           

            var layerNodes = map[currentLayer];

            foreach(var node in layerNodes)
            {
                if(node.Type == LevelType.Combat)
                {
                    node.levelData = GetNextLevelData();
                }else if(node.Type == LevelType.Boss)
                {
                    node.levelData = GetBossLevelByAct(currentAct);
                }
                else
                {
                    node.levelData = GetRandomExtraLevelOfType(node.Type);
                }


            }

            return layerNodes;


        }

        public List<List<MapNode>> GenerateMapLevelByAct(int actNumber)
        {
            List<List<MapNode>> result = null;

            var actConfig = levels.FirstOrDefault(a => a.ActNumber == actNumber);
            
            if (actConfig.Equals(default(ActConfig)))
            {
                Debug.LogError($"Act {actNumber} not found in levels configuration.");
                return result;
            }

            result = mapGenerator.GenerateMap(actConfig.LayerCount, minLevelsPerLayer, maxLevelsPerLayer);

            return result;

        }

        public LevelData GetBossLevelByAct(int actNumber)
        {
            var actConfig = levels.FirstOrDefault(a => a.ActNumber == actNumber);
            if (actConfig.Equals(default(ActConfig)))
            {
                Debug.LogError($"Act {actNumber} not found in levels configuration.");
                return null;
            }
            return actConfig.BossLevel;
        }

        public void AdvanceToNextLayer()
        {
            if (currentLayer < act1Map.Count - 1)
            {
                currentLayer++;
                currentLevelNodeIndex++;
            }
            else
            {
                Debug.LogWarning("Already at the last layer of the current act.");
            }
        }

        private LevelData GetNextLevelData()
        {
            List<LevelData> levelDataList = levelsByAct[currentAct];
            if (currentLevelNodeIndex < levelDataList.Count)
            {
                return levelDataList[currentLevelNodeIndex];
            }
            else
            {
                Debug.LogWarning("No more levels available in the current act.");
                return null;
            }
        }

        private LevelData GetRandomExtraLevelOfType(LevelType type) 
        {
            if (testLevel != null)
            {
                return testLevel; // For testing purposes, return a predefined level
            }

            List<LevelData> levelDataList = extraLevelsByAct[currentAct];
            var filteredLevels = levelDataList.Where(level => level.type == type).ToList();
            if (filteredLevels.Count > 0)
            {
                int randomIndex = Random.Range(0, filteredLevels.Count);
                return filteredLevels[randomIndex];
            }
            else
            {
                Debug.LogWarning($"No extra levels of type {type} available in act {currentAct}.");
                return null;
            }
        }

        public MapNode GetLastVisitedNode() => lastVisitedNode;
        public void SetLastVisitedNode(MapNode node)
        {
            lastVisitedNode = node;
        }



    }

}