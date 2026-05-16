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

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform castPoint;
       

        [Header("Adds")]
        [Tooltip("If the boss summons any adds (additional minions) they will spawn from these transforms")]
        [SerializeField] private Transform[] addsSpawnPoints;
        
        private Transform player;
        private BossBehavior behavior;

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
            behavior.Initialize(this);
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

            abilityLoopRoutine = StartCoroutine(AbilityLoop(phaseOneRuntimes));
        }

        private void EnterPhaseTwo()
        {
            isPhaseOne = false;
            isPhaseTwo = true;

            if (abilityLoopRoutine != null)
            {
                StopCoroutine(abilityLoopRoutine);
            }

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
                    if (!ability.CanExecute(this,context))
                        continue;

                    if (selected == null || ability.GetBossAbility().priority > selected.GetBossAbility().priority)
                    {
                        selected = ability;
                    }
                }

                if (selected != null)
                {
                    yield return StartCoroutine(selected.Execute(
                        this,
                        context,
                        OnAbilityStarted,
                        OnAbilityEnded
                    ));
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
                animator.ResetTrigger(ability.initialAnimationName);
                animator.SetTrigger(ability.initialAnimationName);
            }
        }

        private void OnAbilityEnded(BossAbilityRuntime abilityRuntime)
        {
            
            var ability = abilityRuntime.GetBossAbility();           
            currentBlockFlags &= ~(int)ability.blocksWhileActive;
            //Debug.Log($"Ability ended: {ability.abilityName}, hasEndAnimation flags: {ability.hasEndAnimation}");
            if (ability.hasEndAnimation)
            {
                animator.ResetTrigger(ability.endAnimationName);
                animator.SetTrigger(ability.endAnimationName);
            }
            //OnAbilityFinished?.Invoke(abilityRuntime);
        }

        public bool HasBlockFlag(AbilityBlockFlags flag)
        {
            return ((AbilityBlockFlags)currentBlockFlags).HasFlag(flag);
        }

        // 🔹 Public API for abilities
        public Transform GetPlayer() => context.playerTransform;
        public Transform GetCastPoint() => castPoint;
        public Animator GetAnimator() => animator;

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
       

       
    }
}
