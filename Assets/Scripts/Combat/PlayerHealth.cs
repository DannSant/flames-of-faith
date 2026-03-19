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
    public class PlayerHealth : MonoBehaviour, IInitializeAfterStateReady, IDependentStateLoader
    {
        //state
        private float defaultMaxHealth = 20; // Default max health value
        private float maxHealth = 20;
        private float currentHealth;
        private bool isDead = false;
        private bool isInvulnerable = false;

        public delegate void OnHealthChanged(float current, float max);
        public event OnHealthChanged onHealthChanged;

        public delegate void OnDeath();
        public event OnDeath onDeath;

        // Invulnerability logic
        [Header("Invulnerability Settings")]
        [SerializeField] private float invulnerabilityDuration = 0.5f;
        private float invulnerableUntilTime = 0f;

        [Header("Testing")]
        [SerializeField] private bool noDamage=false;

        private int armor = 0;
        private CharacterVisual characterVisual;
        private PlayerProgression playerProgression;     

        //public bool IsInvulnerable { get { return isInvulnerable; } set { isInvulnerable = value; } }

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

        }

        public void InitializeAfterStateReady()
        {
            
            float derivedMaxHealth = playerProgression.GetStatTotal(StatType.MaxHealth);
          
            SetMaxHealth(derivedMaxHealth);
            //currentHealth = maxHealth;
            armor = playerProgression.GetStatTotal(StatType.Armor);

            onHealthChanged?.Invoke(currentHealth, maxHealth);

            // Suscribe to onStatUpdated event to get notifications when the stats for health and armor change
            playerProgression.onDerivedStatsChanged += OnStatUpdated;
            if (MainSceneController.Instance == null)
            {
                return;
            }
            MainSceneController.Instance.OnGameplayUISetupRequested += PlayerHealth_OnSceneLoaded;

            /*if(WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveCompleteStarted += ToggleOnInvulnerable;
                WaveSpawner.Instance.OnWaveCompleteEnded += ToggleOffInvulnerable;
            }*/

        }

        private void OnDisable()
        {
            // Unsubscribe to avoid memory leaks
            if (playerProgression != null)
            {
                playerProgression.onDerivedStatsChanged -= OnStatUpdated;
            }

            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayUISetupRequested -= PlayerHealth_OnSceneLoaded;
            }

            /*if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveCompleteStarted -= ToggleOnInvulnerable;
                WaveSpawner.Instance.OnWaveCompleteEnded -= ToggleOffInvulnerable;
            }*/

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

        private void ToggleOnInvulnerable()
        {
           ToggleIsInvulnerable(true);
        }

        private void ToggleOffInvulnerable()
        {
           
            ToggleIsInvulnerable(false);
        }

        private void PlayerHealth_OnSceneLoaded()
        {
            RestoreHealth();
        }

        private void OnStatUpdated()
        {
            int newMaxHealth = playerProgression.GetStatTotal(StatType.MaxHealth);
            if (newMaxHealth != maxHealth)
            {
                SetMaxHealth(newMaxHealth);
            }
            int newArmor = playerProgression.GetStatTotal(StatType.Armor);
            if (newArmor != armor)
            {
                SetArmor(newArmor);
            }
        }


        private void SetMaxHealth(float value)
        {           
            //Debug.Log($"PlayerHealth SetMaxHealth: currentHealth = {currentHealth}, maxHealth={maxHealth}");          
            maxHealth = Mathf.Max(1, value);
            float percent = currentHealth / maxHealth;
            currentHealth = Mathf.Clamp(maxHealth * percent, 1, maxHealth);
            onHealthChanged?.Invoke(currentHealth, maxHealth);
            //Debug.Log($"PlayerHealth SetMaxHealth: currentHealth after recalculation = {currentHealth}, maxHealth={maxHealth}, percent+{percent}");
        }

        private void SetArmor(int value)
        {           
            armor = value;
        }

        public void ToggleIsInvulnerable(bool value)
        {
            /*if(WaveSpawner.Instance != null && WaveSpawner.Instance.EndingWave)
            {
                isInvulnerable = true;
                return; // Don't allow toggling invulnerability during wave complete
            }else
            {
               
            }*/
            isInvulnerable = value;

        }

        public void TakeDamage(float amount)
        {
            if (currentHealth <= 0) return;
            if (noDamage)
            {
                DamageNumberSpawner.Instance.SpawnDamageToPlayerNumber(transform.position, amount);
                return;
            }

            if (Time.time < invulnerableUntilTime)
            {               
                return;
            }

            if (isInvulnerable)
            {
                return;
            }

            if(WaveSpawner.Instance != null && WaveSpawner.Instance.EndingWave)
            {
                return; // Don't take damage during wave complete
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
            if (IsAtMaxHealth()) return;

            float healingReceivedStat = playerProgression.GetStatTotal(StatType.HealingReceived);

            float totalHealAmount = Mathf.Max(0f,amount + healingReceivedStat);

            if(totalHealAmount <= 0)
            {
                DamageNumberSpawner.Instance.SpawnHealToPlayerNumber(transform.position, totalHealAmount);
                return;
            }

            DamageNumberSpawner.Instance.SpawnHealToPlayerNumber(transform.position, totalHealAmount);

            currentHealth += totalHealAmount;
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
            float savedHealth = GameSession.Instance.LoadCurrentHealth();

            // Force a stat refresh
            OnStatUpdated();
           
            currentHealth = Mathf.Clamp(savedHealth, 1, maxHealth);
            onHealthChanged?.Invoke(currentHealth, maxHealth);

            /*maxHealth = GameSession.Instance.PlayerData.savedStats[StatType.MaxHealth];           
            currentHealth = maxHealth;
            armor = GameSession.Instance.PlayerData.savedStats[StatType.Armor];
            onHealthChanged?.Invoke(currentHealth, maxHealth);*/
        }
    }

}