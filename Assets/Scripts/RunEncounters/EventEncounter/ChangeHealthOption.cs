using UnityEngine;

namespace Game.RunEncounters
{
    /// <summary>Heals or hurts the player; reports the health actually healed or lost.</summary>
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/Change Health", fileName = "ChangeHealthOption")]
    public class ChangeHealthOption : EventOptionActionBase
    {
        public override EventResultKind Kind => EventResultKind.Health;

        public override EventActionResult Apply(EventContext context)
        {
            if (context.playerHealth == null)
            {
                Debug.LogError("[ChangeHealthOption] PlayerHealth missing from context.");
                return EventActionResult.Nothing(Kind);
            }

            float finalAmount = RollAmount(context);
            if (finalAmount >= 0)
            {
                float before = context.playerHealth.GetCurrentHealth();
                context.playerHealth.Heal(finalAmount);
                return EventActionResult.Of(Kind, context.playerHealth.GetCurrentHealth() - before);
            }

            // Events happen outside combat levels, where a death isn't handled, so the hit is
            // non-lethal. Report what was actually lost (after armor and the 1 HP floor).
            float damageTaken = context.playerHealth.TakeDamage(-finalAmount, nonLethal: true);
            return EventActionResult.Of(Kind, -damageTaken);
        }
    }

}
