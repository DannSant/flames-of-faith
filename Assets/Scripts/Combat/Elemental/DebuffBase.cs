using UnityEngine;

namespace Game.Combat.Elemental
{
    public abstract class DebuffBase : MonoBehaviour
    {
        private const float MinimumSpreadDuration = 0.1f; // below this a spread copy is not worth creating

        protected EnemyHealth enemyHealth;
        protected DebuffHandler handler;

        protected ElementalDebuffData data;
        protected float duration;
        protected float strength; // how strong is this effect
        protected int generation; // how many contact spreads away this instance is from the original application
        protected int stacks; // always at least 1; only grows for debuffs whose data enables stacking

        private GameObject vfxInstance;
        private float nextSpreadTime;
        private bool expired;

        public ElementalType ElementalType => data != null ? data.ElementalType : ElementalType.None;

        protected virtual void Awake()
        {
            enemyHealth = GetComponent<EnemyHealth>();
            handler = GetComponent<DebuffHandler>();
        }

        /// <summary>
        /// Entry point for the debuff applicator: called right after AddComponent(), and again
        /// every time the same debuff is applied to an already-affected target.
        /// </summary>
        public void Apply(ElementalDebuffData data, float duration, float strength, int generation)
        {
            bool firstApplication = this.data == null;

            this.data = data;
            this.strength = strength;

            // A direct application from the player arrives as generation 0 and resets the counter.
            // A propagated application never walks the counter backwards.
            this.generation = generation == 0 ? 0 : Mathf.Max(this.generation, generation);

            if (firstApplication)
            {
                this.duration = duration;
                stacks = 1;
                SpawnVfx();
                OnApplied();
                return;
            }

            OnReapplied(duration);
        }

        /// <summary>
        /// Called once, when the debuff first lands on the target.
        /// </summary>
        protected virtual void OnApplied() { }

        /// <summary>
        /// Called when the debuff is applied again to an already-affected target. The default is the
        /// generic, data-driven policy: stack if the data says so, otherwise refresh the duration.
        /// Subclasses need no stacking code of their own.
        /// </summary>
        protected virtual void OnReapplied(float newDuration)
        {
            DebuffStackData stacking = data.Stacking;

            if (!stacking.Enabled)
            {
                duration = newDuration; // classic refresh, no stacking
                return;
            }

            int previousStacks = stacks;
            stacks = Mathf.Min(stacks + 1, stacking.MaxStacks);

            // Only flash the stack VFX when a stack was actually gained, so a capped debuff goes quiet.
            if (stacks > previousStacks)
            {
                SpawnOneShotVfx(stacking.StackVfx, stacking.StackVfxLifetime);
            }

            if (stacking.RefreshDurationOnStack)
            {
                duration = newDuration;
            }
        }

        /// <summary>
        /// Called when the debuff is overwritten or ended.
        /// </summary>
        public virtual void End() { }

        /// <summary>
        /// Per-frame hook for subclasses. Only runs while the debuff is still alive.
        /// </summary>
        protected virtual void Tick(float deltaTime) { }

        protected virtual void Update()
        {
            if (expired) return;

            duration -= Time.deltaTime;
            if (duration <= 0f)
            {
                expired = true;
                End();
                if (handler != null) handler.NotifyDebuffEnded(this);
                Destroy(this);
                return;
            }

            Tick(Time.deltaTime);
        }

        private void SpawnVfx()
        {
            if (data == null || data.VFX == null) return;
            if (vfxInstance != null) return; // refreshing must not stack duplicated VFX

            vfxInstance = Instantiate(data.VFX, transform.position, Quaternion.identity, transform);
        }

        /// <summary>
        /// Spawns a fire-and-forget VFX that is NOT parented to the target, so it survives the target dying.
        /// The lifetime is explicit because these prefabs do not necessarily carry a DestroyAfterTime.
        /// </summary>
        protected void SpawnOneShotVfx(GameObject prefab, float lifetime)
        {
            if (prefab == null) return;

            GameObject instance = Instantiate(prefab, transform.position, Quaternion.identity);
            Destroy(instance, lifetime);
        }

        protected virtual void OnDestroy()
        {
            if (vfxInstance != null) Destroy(vfxInstance);
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            TryContactSpread(collision);
        }

        /// <summary>
        /// Generic, data-driven contact propagation. Subclasses need no code of their own for this:
        /// a debuff spreads if (and only if) its <see cref="ElementalDebuffData"/> enables it.
        /// </summary>
        private void TryContactSpread(Collision2D collision)
        {
            if (expired || data == null) return;

            DebuffContactSpreadData spread = data.ContactSpread;
            if (!spread.Enabled) return;
            if (generation >= spread.MaxGenerations) return;
            if (enemyHealth == null || enemyHealth.IsDead()) return;

            // Consume the cooldown before rolling, so a sustained contact cannot reroll every physics frame.
            if (Time.time < nextSpreadTime) return;
            nextSpreadTime = Time.time + spread.Cooldown;

            if (Random.value > spread.Chance) return;

            float spreadDuration = duration * spread.DurationFalloff;
            if (spreadDuration < MinimumSpreadDuration) return;

            DebuffHandler target = ResolveHandler(collision);
            if (target == null || target.gameObject == gameObject) return;

            target.ApplyDebuff(data, spreadDuration, strength * spread.StrengthFalloff, generation + 1);
        }

        /// <summary>
        /// Enemy variants such as EnemyLightDevourer wrap EnemyBase as a child, so the colliding GameObject
        /// is not necessarily the one holding the DebuffHandler.
        /// </summary>
        private static DebuffHandler ResolveHandler(Collision2D collision)
        {
            if (collision.rigidbody != null && collision.rigidbody.TryGetComponent(out DebuffHandler byBody))
            {
                return byBody;
            }

            return collision.collider != null ? collision.collider.GetComponentInParent<DebuffHandler>() : null;
        }
    }

}
