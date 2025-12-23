using Game.Audio;
using Game.Misc;
using Game.Progression;
using Game.UI;
using System.Collections;
using UnityEngine;
using static Game.Combat.PlayerHealth;

namespace Game.Combat
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [Header("UI")]
        [SerializeField] private EnemyHealthbar healthbar;

        [Header("Settings")]
        [SerializeField] private float deathDelay = 0.5f; // Delay before destroying the object after death

        [Header("Effects")]
        [SerializeField] private GameObject deathVfxPrefab;
        [SerializeField] private AudioClip damagedSFX;
        [SerializeField] private AudioClip deathSFX;
        [SerializeField] private bool shouldSpawnDamageNumbers = true;

        public event OnHealthChanged onHealthChanged;
        public event OnDeath onDeath;

        private float maxHealth;
        private float currentHealth;
        private Flash flash;    
        private float extraDamageTakenPercentage = 0f;

        //References
        private PlayerHealth playerHealth;


        private void Start()
        {
            currentHealth = maxHealth;
            flash = GetComponent<Flash>();
            playerHealth = Scene.PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
        }

        public void SetMaxHealth(int amount)
        {
            maxHealth = amount;
            currentHealth = maxHealth;
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(DamageRequest damageRequest)
        {
            if(damageRequest == null)
            {
                Debug.LogWarning("DamageRequest is null!");
                return;
            }
            float damage = damageRequest.baseDamage;

            if (damagedSFX != null)
            {
                AudioManager.Instance.PlayLowVolumeSFX(damagedSFX, true);
            }
            currentHealth -= damage;
            onHealthChanged?.Invoke(currentHealth, maxHealth);
            healthbar.SetHealth(currentHealth, maxHealth);
            flash.StartFlash();
            if (currentHealth <= 0)
            {
                healthbar.Hide();
                DetectDeath();
                return;
            }

            if (extraDamageTakenPercentage > 0f)
            {
                InflictExtraDamage(damage);
            }

            bool canTriggerLifesteal = damageRequest.canTriggerLifeSteal;
            if (playerHealth != null && canTriggerLifesteal)
            {
                playerHealth.Lifesteal(damage);
            }

        }

        private void InflictExtraDamage(float damage)
        {
            float extraDamage = damage * (extraDamageTakenPercentage);
            currentHealth -= damage;
            onHealthChanged?.Invoke(currentHealth, maxHealth);
            healthbar.SetHealth(currentHealth, maxHealth);
            DamageNumberSpawner.Instance.SpawnFrostDebuffDamageNumber(transform.position,extraDamage);
            if (currentHealth <= 0)
            {
                healthbar.Hide();
                DetectDeath();
            }
        }

        public void SetExtraDamageTakenPercentage(float percentage)
        {
            extraDamageTakenPercentage = percentage;
        }

        public void ResetExtraDamageTakenPercentage()
        {
            extraDamageTakenPercentage = 0f;
        }

        private void DetectDeath()
        {
            if (damagedSFX != null)
            {
                AudioManager.Instance.PlayLowVolumeSFX(deathSFX, true);
            }
            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            yield return new WaitForSeconds(deathDelay);
            if (deathVfxPrefab != null)
            {
                var instancedVfx = Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
                GameObject.Destroy(instancedVfx, 2f);
            }           
            //GameObject.Destroy(gameObject);
            onDeath?.Invoke();
        }

        public bool ShouldSpawnDamageNumber()
        {
            return shouldSpawnDamageNumbers;
        }

        public bool ShouldSpawnEffectObject()
        {
            return true;
        }
    }
}
