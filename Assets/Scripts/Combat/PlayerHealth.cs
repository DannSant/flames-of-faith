using Game.Common;
using Game.Control;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using Game.Waves;
using System.Collections;
using UnityEngine;
using static Game.Progression.PlayerProgression;

namespace Game.Combat {
    public class PlayerHealth : MonoBehaviour, ILateInitializable, IDependentStateLoader
    {
        //state
        private float defaultMaxHealth = 20; // Default max health value
        private float maxHealth = 20;
        private float currentHealth;
        private bool isDead = false;

        public delegate void OnHealthChanged(float current, float max);
        public event OnHealthChanged onHealthChanged;

        public delegate void OnDeath();
        public event OnDeath onDeath;

        // Invulnerability logic
        [Header("Invulnerability Settings")]
        [SerializeField] private float invulnerabilityDuration = 0.5f;
        private float invulnerableUntilTime = 0f;

        private int armor = 0;
        private CharacterVisual characterVisual;
        private PlayerProgression playerProgression;     
      
        private void Awake()
        {          
            characterVisual = GetComponentInChildren<CharacterVisual>();
            if(characterVisual == null)
            {
                Debug.LogError("CharacterVisual component not found on PlayerHealth.");
            }
           
        }

        private void Start()
        {
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            currentHealth = playerProgression.GetStatTotal(StatType.MaxHealth);
            armor = playerProgression.GetStatTotal(StatType.Armor);

            onHealthChanged?.Invoke(currentHealth, maxHealth);

            // Suscribe to onStatUpdated event to get notifications when the stats for health and armor change
            playerProgression.onStatUpdated += OnStatUpdated;           

        }

        public void LateInitialize()
        {
            if (MainSceneController.Instance == null)
            {
                return;
            }
            MainSceneController.Instance.OnGameplayUISetupRequested += PlayerHealth_OnSceneLoaded;
        }

        private void OnDisable()
        {
            // Unsubscribe to avoid memory leaks
            playerProgression.onStatUpdated -= OnStatUpdated;
            MainSceneController.Instance.OnGameplayUISetupRequested -= PlayerHealth_OnSceneLoaded;
        }      

        private void ResetPlayerHealthState() 
        {
            isDead = false;
            characterVisual?.Show();
            maxHealth = defaultMaxHealth;
            currentHealth = maxHealth;
            armor = 0;
            invulnerableUntilTime = 0f;
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void PlayerHealth_OnSceneLoaded()
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
            float increasedAmount = value - maxHealth;
            maxHealth = Mathf.Max(1, value);
            currentHealth = currentHealth + increasedAmount;
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void SetArmor(int value)
        {
          
            armor = Mathf.Max(0, value); // Ensure armor is never negative
        }

        public void TakeDamage(float amount)
        {
            if (currentHealth <= 0) return;

            if (Time.time < invulnerableUntilTime)
            {               
                return;
            }            

            // Calculate damage after armor, ensure taking at least 1 damage
            float finalDamage = Mathf.Max(1, amount - armor);
            currentHealth -= finalDamage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            DamageNumberSpawner.Instance.SpawnDamageToPlayerNumber(transform.position, finalDamage);

            onHealthChanged?.Invoke(currentHealth, maxHealth);

            // Set invulnerability time
            invulnerableUntilTime = Time.time + invulnerabilityDuration;
            characterVisual?.TriggerFlash();
            if (currentHealth <= 0)
            {
                Die();
            }
        }       

        public void Heal(float amount)
        {
            if (currentHealth <= 0) return;

            DamageNumberSpawner.Instance.SpawnHealToPlayerNumber(transform.position, amount);

            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void Lifesteal(float damage)
        {
            if (currentHealth <= 0) return;
            if (damage <= 0) return;

            float lifesteal = playerProgression.GetStatTotal(StatType.LifeSteal);

            if (lifesteal<=0)
            {
                return;
            }

            float minLifestealDamageThreshold = 1f;
            float maxLifestealDamageThreshold = 20f;
            float lifestealChanceBase = 0.20f;
            float lifestealChancePerStat = 0.01f;            
            float lifestealAmountPerStat = 0.1f;
           
           
            float chance = lifestealChanceBase + (lifestealChancePerStat * lifesteal);
            if (Random.value <= chance)
            {                
                float healAmount = Mathf.Clamp(lifesteal * lifestealAmountPerStat, minLifestealDamageThreshold, maxLifestealDamageThreshold);
                Heal(healAmount);
                Debug.Log($"[PlayerHealth] Lifesteal activated! Healed for {healAmount} health.");
            }else
            {
                Debug.Log($"[PlayerHealth] Lifesteal did not activate. Current Chance: {chance}");
            }
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
            characterVisual?.Hide(); // Hide the player sprite          
            yield return new WaitForSeconds(1f);
            onDeath?.Invoke();
        }

        public float GetCurrentHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;
        public bool IsDead() => isDead;
        public bool IsAtMaxHealth() => currentHealth >= maxHealth;

        public void ResetState()
        {
            ResetPlayerHealthState();
        }

        public void SaveState()
        {
            GameSession.Instance.SaveCurrentHealth(currentHealth);
        }

        public void LoadState()
        {
            maxHealth = GameSession.Instance.PlayerData.savedStats[StatType.MaxHealth];           
            currentHealth = maxHealth;
            armor = GameSession.Instance.PlayerData.savedStats[StatType.Armor];
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

}