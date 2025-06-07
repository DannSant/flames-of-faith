using Game.Common;
using Game.Control;
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
            if (WaveSpawner.Instance == null)
            {
                Debug.LogError("WaveSpawner instance not found.");
            }
            WaveSpawner.Instance.OnWaveStarted += PlayerHealth_OnWaveStarted;
        }

        private void OnDisable()
        {
            // Unsubscribe to avoid memory leaks
            playerProgression.onStatUpdated -= OnStatUpdated;
            WaveSpawner.Instance.OnWaveStarted -= PlayerHealth_OnWaveStarted;           
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
            characterVisual?.TriggerFlash();
            if (currentHealth <= 0)
            {
                Die();
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
            characterVisual?.Hide(); // Hide the player sprite          
            yield return new WaitForSeconds(1f);
            onDeath?.Invoke();
        }

        public int GetCurrentHealth() => currentHealth;
        public int GetMaxHealth() => maxHealth;
        public bool IsDead() => isDead;

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
            currentHealth = GameSession.Instance.PlayerData.currentHealth;
        }
    }

}