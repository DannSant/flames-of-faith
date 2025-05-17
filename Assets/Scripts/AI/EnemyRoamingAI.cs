using Game.Misc;
using System.Collections;
using UnityEngine;

namespace Game.AI
{
    public class EnemyRoamingAI : EnemyAIBase
    {
        [SerializeField] private float directionChangeInterval = 2f;

        private Rigidbody2D rb;
        private Vector2 moveDirection;
        private float speed = 3f;        
        private float directionChangeTimer;
        private Knockback knockback;

        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody2D>();
            knockback = GetComponent<Knockback>();
        }

        protected override void Start()
        {
            base.Start();
            PickNewDirection();
            directionChangeTimer = directionChangeInterval;
        }

        protected override void Update()
        {
            base.Update();
            directionChangeTimer -= Time.deltaTime;
            if (directionChangeTimer <= 0f)
            {
                PickNewDirection();
                directionChangeTimer = directionChangeInterval;
            }
        }

        private void FixedUpdate()
        {
            if (knockback.IsKnockbacked)
            {
                return;
            }
            Vector2 nextPosition = rb.position + moveDirection * speed * Time.fixedDeltaTime;           
            rb.MovePosition(nextPosition);
        }

        private void PickNewDirection()
        {
            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            moveDirection = new Vector2(x, y).normalized;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Change direction on collision with anything (walls, player, etc.)
            PickNewDirection();
        }

        public void SetSpeed(float newSpeed)
        {
            speed = Mathf.Max(0f, newSpeed);
        }

        public override void SetProjectileDamage(int amount)
        {}
    }
}
