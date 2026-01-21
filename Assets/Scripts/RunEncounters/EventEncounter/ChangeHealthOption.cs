using UnityEngine;

namespace Game.RunEncounters
{
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/Change Health", fileName = "ChangeHealthOption")]
    public class ChangeHealthOption : EventOptionActionBase
    {
        [SerializeField] private float amount = 10f;
        [SerializeField] private bool increase = true;

        public override void Apply(EventContext context)
        {
            if (context.playerHealth == null)
            {
                Debug.LogError("[ChangeHealthOption] PlayerHealth missing from context.");
                return;
            }
            if (increase)
            {
                context.playerHealth.Heal(amount);                
            }else
            {
                context.playerHealth.TakeDamage(amount);
            }
            
        }
    }

}