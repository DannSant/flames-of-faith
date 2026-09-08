using DamageNumbersPro;
using Game.Combat;
using Game.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Misc
{
    public class DamageNumberSpawner : Singleton<DamageNumberSpawner>
    {
        [SerializeField] private DamageNumber damageToEnemyNumberPrefab;
        [SerializeField] private DamageNumber damageToEnemyMeleeNumberPrefab;
        [SerializeField] private DamageNumber damageToEnemyRangedNumberPrefab;
        [SerializeField] private DamageNumber damageToEnemyMagicNumberPrefab;
        [SerializeField] private DamageNumber damageToPlayerNumberPrefab;
        [SerializeField] private DamageNumber healToPlayerNumberPrefab;
        [SerializeField] private DamageNumber graceGainedNumberPrefab;
        [SerializeField] private DamageNumber graceLostNumberPrefab;
        [SerializeField] private DamageNumber fireDebuffDamageNumberPrefab;
        [SerializeField] private DamageNumber frostDebuffDamageNumberPrefab;
        [FormerlySerializedAs("energyDebuffDamageNumberPrefab")]
        [SerializeField] private DamageNumber leyChargeDebuffDamageNumberPrefab;
        [SerializeField] private DamageNumber bleedDebuffDamageNumberPrefab;

        protected override void Awake()
        {
            base.Awake();
        }

        public void SpawnDamageToEnemyNumber(Vector3 positionTospawn, float number)
        {
            damageToEnemyNumberPrefab.Spawn(positionTospawn, number);
        }

        public void SpawnDamageToEnemyNumber(Vector3 positionTospawn, float number, WeaponClass damageType)
        {
            DamageNumber prefab = damageType switch
            {
                WeaponClass.Melee => damageToEnemyMeleeNumberPrefab,
                WeaponClass.Ranged => damageToEnemyRangedNumberPrefab,
                WeaponClass.Magic => damageToEnemyMagicNumberPrefab,
                _ => damageToEnemyNumberPrefab
            };
            prefab.Spawn(positionTospawn, number).SetColor(DamageTypeColorHelper.GetColor(damageType));
        }

        public void SpawnStatGainedByWeaponClass(Vector3 positionTospawn, string text, WeaponClass damageType)
        {
            DamageNumber prefab = damageType switch
            {
                WeaponClass.Melee => damageToEnemyMeleeNumberPrefab,
                WeaponClass.Ranged => damageToEnemyRangedNumberPrefab,
                WeaponClass.Magic => damageToEnemyMagicNumberPrefab,
                _ => damageToEnemyNumberPrefab
            };
            prefab.Spawn(positionTospawn, text).SetColor(DamageTypeColorHelper.GetColor(damageType));
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
        public void SpawnFrostDebuffDamageNumber(Vector3 positionTospawn, float number)
        {
            frostDebuffDamageNumberPrefab.Spawn(positionTospawn, number);
        }
        public void SpawnLeyChargeDebuffDamageNumber(Vector3 positionTospawn, float number)
        {
            leyChargeDebuffDamageNumberPrefab.Spawn(positionTospawn, number);
        }
        public void SpawnBleedDebuffDamageNumber(Vector3 positionTospawn, float number)
        {
            // Guarded because the prefab is not wired up yet; the others are all assigned already.
            if (bleedDebuffDamageNumberPrefab == null) return;
            bleedDebuffDamageNumberPrefab.Spawn(positionTospawn, number);
        }
        public void SpawnGraceLostNumber(Vector3 positionTospawn, float number)
        {
            graceLostNumberPrefab.Spawn(positionTospawn, number);
        }
    }

}