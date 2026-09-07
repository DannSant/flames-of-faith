using Game.Audio;
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

        [Header("Armor")]
        [Tooltip("Controls how quickly armor loses effectiveness. Reduction = armor / (armor + this). " +
            "Higher values make each point of armor weaker. At 25: armor 10 = -29%, armor 25 = -50%, armor 50 = -67%.")]
        [SerializeField] private float armorEffectivenessConstant = 25f;
        [Tooltip("Armor can never reduce an attack below this fraction of its raw damage, " +
            "so stacking armor can't trivialise every enemy.")]
        [Range(0f, 1f)]
        [SerializeField] private float minDamagePercent = 0.15f;

        [Header("Testing")]
        [SerializeField] private bool noDamage=false;

        [Header("Death")]
        [SerializeField] private AudioClip deathSFX;

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
            characterVisual?.ResetDeathAnimationState();
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
           
            float oldMax = maxHealth;
            float newMax = Mathf.Max(1, value);

            // If nothing changed, nothing to do
            if (Mathf.Approximately(oldMax, newMax))
            {
                return;
            }

            float newCurrent = currentHealth;

            // Avoid division by zero and handle scaling when max increases/decreases
            if (oldMax > 0f && currentHealth > 0f)
            {
                // If player was exactly at full health, keep them full after the change
                if (Mathf.Approximately(currentHealth, oldMax))
                {
                    newCurrent = newMax;
                }
                else
                {
                    float ratio = newMax / oldMax;
                    newCurrent = currentHealth * ratio;

                    // When increasing max health the requirement is to round down the resulting current health
                    if (ratio > 1f)
                    {
                        newCurrent = Mathf.Floor(newCurrent);
                    }
                }
            }

            // Apply new max and clamp current between 0 and newMax
            maxHealth = newMax;
            currentHealth = Mathf.Clamp(newCurrent, 0f, maxHealth);
            onHealthChanged?.Invoke(currentHealth, maxHealth);
            
        }

        private void SetArmor(int value)
        {
            armor = value;
        }

        /// <summary>
        /// Armor mitigates a proportion of incoming damage rather than subtracting a flat amount.
        /// Flat subtraction was a step function: every attack weaker than the player's armor
        /// collapsed to exactly 1 while anything stronger came through almost untouched, so two
        /// enemies in the same run could feel 7x apart, and a +1 tuning change could flip an enemy
        /// across that edge. Scaling keeps the relative difference between enemies intact and makes
        /// balancing predictable.
        /// </summary>
        private float ApplyArmor(float amount)
        {
            if (amount <= 0f) return 0f;

            float effectiveArmor = Mathf.Max(0f, armor);
            float reduction = effectiveArmor / (effectiveArmor + Mathf.Max(1f, armorEffectivenessConstant));
            float mitigated = amount * (1f - reduction);

            // Cap total mitigation so heavy armor stacking can't reduce everything to nothing.
            mitigated = Mathf.Max(amount * minDamagePercent, mitigated);

            // Damage is displayed as a whole number, so keep the health math matching what the
            // player is shown. The floor of 1 is a sanity guard, not the old cliff - with
            // proportional mitigation it only binds for genuinely tiny hits.
            return Mathf.Max(1f, Mathf.Round(mitigated));
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

            float finalDamage = ApplyArmor(amount);
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

        public void Heal(float amount, bool applyHealingReceivedStat = true)
        {
            if (currentHealth <= 0) return;
            if (IsAtMaxHealth()) return;

            float healingReceivedStat = applyHealingReceivedStat ? playerProgression.GetStatTotal(StatType.HealingReceived) : 0f;

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
            if (deathSFX != null) AudioManager.Instance?.PlaySFX(deathSFX);
            if (characterVisual != null) yield return characterVisual.PlayDeathAnimationRoutine();
            //characterVisual?.Hide(); // Hide the player sprite
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
        }
    }

}