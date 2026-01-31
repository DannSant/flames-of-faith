using Game.Scene;
using Game.Utils;
using System;
using UnityEngine;

namespace Game.Control
{
    public class CharacterVisual : MonoBehaviour, IInitializeAfterStateReady
    {
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private SpriteRenderer attackEffect;

        private CharacterClassData characterData;
        private SpriteRenderer spriteRenderer;
       
        private Animator animator;
        private Color originalColor;
        private bool isFlashing;
        private float flashTimer;
        private bool isAttackAnimationPlaying = false;
        private bool isSpecialAttackAnimationPlaying = false;

        public CharacterClassData CharacterData => characterData;
        public bool IsAttackAnimationPlaying { get { return isAttackAnimationPlaying; } set { isAttackAnimationPlaying = value; } }
        public bool IsSpecialAttackAnimationPlaying { get { return isSpecialAttackAnimationPlaying; } set { isSpecialAttackAnimationPlaying = value; } }

        public event Action OnAttackStartAnimEvent;
        public event Action OnAttackEndAnimEvent;
        public event Action OnSpecialAttackStartAnimEvent;
        public event Action OnSpecialAttackEndAnimEvent;

        private LevelData currentLevelData = null;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            originalColor = spriteRenderer.color;            

            
        }


        public void InitializeAnimationParams()
        {
           

            var levelSettings = FindAnyObjectByType<LevelSettings>();
            if (levelSettings != null)
            {               
                currentLevelData = levelSettings.LevelData;
            }
            


            if (currentLevelData != null)
            { 
               
                if (currentLevelData.type == LevelType.Combat || currentLevelData.type == LevelType.Boss)
                {
                    animator.SetBool("InCombat", true);
                }
                else
                {
                    animator.SetBool("InCombat", false);
                }
            }
            else
            {
                animator.SetBool("InCombat", true);
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

        public void SetFacingDirection(Vector2 facingDirection)
        {
            animator.SetFloat("DirectionX", facingDirection.x);
            animator.SetFloat("DirectionY", facingDirection.y);
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

        public void PlayCleanseAnimation()
        {
            animator.SetTrigger("Cleanse");
        }

        public void PlayAttackAnimation()
        {
            IsAttackAnimationPlaying = true;
            animator.SetTrigger("Attack");
        }

        public void AttackStartAnimEvent()
        {
            OnAttackStartAnimEvent?.Invoke();
        }


        public void AttackEndAnimEvent() 
        {
            IsAttackAnimationPlaying = false;
            OnAttackEndAnimEvent?.Invoke();
        }

        public void SpecialAttackStartAnimEvent()
        {
            OnSpecialAttackStartAnimEvent?.Invoke();
        }

        public void SpecialAttackEndAnimEvent()
        {
            IsSpecialAttackAnimationPlaying = false;
            OnSpecialAttackEndAnimEvent?.Invoke();
        }

        public void PlayAttackSpecialAnimation()
        {
            IsSpecialAttackAnimationPlaying = true;
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

        public void InitializeAfterStateReady()
        {
            InitializeAnimationParams();
        }
    }
}
