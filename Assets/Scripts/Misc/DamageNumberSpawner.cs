using DamageNumbersPro;
using Game.Common;
using UnityEngine;

namespace Game.Misc
{
    public class DamageNumberSpawner : Singleton<DamageNumberSpawner>
    {
        [SerializeField] private DamageNumber damageToEnemyNumberPrefab;
        [SerializeField] private DamageNumber damageToPlayerNumberPrefab;

        protected override void Awake()
        {
            base.Awake();
        }

        public void SpawnDamageToEnemyNumber(Vector3 positionTospawn, float number)
        {
            damageToEnemyNumberPrefab.Spawn(positionTospawn, number);
        }

        public void SpawnDamageToPlayerNumber(Vector3 positionTospawn, float number)
        {
            damageToPlayerNumberPrefab.Spawn(positionTospawn, number);
        }
    }

}