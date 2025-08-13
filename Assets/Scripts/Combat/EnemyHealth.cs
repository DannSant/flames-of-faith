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

        public event OnHealthChanged onHealthChanged;
        public event OnDeath onDeath;

        private float maxHealth;
        private float currentHealth;
        private Flash flash;     
        
     
        private void Start()
        {
            currentHealth = maxHealth;
            flash = GetComponent<Flash>();
        }

        public void SetMaxHealth(int amount)
        {
            maxHealth = amount;
            currentHealth = maxHealth;
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(float damage)
        {
            if(damagedSFX!=null)
            {
                AudioManager.Instance.PlayLowVolumeSFX(damagedSFX,true);
            }
            currentHealth -= damage;
            onHealthChanged?.Invoke(currentHealth, maxHealth);
            healthbar.SetHealth(currentHealth, maxHealth);
            flash.StartFlash();
            if (currentHealth <= 0)
            {
                healthbar.Hide();
                DetectDeath();
            }
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
            var instancedVfx = Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
            //GameObject.Destroy(gameObject);
            onDeath?.Invoke();
            GameObject.Destroy(instancedVfx, 2f); 

        }
    }
}
