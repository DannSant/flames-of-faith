using Game.Combat;
using System;
using UnityEngine;

namespace Game.Boss
{
    public class BossRenderer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        private EnemyHealth health;

        private void Awake()
        {
          
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    Debug.LogWarning("SpriteRenderer not found on BossRenderer.");
                }
            }

            ToggleSprite(false);

            health = GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.onDeath += DeathAnimation;
            }
        }

        private void OnDisable()
        {
            if(health != null)
            {
                health.onDeath -= DeathAnimation;
            }
        }

        private void DeathAnimation()
        {
           animator.SetTrigger("Death");
        }

        public void ToggleSprite(bool value)
        {
            spriteRenderer.enabled = value;
        }

        public void TriggerAnimation(string animationName)
        {
            if (animator == null)
            {
                Debug.LogWarning("Animator not found on BossRenderer.");
                return;
            }
            animator.SetTrigger(animationName);
        }

        public void SetAnimationBool(string parameterName, bool value)
        {
            if (animator == null)
            {
                Debug.LogWarning("Animator not found on BossRenderer.");
                return;
            }
            animator.SetBool(parameterName, value);
        }

        public void ResetTrigger(string animationName)
        {
            if (animator == null)
            {
                Debug.LogWarning("Animator not found on BossRenderer.");
                return;
            }
            animator.ResetTrigger(animationName);
        }
    }
}