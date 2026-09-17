using Game.AI;
using Game.Combat;
using Game.Control;
using Game.Scene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Boss
{
    public class BossController : MonoBehaviour
    {
        [SerializeField] private BossWaveHandler waveHandler;

        [Header("Abilities")]
        [SerializeField] private List<BossAbilityBase> phaseOneAbilities;
        [SerializeField] private List<BossAbilityBase> phaseTwoAbilities;

        //[Header("References")]
        //[SerializeField] private Animator animator;  


        [Header("Adds")]
        [Tooltip("If the boss summons any adds (additional minions) they will spawn from these transforms")]
        [SerializeField] private Transform[] addsSpawnPoints;

        [Header("Cast points")]
        [SerializeField] private Transform castPointN;
        [SerializeField] private Transform castPointNE;
        [SerializeField] private Transform castPointE;
        [SerializeField] private Transform castPointSE;
        [SerializeField] private Transform castPointS;
        [SerializeField] private Transform castPointSW;
        [SerializeField] private Transform castPointW;
        [SerializeField] private Transform castPointNW;

        private Transform player;
        private BossBehavior behavior;
        private BossMovement movement;
        private BossRenderer bossRenderer;
        private Enemy enemyComponent;
        private EnemyHealth health;
        private PlayerHealth playerHealth;

        //State
        private bool isPhaseOne = false;
        private bool isPhaseTwo = false;
        private int enrageLevel = 0;
        private int currentBlockFlags = 0;
        private BossAbilityContext context;
        private Coroutine abilityLoopRoutine;

        private List<BossAbilityRuntime> phaseOneRuntimes;
        private List<BossAbilityRuntime> phaseTwoRuntimes;
        private readonly List<GameObject> activeAdds = new();

        //Events
        public event System.Action<BossAbilityRuntime> OnAbilityCasted;
        public event System.Action<BossAbilityRuntime> OnAbilityFinished;
        public event System.Action OnAllAddsDeath;

        // Phase entry. By design every boss has exactly two phases: an invulnerable first phase and
        // the real fight. OnSecondPhaseEntered fires after the transition animation, at the same
        // moment immunity is dropped, so presentation can never claim the boss is vulnerable before
        // it actually is.
        public event System.Action OnFirstPhaseEntered;
        public event System.Action OnSecondPhaseEntered;

        private void Awake()
        {
            behavior = GetComponent<BossBehavior>();
            movement = GetComponent<BossMovement>();
            behavior.Initialize(this);
            bossRenderer = GetComponent<BossRenderer>();
            enemyComponent = GetComponent<Enemy>();
            enemyComponent.InitializeBossHealth();
            health = GetComponent<EnemyHealth>();

            phaseOneRuntimes = BuildRuntimeList(phaseOneAbilities);
            phaseTwoRuntimes = BuildRuntimeList(phaseTwoAbilities);
            CreateContext();

            if (health != null) {
                health.onDeath += StopBossAbilities;
                health.onDeath += HandleBossDefeated;
            }
        }

        // The running ability is nested inside the loop coroutine (AbilityLoop yields the ability's
        // enumerator directly), so stopping the loop stops the ability mid-cast. That also means
        // OnAbilityEnded never runs for it, so the state it would have cleaned up is cleared here -
        // including its end animation, which for the phase one abilities is the fade out that used
        // to hide the boss for good if it landed after the phase two transition.
        private void StopBossAbilities()
        {
            if (abilityLoopRoutine != null)
            {
                StopCoroutine(abilityLoopRoutine);
                abilityLoopRoutine = null;
            }

            if (context != null)
            {
                context.currentAbility = null;
            }

            ClearAllBlockFlags();
        }

        // BossWaveHandler.OnBossFightEnded already exists for exactly this (FlameBarUI already
        // listens to it) - it just needed something to actually call NotifyBossDied().
        private void HandleBossDefeated()
        {
            if (waveHandler != null)
            {
                waveHandler.NotifyBossDied();
            }
        }

        private void Start()
        {
            player = PlayerManager.Instance.gameObject.transform;

            playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onDeath += HandlePlayerDeath;
            }
        }

        private void HandlePlayerDeath()
        {
            if (health != null && health.IsDead()) return; // boss already dead, nothing to hide

            StopBossAbilities();
            movement.SetCasting(true); // freeze in place instead of sliding away mid-fade
            // The one fade out the boss is still allowed after the phase two transition:
            // disappearing for good because the fight is over.
            bossRenderer.SetFadeOutAllowed(true);
            bossRenderer.TriggerAnimation(behavior.GetFadeOutAnimationName());
        }

        private void Update()
        {
            if (context == null) return;

            context.phaseTime += Time.deltaTime;
            context.enrageLevel = enrageLevel; // keep synced
            context.activeAddsCount = activeAdds.Count;
            context.isPhaseTwo = isPhaseTwo;

        }

        private void CreateContext()
        {
            context = new BossAbilityContext
            {
                bossTransform = transform,
                playerTransform = FindPlayer(),
                coroutineRunner = this,
                phaseTime = 0f,
                enrageLevel = 0,
                isPhaseTwo = false
            };
        }

        private void OnEnable()
        {
            waveHandler.OnPhaseOneStarted += EnterPhaseOne;
            waveHandler.OnPhaseTwoStarted += EnterPhaseTwo;
        }

        private void OnDisable()
        {
            waveHandler.OnPhaseOneStarted -= EnterPhaseOne;
            waveHandler.OnPhaseTwoStarted -= EnterPhaseTwo;

            if (health != null)
            {
                health.onDeath -= StopBossAbilities;
                health.onDeath -= HandleBossDefeated;
            }

            if (playerHealth != null)
            {
                playerHealth.onDeath -= HandlePlayerDeath;
            }
        }

        private List<BossAbilityRuntime> BuildRuntimeList(List<BossAbilityBase> abilities)
        {
            List<BossAbilityRuntime> list = new();

            foreach (var ability in abilities)
            {
                list.Add(new BossAbilityRuntime(ability));
            }

            return list;
        }

        private void EnterPhaseOne()
        {
            isPhaseOne = true;
            isPhaseTwo = false;
            behavior.OnPhaseOneStart();

            //Set immunity flag for phase 1 if needed
            health.IsImmuneFlag = true;

            OnFirstPhaseEntered?.Invoke();

            abilityLoopRoutine = StartCoroutine(AbilityLoop(phaseOneRuntimes));
        }

        private void EnterPhaseTwo()
        {
            isPhaseOne = false;
            isPhaseTwo = true;

            // Immunity is deliberately NOT dropped here - it is dropped at the end of
            // PhaseOneToTwoTransition, so the boss stays invulnerable for the duration of the
            // transition animation and becomes vulnerable and opaque at the same instant.

            StopBossAbilities();

            StartCoroutine(PhaseOneToTwoTransition());

        }

        private IEnumerator PhaseOneToTwoTransition()
        {
            // Phase one is the only part of the fight where the boss hides itself, so from here on
            // a fade out - one already playing, or one a phase one handler fires during the
            // transition - must not be able to take the boss away again. Without this the boss can
            // spend the whole second phase invisible and, because the collider follows the sprite,
            // unhittable, while still casting its phase two abilities.
            bossRenderer.SetFadeOutAllowed(false);

            // SetVisible (not just the sprite) because phase one usually ends while the boss is
            // faded out, and nothing in phase two plays the FadeIn whose animation event used to be
            // the only thing that re-enabled the collider - the boss would be visible but unhittable
            // for the whole fight.
            bossRenderer.SetVisible(true);
            bossRenderer.TriggerAnimation(behavior.GetPhaseTransitionAnimationName());
            yield return new WaitForSeconds(2f);
            // Reset flags on ability runtimes to prevent any weird edge cases during transition
            foreach (var runtime in phaseOneRuntimes)
            {
                runtime.ResetRuntimeState();
            }
            foreach (var runtime in phaseTwoRuntimes)
            {
                runtime.ResetRuntimeState();
            }

            // Remove immunity flag for phase 2 after transition animation
            health.IsImmuneFlag = false;

            // Fired next to the immunity drop on purpose: anything that presents "the boss is
            // vulnerable now" (the first phase transparency, for one) has to change at exactly this
            // moment, or it goes back to lying to the player.
            OnSecondPhaseEntered?.Invoke();

            // Reset flags on components if needed
            movement.SetCasting(false);

            // Before the loop starts, not after: AbilityLoop runs its first selection pass
            // synchronously, and every phase two ability blocks on AbilityBlockFlags.Everything, so
            // leftover flags from the interrupted phase one cast would skip that pass.
            ClearAllBlockFlags();

            behavior.OnPhaseTwoStart();

            abilityLoopRoutine = StartCoroutine(AbilityLoop(phaseTwoRuntimes));


        }

        private IEnumerator AbilityLoop(List<BossAbilityRuntime> abilities)
        {
            while (true)
            {
                // 🔹 Pick highest priority valid ability
                BossAbilityRuntime selected = null;

                foreach (var ability in abilities)
                {
                    if (!ability.CanExecute(this, context))
                    {
                        /*if(isPhaseTwo)
                        {
                            Debug.Log($"[BossController] Phase 2 - Skipping ability: {ability.GetBossAbility().abilityName}, Enrage Level: {enrageLevel}, Active Adds: {context.activeAddsCount}");
                        }*/
                        continue;
                    }

                    if (selected == null || ability.GetBossAbility().priority > selected.GetBossAbility().priority)
                    {
                        selected = ability;
                    }
                }

                /*if (isPhaseTwo)
                {
                    Debug.Log($"[BossController] Phase 2 - Selected ability: {(selected != null ? selected.GetBossAbility().abilityName : "None")}, Enrage Level: {enrageLevel}, Active Adds: {context.activeAddsCount}");
                }*/

                if (selected != null)
                {
                    context.currentAbility = selected.GetBossAbility();
                    movement.SetCasting(true);
                    // Yielded directly rather than through StartCoroutine: a coroutine started that
                    // way is independent of this loop and survives StopCoroutine(abilityLoopRoutine),
                    // so a phase one ability interrupted by the phase two transition kept running -
                    // still spawning projectiles, and still firing its fade out end animation into
                    // the second phase.
                    yield return selected.Execute(
                        this,
                        context,
                        OnAbilityStarted,
                        OnAbilityEnded
                    );
                    context.currentAbility = null;
                    movement.SetCasting(false);
                }

                yield return null;
            }
        }

        private Transform FindPlayer()
        {
            return PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
        }

        private void OnAbilityStarted(AbilityBlockFlags flags, BossAbilityRuntime abilityRuntime)
        {
            OnAbilityCasted?.Invoke(abilityRuntime);
            currentBlockFlags |= (int)flags;
            var ability = abilityRuntime.GetBossAbility();
            // Debug.Log($"Ability started: {ability.abilityName}, hasInitialAnimation flags: {ability.hasInitialAnimation}");
            if (ability.hasInitialAnimation)
            {
                bossRenderer.ResetTrigger(ability.initialAnimationName);
                bossRenderer.TriggerAnimation(ability.initialAnimationName);
            }
        }

        private void OnAbilityEnded(BossAbilityRuntime abilityRuntime)
        {

            var ability = abilityRuntime.GetBossAbility();
            currentBlockFlags &= ~(int)ability.blocksWhileActive;
            //Debug.Log($"Ability ended: {ability.abilityName}, hasEndAnimation flags: {ability.hasEndAnimation}");
            if (ability.hasEndAnimation)
            {
                bossRenderer.ResetTrigger(ability.endAnimationName);
                bossRenderer.TriggerAnimation(ability.endAnimationName);
            }

        }

        public bool HasBlockFlag(AbilityBlockFlags flag)
        {
            bool result = ((AbilityBlockFlags)currentBlockFlags).HasFlag(flag);
            /*if (isPhaseTwo)
            {
                Debug.Log($"[BossController] HasBlockFlag called. flag={flag}, currentBlockFlags={(AbilityBlockFlags)currentBlockFlags}, result={result}");
            }*/
            return result;
        }

        // Reset example
        private void ClearAllBlockFlags()
        {
            currentBlockFlags = (int)AbilityBlockFlags.None; // equals 0
        }

        // 🔹 Public API for abilities
        public Transform GetPlayer() => context.playerTransform;

        public BossRenderer GetBossRenderer() => bossRenderer;

        /// <summary>
        /// Name of the vanish animation for this boss, so the renderer can recognise (and refuse) a
        /// fade out request without the name being duplicated in the inspector.
        /// </summary>
        public string GetFadeOutAnimationName() => behavior != null ? behavior.GetFadeOutAnimationName() : null;

        public int GetEnrageLevel() => enrageLevel;

        public void IncreaseEnrageLevel()
        {
            enrageLevel++;           
        }

        public void RegisterAdd(GameObject add)
        {

            activeAdds.Add(add);
        }

        public void NotifyAddDied(GameObject add)
        {
            activeAdds.Remove(add);

            if (activeAdds.Count <= 0)
            {
                IncreaseEnrageLevel();
                OnAllAddsDeath?.Invoke(); // we can pass context if needed
            }
        }

        public Transform[] GetAddsSpawnPoints() => addsSpawnPoints;

        public FacingDirection GetFacingDirection(Vector2 dir)
        {
            return movement.GetFacingDirection(dir);
        }

        public Vector2 GetFacingDirectionVector()
        {
            return movement.Direction;
        }

        public Transform GetCurrentCastPoint(Vector2 dir)
        {
            return GetFacingDirection(dir) switch
            {
                FacingDirection.N => castPointN,
                FacingDirection.NE => castPointNE,
                FacingDirection.E => castPointE,
                FacingDirection.SE => castPointSE,
                FacingDirection.S => castPointS,
                FacingDirection.SW => castPointSW,
                FacingDirection.W => castPointW,
                FacingDirection.NW => castPointNW,
                _ => castPointS
            };
        }

        public bool IsCastingAbility()
        {
            return context.currentAbility != null;



        }
    }
}
