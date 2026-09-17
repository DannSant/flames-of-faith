using Game.Combat;
using UnityEngine;

namespace Game.Boss
{
    public class BossRenderer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        [Tooltip("Toggled together with the sprite, so the boss can never be visible-but-unhittable " +
            "or invisible-but-hittable. Falls back to a Collider2D on this object.")]
        [SerializeField] private Collider2D bossCollider;

        [Header("First Phase Transparency")]
        [Tooltip("Fades the sprite while the boss is in its invulnerable first phase, so players can " +
            "read that it isn't hittable yet instead of only finding out by seeing zeros.")]
        [SerializeField] private bool enableFirstPhaseTransparency = true;

        [Tooltip("Sprite alpha during the first phase. 1 = fully opaque.")]
        [Range(0f, 1f)]
        [SerializeField] private float firstPhaseAlpha = 0.7f;

        private EnemyHealth health;
        private BossController bossController;
        private float defaultAlpha = 1f;

        // Hiding the boss is a phase one behaviour. Once the fight becomes real the boss has to stay
        // on screen, so fade outs are refused unless something deliberately re-allows one (the boss
        // disappearing after the player dies).
        private bool fadeOutAllowed = true;

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

            if (bossCollider == null)
            {
                bossCollider = GetComponent<Collider2D>();
            }

            if (spriteRenderer != null)
            {
                defaultAlpha = spriteRenderer.color.a;
            }

            bossController = GetComponent<BossController>();

            SetVisible(false);

            health = GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.onDeath += DeathAnimation;
            }
        }

        private void OnEnable()
        {
            if (bossController == null) return;

            bossController.OnFirstPhaseEntered += HandleFirstPhaseEntered;
            bossController.OnSecondPhaseEntered += HandleSecondPhaseEntered;
        }

        private void OnDisable()
        {
            if(health != null)
            {
                health.onDeath -= DeathAnimation;
            }

            if (bossController == null) return;

            bossController.OnFirstPhaseEntered -= HandleFirstPhaseEntered;
            bossController.OnSecondPhaseEntered -= HandleSecondPhaseEntered;
        }

        private void HandleFirstPhaseEntered()
        {
            if (!enableFirstPhaseTransparency) return;

            // Safe to set while the sprite is still hidden - alpha survives the renderer being
            // toggled off and on again by the fade in/out abilities.
            ApplyAlpha(firstPhaseAlpha);
        }

        private void HandleSecondPhaseEntered()
        {
            // Immunity drops at this exact moment, so the boss has to be present at this exact
            // moment too - an immunity flag that is off on a boss with no collider is just an
            // unkillable fight.
            SetVisible(true);

            if (!enableFirstPhaseTransparency) return;

            ApplyAlpha(defaultAlpha);
        }

        /// <summary>
        /// Whether the boss may still vanish. Turned off for the phase two transition, which also
        /// drops any fade out trigger that was already queued or playing.
        /// </summary>
        public void SetFadeOutAllowed(bool value)
        {
            fadeOutAllowed = value;

            if (value) return;

            string fadeOutAnimation = GetFadeOutAnimationName();
            if (!string.IsNullOrEmpty(fadeOutAnimation))
            {
                ResetTrigger(fadeOutAnimation);
            }
        }

        /// <summary>
        /// Called from the fade out animation's end event. It hides the boss only if that fade out
        /// is still what the boss wants - a fade already in flight when the phase changed must not
        /// take the boss away again after the transition brought it back.
        /// </summary>
        public void NotifyFadeOutCompleted()
        {
            if (!fadeOutAllowed) return;

            SetVisible(false);
        }

        private string GetFadeOutAnimationName()
        {
            return bossController != null ? bossController.GetFadeOutAnimationName() : null;
        }

        private void ApplyAlpha(float alpha)
        {
            if (spriteRenderer == null) return;

            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        private void DeathAnimation()
        {
           animator.SetTrigger("Death");
        }

        /// <summary>
        /// Whether the boss is present in the world. Sprite and collider move together so the two
        /// can never disagree - a boss the player can see is always a boss the player can hit.
        /// </summary>
        public void SetVisible(bool value)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = value;
            }

            SetTargetable(value);
        }

        /// <summary>
        /// Collider only, for the one case where presence and targetability legitimately differ:
        /// death, where the boss stays on screen for its death animation but must stop taking hits.
        /// </summary>
        public void SetTargetable(bool value)
        {
            if (bossCollider == null) return;

            bossCollider.enabled = value;
        }

        public void TriggerAnimation(string animationName)
        {
            if (animator == null)
            {
                Debug.LogWarning("Animator not found on BossRenderer.");
                return;
            }

            // Refused rather than played: the fade out transition has no exit time, so letting the
            // trigger through would pull the boss straight out of its phase transition animation.
            if (!fadeOutAllowed && animationName == GetFadeOutAnimationName())
            {
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
