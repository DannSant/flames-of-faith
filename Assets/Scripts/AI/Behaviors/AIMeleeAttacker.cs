using Game.Combat;
using Game.Enemies;
using Game.Scene;
using UnityEngine;

namespace Game.AI.Behaviors
{
    public class MeleeAttackerBehaviorState
    {
        public float attackTimer = 0f;
        public bool isAttacking = false;
        public Collider2D meleeCollider;
    }
    [CreateAssetMenu(menuName = "Behaviors/MeleeAttacker")]
    public class AIMeleeAttacker : AIUpdateBehavior, IAnimationEventReceiver
    {
        [SerializeField] private float maxRange = 7f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private string colliderObjectName = "MeleeCollider";

       

        public override void Tick(BehaviorContext context)
        {
            if (context.enemyTransform == null || context.playerTransform == null) return;
            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();
            var animator = context.enemyTransform.GetComponent<Animator>();
            var colliderTransform = context.enemyTransform.Find(colliderObjectName);
            if (colliderTransform == null) return;

            var colliderObj = colliderTransform.gameObject;

            if (rb == null || animator == null || colliderObj == null) return;

            float distance = Vector2.Distance(rb.position, context.playerTransform.position);

            var state = context.GetState<MeleeAttackerBehaviorState>(this);

            // Initialize melee collider if not already set
            if (state.meleeCollider == null)
            {
                state.meleeCollider = colliderObj.GetComponent<Collider2D>();
                if (state.meleeCollider != null) { 
                    state.meleeCollider.enabled = false;
                    if (colliderObj.TryGetComponent(out EnemyDamage damage))
                    {
                        int levelDamageBonus = GameSession.Instance.LevelsBeaten * context.enemyData.damagePerLevel;
                        int waveDamageBonus = context.enemyData.damagePerWave * (context.waveNumber - 1);
                        int damageAmount = context.enemyData.damageBase + waveDamageBonus + levelDamageBonus;
                        damage.SetDamageAmount(damageAmount);
                    }
                }

            }

            if (state.isAttacking) return;
            if (distance <= maxRange)
            {
                if (state.attackTimer <= 0f)
                {
                    Attack(animator, state);
                }
                else
                {
                    state.attackTimer -= Time.deltaTime;
                }
            }
        }

        private void Attack(Animator animator, MeleeAttackerBehaviorState state )
        {
            state.isAttacking = true;
            state.attackTimer = attackCooldown;
            
          
            if (animator != null)
            {
               
                animator.SetTrigger("Attack");

            }
        }

        public void OnAnimationEventEnd(BehaviorContext context, string eventName)
        {
            var state = context.GetState<MeleeAttackerBehaviorState>(this);
            state.isAttacking = false;
         
            if (state.meleeCollider != null)
            {
                state.meleeCollider.enabled = false;
            }else
            {
                Debug.LogWarning("Melee collider is not set in AIMeleeAttacker behavior state.");
            }

        }

        public void OnAnimationEventStart(BehaviorContext context, string eventName)
        {
            var state = context.GetState<MeleeAttackerBehaviorState>(this);
            if (state.meleeCollider != null)
            {
                state.meleeCollider.enabled = true;
            }
            else
            {
                Debug.LogWarning("Melee collider is not set in AIMeleeAttacker behavior state.");
            }
        }
    }

}