using Game.AI.Behaviors;
using Game.Combat;
using Game.Control;
using Game.Enemies;
using Game.Scene;
using UnityEngine;

namespace Game.AI {
    public class Enemy : MonoBehaviour
    {
        [SerializeField]
        private EnemyType enemyType;

        [SerializeField] private int healthPerWave = 2; // Base health increase per wave
        [SerializeField] private int healthPerLevel = 3; // Health increase per level

        private EnemyHealth health;
       
        private BehaviorController behaviorController;
        private EnemyData enemyData;

        private void Awake()
        {           
            health = GetComponent<EnemyHealth>();
           
            behaviorController = GetComponent<BehaviorController>();
        }

        private void Start()
        {
            enemyData = FindEnemyData();
        }

        //Finds the EnemyData scriptable object in the EnemyDatabase based on the enemyType.
        private EnemyData FindEnemyData()
        {
            var enemyDatabase = EnemyDatabaseProvider.Instance.EnemyDatabase;
            if (enemyDatabase == null)
            {
                Debug.LogError("EnemyDatabase is not initialized.");
                return null;
            }
            return enemyDatabase.GetEnemyData(enemyType);
        }

        public void Initialize(int waveNumber)
        {
            if(enemyData == null)
            {
                enemyData = FindEnemyData();
            }

            int baseHealth = enemyData.healthBase;
            int levelHealthBonus = GameSession.Instance.LevelsBeaten * healthPerLevel; // Health bonus based on levels beaten
            int waveHealthBonus = (waveNumber - 1) * healthPerWave;
            int calculatedHealth = baseHealth  + waveHealthBonus + levelHealthBonus;
           
            health.SetMaxHealth(calculatedHealth);

            var player = PlayerManager.Instance.GetPlayerComponent<PlayerController>();
            if(player==null)
            {
                return;
            }
           
            var context = new BehaviorContext
            {
                enemyGameObject = gameObject,
                enemyTransform = transform,
                playerTransform = player.transform,
                enemyData = enemyData,
                waveNumber = waveNumber
            };

            behaviorController.Initialize(context);
        }
    }

}