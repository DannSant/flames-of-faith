using Game.Common;
using Game.Control;
using Game.Progression;
using Game.Scene;
using Game.Waves;
using System.Collections;
using UnityEngine;
using static Game.Progression.PlayerProgression;

namespace Game.Combat {
    public class PlayerHealth : Singleton<PlayerHealth>
    {
        private int defaultMaxHealth = 20; // Default max health value
        private int maxHealth = 20;
        private int currentHealth;
        private bool isDead = false;

        public delegate void OnHealthChanged(int current, int max);
        public event OnHealthChanged onHealthChanged;

        public delegate void OnDeath();
        public event OnDeath onDeath;

        // Invulnerability logic
        [Header("Invulnerability Settings")]
        [SerializeField] private float invulnerabilityDuration = 0.5f;
        private float invulnerableUntilTime = 0f;

        [Header("Blinking Settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float blinkInterval = 0.1f;
        private float nextBlinkTime;
        private bool isCurrentlyVisible = true;

        private int armor = 0;


        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            //currentHealth = maxHealth;
            currentHealth = PlayerProgression.Instance.GetStatTotal(StatType.MaxHealth);
            armor = PlayerProgression.Instance.GetStatTotal(StatType.Armor);

            onHealthChanged?.Invoke(currentHealth, maxHealth);

            // Suscribe to onStatUpdated event to get notifications when the stats for health and armor change
            PlayerProgression.Instance.onStatUpdated += OnStatUpdated;

            // Suscribe to wave event to restore health at the start of each wave
            WaveSpawner.Instance.OnWaveStarted += PlayerHealth_OnWaveStarted;

            // Suscribe to OnGameplayResetRequested to reset the state after the game reloads
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayResetRequested += ResetPlayerHealthState;
            }

        }        

        private void Update()
        {
            if (Time.time < invulnerableUntilTime)
            {
                HandleBlinking();
            }
            else if (!isCurrentlyVisible)
            {
                // Restore visibility once invulnerability ends
                if (!isDead) {
                    spriteRenderer.enabled = true;
                    isCurrentlyVisible = true;
                }
                
            }
        }

        

        private void OnDisable()
        {
            // Unsubscribe to avoid memory leaks
            PlayerProgression.Instance.onStatUpdated -= OnStatUpdated;
            WaveSpawner.Instance.OnWaveStarted -= PlayerHealth_OnWaveStarted;
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayResetRequested -= ResetPlayerHealthState;
            }
        }

        private void ResetPlayerHealthState() 
        {
            isDead = false;
            spriteRenderer.enabled = true;
            maxHealth = defaultMaxHealth;
            currentHealth = maxHealth;
            armor = 0;
            invulnerableUntilTime = 0f;
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void PlayerHealth_OnWaveStarted(int obj)
        {
            RestoreHealth();
        }

        private void OnStatUpdated(StatType statType, int value)
        {
            if (statType == StatType.MaxHealth)
            {
                SetMaxHealth(value);                
            }
            else if (statType == StatType.Armor)
            {
                SetArmor(value);
            }
        }

        private void SetMaxHealth(int value)
        {
           
            maxHealth = Mathf.Max(1, value); // Ensure it’s always at least 1
            currentHealth = maxHealth;
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void SetArmor(int value)
        {
          
            armor = Mathf.Max(0, value); // Ensure armor is never negative
        }

        public void TakeDamage(int amount)
        {
            if (currentHealth <= 0) return;

            if (Time.time < invulnerableUntilTime)
            {               
                return;
            }

            // Calculate damage after armor, ensure taking at least 1 damage
            int finalDamage = Mathf.Max(1, amount - armor);
            currentHealth -= finalDamage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);           

            onHealthChanged?.Invoke(currentHealth, maxHealth);

            // Set invulnerability time
            invulnerableUntilTime = Time.time + invulnerabilityDuration;

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void HandleBlinking()
        {
            if (Time.time >= nextBlinkTime)
            {
                isCurrentlyVisible = !isCurrentlyVisible;
                spriteRenderer.enabled = isCurrentlyVisible;
                nextBlinkTime = Time.time + blinkInterval;
            }
        }

        public void Heal(int amount)
        {
            if (currentHealth <= 0) return;

            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void RestoreHealth()
        {
            currentHealth = maxHealth;
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Die()
        {
            isDead = true;
           StartCoroutine(DeathRoutine());          

        }

        private IEnumerator DeathRoutine() 
        {          
            spriteRenderer.enabled = false; // Hide the player sprite          
            yield return new WaitForSeconds(1f);
            onDeath?.Invoke();
        }

        public int GetCurrentHealth() => currentHealth;
        public int GetMaxHealth() => maxHealth;
    }

}