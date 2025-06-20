using Game.Common;
using UnityEngine;

namespace Game.Enemies
{
    public class EnemyDatabaseProvider : Singleton<EnemyDatabaseProvider>
    {
        [SerializeField] private EnemyDatabase enemyDatabase;

        protected override void Awake()
        {
            base.Awake();
            if (enemyDatabase == null)
            {
                Debug.LogError("EnemyDatabase is not assigned in the EnemyDatabaseProvider.");
            }
            else
            {
                enemyDatabase.Initialize();
            }
        }

        public EnemyDatabase EnemyDatabase
        {
            get
            {
                if (enemyDatabase == null)
                {
                    Debug.LogError("EnemyDatabase is not assigned in the EnemyDatabaseProvider.");
                }
                return enemyDatabase;
            }
        }
    }

}