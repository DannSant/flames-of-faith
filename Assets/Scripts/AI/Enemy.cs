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

        //[SerializeField] private int healthPerWave = 2; // Base health increase per wave
        //[SerializeField] private int healthPerLevel = 3; // Health increase per level

        private EnemyHealth health;
       
        private BehaviorController behaviorController;
        private EnemyData enemyData;
        private bool isCorrupted;

        public bool IsCorrupted => isCorrupted;

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

        public void InitializeBossHealth()
        {
            if (health == null)
            {
                health = GetComponent<EnemyHealth>();
            }

            if (enemyData == null)
            {
                enemyData = FindEnemyData();
            }

            int baseHealth = enemyData.healthBase;
            health.SetMaxHealth(baseHealth);

        }

        public void Initialize(int waveNumber, bool spawnCorrupted = false)
        {
            if (health == null)
            {
                health = GetComponent<EnemyHealth>();
            }

            if(enemyData == null)
            {
                enemyData = FindEnemyData();
            }

            int baseHealth = enemyData.healthBase;
            int levelHealthBonus = GameSession.Instance.LevelsBeaten * enemyData.healthPerLevel; // Health bonus based on levels beaten
          
            int waveHealthBonus = (waveNumber - 1) * enemyData.healthPerWave;
           
            int calculatedHealth = baseHealth  + waveHealthBonus + levelHealthBonus;

            SetCorrupted(spawnCorrupted || enemyData.alwaysCorrupted);
            if (isCorrupted)
            {
                float healthBonus = enemyData.GetCorruptedHealthBonus(CorruptionSettings.Instance.CorruptedHealthBonus);
                calculatedHealth = Mathf.RoundToInt(calculatedHealth * (1f + healthBonus));
            }
            var enemyAnimController = GetComponent<EnemyAnimationController>();
            var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

            health.SetMaxHealth(calculatedHealth);

            var player = PlayerManager.Instance.GetPlayerComponent<PlayerController>();
            if(player==null)
            {
                return;
            }

            var target = FindAnyObjectByType<AITarget>();

            var context = new BehaviorContext
            {
                enemyGameObject = gameObject,
                enemyTransform = transform,
                playerTransform = player.transform,
                enemyData = enemyData,
                waveNumber = waveNumber,
                enemyAnimController = enemyAnimController,
                navMeshAgent = agent,
                aiFixedTarget = target,
                isCorrupted = isCorrupted
            };

            behaviorController.Initialize(context);
        }

        /// <summary>
        /// Stops the enemy and makes it untouchable, e.g. while it leaves through a portal.
        /// </summary>
        public void FreezeForDespawn()
        {
            if (behaviorController != null)
            {
                behaviorController.enabled = false;
            }

            if (TryGetComponent(out Rigidbody2D rb))
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }

            foreach (var col in GetComponentsInChildren<Collider2D>())
            {
                col.enabled = false;
            }
        }

        private void SetCorrupted(bool value)
        {
            isCorrupted = value;
            if (!isCorrupted)
            {
                return;
            }

            CorruptedDamageSource.Mark(gameObject);
            CorruptedVisual.GetOrAdd(gameObject).SetCorrupted(true);
        }
    }

}