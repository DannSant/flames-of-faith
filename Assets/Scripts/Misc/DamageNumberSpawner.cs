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
        [Tooltip("Optional. Shown when the player dodges a hit (Evasion). Falls back to the damage-to-player style.")]
        [SerializeField] private DamageNumber dodgeNumberPrefab;
        [Tooltip("Optional. Shown when the player earns Favors. Falls back to the Grace-gained style.")]
        [SerializeField] private DamageNumber favorNumberPrefab;
        [SerializeField] private Color favorColor = new Color(1f, 0.85f, 0.35f);
        [SerializeField] private string dodgeText = "Dodge";
        [Tooltip("Optional. Shown when an immune enemy is hit (the boss's first phase). Falls back to the damage-to-enemy style.")]
        [SerializeField] private DamageNumber immuneNumberPrefab;
        [SerializeField] private string immuneText = "IMMUNE";
        [Tooltip("Tint of the IMMUNE text when it uses the fallback damage-to-enemy style.")]
        [SerializeField] private Color immuneColor = new Color(0.75f, 0.75f, 0.75f);
        [SerializeField] private DamageNumber graceGainedNumberPrefab;
        [SerializeField] private DamageNumber graceLostNumberPrefab;
        [SerializeField] private DamageNumber corruptionGainedNumberPrefab;
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
            // Damage to the player is fractional, but it's shown as a whole number. Any hit that
            // lands shows at least 1, so a small hit never reads as 0.
            float shown = number > 0f ? Mathf.Max(1f, Mathf.Round(number)) : 0f;
            damageToPlayerNumberPrefab.Spawn(positionTospawn, shown);
        }
        public void SpawnFavorGainedNumber(Vector3 positionTospawn, int amount)
        {
            var prefab = favorNumberPrefab != null ? favorNumberPrefab : graceGainedNumberPrefab;
            if (prefab == null || amount == 0) return;
            string text = $"{(amount > 0 ? "+" : "")}{amount} {(Mathf.Abs(amount) == 1 ? "Favor" : "Favors")}";
            prefab.Spawn(positionTospawn, text).SetColor(favorColor);
        }

        public void SpawnDodgeNumber(Vector3 positionTospawn)
        {
            var prefab = dodgeNumberPrefab != null ? dodgeNumberPrefab : damageToPlayerNumberPrefab;
            if (prefab == null) return;
            prefab.Spawn(positionTospawn, dodgeText);
        }

        /// <summary>A hit on an immune enemy: "IMMUNE" instead of a 0 (EnemyHealth throttles how often).</summary>
        public void SpawnImmuneNumber(Vector3 positionTospawn)
        {
            if (immuneNumberPrefab != null)
            {
                immuneNumberPrefab.Spawn(positionTospawn, immuneText);
                return;
            }
            if (damageToEnemyNumberPrefab == null) return;
            damageToEnemyNumberPrefab.Spawn(positionTospawn, immuneText).SetColor(immuneColor);
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
        public void SpawnCorruptionGainedNumber(Vector3 positionTospawn, float number)
        {
            if (corruptionGainedNumberPrefab == null) return;
            corruptionGainedNumberPrefab.Spawn(positionTospawn, number);
        }
    }

}