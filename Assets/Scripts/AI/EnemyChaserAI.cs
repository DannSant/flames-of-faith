using Game.AI;
using Game.Control;
using Game.Misc;
using Game.Scene;
using UnityEngine;

public class EnemyChaserAI : EnemyAIBase
{
    private Rigidbody2D rb;
    private float speed = 1f;
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
        //player = PlayerController.Instance.transform;
        player = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
    }

    protected override void Update()
    {
        base.Update();
        // Movement is handled in FixedUpdate for physics
    }

    private void FixedUpdate()
    {
        if (player == null) return;
        if (knockback.IsKnockbacked)
        {
            return;
        }

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        Vector2 nextPosition = rb.position + direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);
    }

    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Max(0f, newSpeed);
    }
    public override void SetProjectileDamage(int amount)
    { }
}
