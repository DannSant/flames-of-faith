using Game.AI;
using Game.Combat;
using System;
using UnityEngine;

namespace Game.Boss
{
    [System.Serializable]
    public class BossAltar :  AITarget
    {
        [Header("Settings")]
        [SerializeField] private float midThreshold = 50f;
        [SerializeField] private float maxThreshold = 99f;
        [Header("References")]
        [SerializeField] private Animator animator;

        private BossWaveHandler waveHandler;

        private void Start()
        {
            waveHandler = FindAnyObjectByType<BossWaveHandler>();
            if (waveHandler == null)
            {
                Debug.LogError("BossWaveHandler not found!");
            }

            waveHandler.OnFlameProgressChanged += HandleFlameProgressChanged;
        }

        private void OnDisable()
        {
            if(waveHandler != null)
            {
                waveHandler.OnFlameProgressChanged -= HandleFlameProgressChanged;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var altarAttacker = collision.GetComponent<AltarAttacker>();
            if (altarAttacker != null && waveHandler != null)
            {
                waveHandler.ReduceFlameProgress(altarAttacker.DamageAmount);
            }

            var enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(new DamageRequest(9999f, WeaponClass.Melee, false));
            }
        }

        private void HandleFlameProgressChanged(float current, float max)
        {
           float percentage = (current / max) * 100f;
            if(percentage < midThreshold)
            {
                animator.SetTrigger("Small");
            }else if (percentage >= midThreshold && percentage < maxThreshold)
            {
                animator.SetTrigger("Medium");
            }
            else if (percentage >= maxThreshold)
            {
                animator.SetTrigger("High");
            }
        }
    }

}