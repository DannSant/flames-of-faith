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
        }

        private void Start()
        {
            player = PlayerManager.Instance.gameObject.transform;

        }

        private void Update()
        {
            if (context == null) return;

            context.phaseTime += Time.deltaTime;
            context.enrageLevel = enrageLevel; // keep synced
            context.activeAddsCount = activeAdds.Count;

        }

        private void CreateContext()
        {
            context = new BossAbilityContext
            {
                bossTransform = transform,
                playerTransform = FindPlayer(),
                coroutineRunner = this,
                phaseTime = 0f,
                enrageLevel = 0
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

            abilityLoopRoutine = StartCoroutine(AbilityLoop(phaseOneRuntimes));
        }

        private void EnterPhaseTwo()
        {
            isPhaseOne = false;
            isPhaseTwo = true;          

            //Remove immunity flag for phase 2
            health.IsImmuneFlag = false;

            if (abilityLoopRoutine != null)
            {
                StopCoroutine(abilityLoopRoutine);
            }

            StartCoroutine(PhaseOneToTwoTransition());

        }

        private IEnumerator PhaseOneToTwoTransition()
        {
            bossRenderer.TriggerAnimation(behavior.GetPhaseTransitionAnimationName());
            yield return new WaitForSeconds(2f);
            abilityLoopRoutine = StartCoroutine(AbilityLoop(phaseTwoRuntimes));

            // Remove immunity flag for phase 2 after transition animation
            health.IsImmuneFlag = false;

            behavior.OnPhaseTwoStart();
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
                        continue;

                    if (selected == null || ability.GetBossAbility().priority > selected.GetBossAbility().priority)
                    {
                        selected = ability;
                    }
                }

                if (selected != null)
                {
                    context.currentAbility = selected.GetBossAbility();
                    movement.SetCasting(true);
                    yield return StartCoroutine(selected.Execute(
                        this,
                        context,
                        OnAbilityStarted,
                        OnAbilityEnded
                    ));
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
            return ((AbilityBlockFlags)currentBlockFlags).HasFlag(flag);
        }

        // 🔹 Public API for abilities
        public Transform GetPlayer() => context.playerTransform;

        public BossRenderer GetBossRenderer() => bossRenderer;

        public int GetEnrageLevel() => enrageLevel;

        public void IncreaseEnrageLevel()
        {
            enrageLevel++;
            Debug.Log($"Boss enraged! Level: {enrageLevel}");
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
