using UnityEngine;

namespace Game.AI.Behaviors
{
    [CreateAssetMenu(menuName = "Behaviors/Flip Toward Player")]
    public class FlipTowardPlayerBehavior : AIUpdateBehavior
    {
        [Tooltip("If true, flip opposite (useful for weird pivots)")]
        [SerializeField] private bool invert = false; 
        public override void Tick(BehaviorContext context)
        {
            if (context.playerTransform == null || context.enemyTransform == null) return;
            var spriteTransform = context.enemyTransform;

            float dx = context.playerTransform.position.x - context.enemyTransform.position.x;
            bool shouldFlip = dx < 0;

            Vector3 scale = spriteTransform.localScale;
            scale.x = Mathf.Abs(scale.x) * (shouldFlip ^ invert ? -1 : 1);
            spriteTransform.localScale = scale;
        }
    }

}