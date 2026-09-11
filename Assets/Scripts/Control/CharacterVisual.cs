using Game.Common;
using Game.Scene;
using Game.Utils;
using System;
using System.Collections;
using UnityEngine;

namespace Game.Control
{
    public class CharacterVisual : MonoBehaviour, IInitializeAfterStateReady, IMapComponentDisabler
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
        private Vector2 facingDirection = Vector2.right;

        public CharacterClassData CharacterData => characterData;
        // Last non-zero direction passed to SetFacingDirection, regardless of source
        // (mouse today, potentially a gamepad look/right-stick later) - lets other
        // systems (e.g. dash) reuse "the direction the player is looking" generically.
        public Vector2 FacingDirection => facingDirection;
        public bool IsAttackAnimationPlaying { get { return isAttackAnimationPlaying; } set { isAttackAnimationPlaying = value; } }
        public bool IsSpecialAttackAnimationPlaying { get { return isSpecialAttackAnimationPlaying; } set { isSpecialAttackAnimationPlaying = value; } }

        public event Action OnAttackStartAnimEvent;
        public event Action OnAttackEndAnimEvent;
        public event Action OnSpecialAttackStartAnimEvent;
        public event Action OnSpecialAttackEndAnimEvent;

        private const string DeathStateName = "Death";

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
            if (facingDirection.sqrMagnitude > 0.0001f)
            {
                this.facingDirection = facingDirection.normalized;
            }
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
            // Dash can interrupt an in-progress Attack/SpecialAttack animation via an Any
            // State transition, which skips that animation's exit frame and its Animation
            // Event — leaving IsAttackAnimationPlaying/IsSpecialAttackAnimationPlaying stuck
            // true forever, which permanently blocks auto-attack (WeaponManager.ManageAutoAttack).
            IsAttackAnimationPlaying = false;
            IsSpecialAttackAnimationPlaying = false;
            animator.SetTrigger("Dash");
        }

        public void PlayCleanseAnimation()
        {
            // Same interruption risk as Dash above (e.g. wave-end cleanse firing mid-attack).
            IsAttackAnimationPlaying = false;
            IsSpecialAttackAnimationPlaying = false;
            animator.SetTrigger("Cleanse");
        }

        // Each class has its own Cleanse clip (currently all 22 frames / ~1.83s, but
        // that isn't guaranteed to stay true), so callers should wait this long rather
        // than a shared hardcoded duration.
        public float GetCleanseAnimationDuration()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return 0f;

            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == "Cleanse")
                {
                    return clip.length;
                }
            }
            return 0f;
        }

        public void PlayAttackAnimation()
        {
            IsAttackAnimationPlaying = true;
            IsSpecialAttackAnimationPlaying = false; // Attack/SpecialAttack are mutually exclusive animator states
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
            IsAttackAnimationPlaying = false; // Attack/SpecialAttack are mutually exclusive animator states
            animator.SetTrigger("SpecialAttack");
        }

        public IEnumerator PlayDeathAnimationRoutine()
        {
            if (animator == null) yield break;

            // Clear any queued triggers so a stray Dash/Attack/Special/Cleanse
            // input right before death can't leave a trigger sitting around
            // waiting to fire once Death is reset on respawn.
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("SpecialAttack");
            animator.ResetTrigger("Dash");
            animator.ResetTrigger("Cleanse");
            // Same reasoning as Dash/Cleanse above: dying mid-attack would otherwise skip
            // the attack's exit event and leave these stuck true, blocking auto-attack
            // permanently on respawn.
            IsAttackAnimationPlaying = false;
            IsSpecialAttackAnimationPlaying = false;
            // Death is a Bool, not a Trigger: every other Any State transition
            // is gated on Death == false, so once this is set it can't be
            // interrupted by a same-frame/queued Attack/Dash/etc. trigger the
            // way a Trigger-vs-Trigger race on Any State could.
            animator.SetBool(DeathStateName, true);
            //Debug.Log($"[{Time.time:F2}] PlayDeathAnimationRoutine: Waiting for Death animation to start and finish...");

            float timeout = Time.time + 5f; // safety valve if the controller is misconfigured
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName(DeathStateName) && Time.time < timeout)
                yield return null;
            while (animator.GetCurrentAnimatorStateInfo(0).IsName(DeathStateName) &&
                   animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f && Time.time < timeout)
                yield return null;
        }

        public void ResetDeathAnimationState()
        {
            if (animator == null) return;
            animator.SetBool(DeathStateName, false);
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("SpecialAttack");
            animator.ResetTrigger("Dash");
            animator.ResetTrigger("Cleanse");
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

        public void DisableComponentsOnMap()
        {
            spriteRenderer.enabled = false;
        }
    }
}
