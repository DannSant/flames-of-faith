using UnityEngine;

namespace Game.RunEncounters
{
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/Lose Health", fileName = "LoseHealthOption")]
    public class LoseHealthOption : EventOptionActionBase
    {
        [SerializeField] private float damage = 10f;

        public override void Apply(EventContext context)
        {
            if (context.playerHealth == null)
            {
                Debug.LogError("[LoseHealthOption] PlayerHealth missing from context.");
                return;
            }

            context.playerHealth.TakeDamage(damage);
        }
    }

}