using System;
using UnityEngine;

namespace Game.Control
{
    public class CharacterVisual : MonoBehaviour
    {
        [SerializeField] private CharacterClassData characterData;
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private SpriteRenderer attackEffect;

        private SpriteRenderer spriteRenderer;
       
        private Animator animator;
        private Color originalColor;
        private bool isFlashing;
        private float flashTimer;

        public CharacterClassData CharacterData => characterData;

        public event Action OnAttackStartAnimEvent;
        public event Action OnAttackEndAnimEvent;
        public event Action OnSpecialAttackStartAnimEvent;
        public event Action OnSpecialAttackEndAnimEvent;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            originalColor = spriteRenderer.color;

            if (characterData != null)
            {
                Initialize(characterData);
            }
        }

        public void Initialize(CharacterClassData data)
        {
            characterData = data;

            if (animator != null && characterData.animatorController != null)
            {
                animator.runtimeAnimatorController = characterData.animatorController;
            }

            if (spriteRenderer != null && characterData.defaultSprite != null)
            {
                spriteRenderer.sprite = characterData.defaultSprite;
            }
        }

        public void SetFacingDirection(bool facingLeft)
        {
            spriteRenderer.flipX = facingLeft;
            if (attackEffect != null)
            {
                attackEffect.flipX = facingLeft;
            }
        }

        public void TriggerFlash()
        {            
            if (isFlashing) return;

            isFlashing = true;
            flashTimer = flashDuration;
            spriteRenderer.color = flashColor;
        }

        private void Update()
        {
            if (isFlashing)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f)
                {
                    spriteRenderer.color = originalColor;
                    isFlashing = false;
                }
            }
        }

        public void PlayMoveAnimation(bool isMoving)
        {
            animator.SetBool("Move", isMoving);
        }

        public void PlayDashAnimation()
        {
            animator.SetTrigger("Dash");
        }

        public void PlayAttackAnimation()
        {
            animator.SetTrigger("Attack");
        }

        public void AttackStartAnimEvent()
        {
            OnAttackStartAnimEvent?.Invoke();
        }


        public void AttackEndAnimEvent() 
        {
            OnAttackEndAnimEvent?.Invoke();
        }

        public void SpecialAttackStartAnimEvent()
        {
            OnSpecialAttackStartAnimEvent?.Invoke();
        }

        public void SpecialAttackEndAnimEvent()
        {
            OnSpecialAttackEndAnimEvent?.Invoke();
        }

        public void PlayAttackSpecialAnimation()
        {
            animator.SetTrigger("SpecialAttack");
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}
