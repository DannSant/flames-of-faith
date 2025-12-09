using DamageNumbersPro;
using Game.Common;
using UnityEngine;

namespace Game.Misc
{
    public class DamageNumberSpawner : Singleton<DamageNumberSpawner>
    {
        [SerializeField] private DamageNumber damageToEnemyNumberPrefab;
        [SerializeField] private DamageNumber damageToPlayerNumberPrefab;
        [SerializeField] private DamageNumber healToPlayerNumberPrefab;
        [SerializeField] private DamageNumber graceGainedNumberPrefab;
        [SerializeField] private DamageNumber fireDebuffDamageNumberPrefab;

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
        public void SpawnHealToPlayerNumber(Vector3 positionTospawn, float number)
        {
            healToPlayerNumberPrefab.Spawn(positionTospawn, number);
        }
        public void SpawnGraceGainedNumber(Vector3 positionTospawn, float number)
        {
            graceGainedNumberPrefab.Spawn(positionTospawn, number);
        }
        public void SpawnFireDebuffDamageNumber(Vector3 positionTospawn, float number)
        {
            fireDebuffDamageNumberPrefab.Spawn(positionTospawn, number);
        }
    }

}