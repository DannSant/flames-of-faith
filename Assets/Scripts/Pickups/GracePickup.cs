using Game.Combat;
using UnityEngine;

namespace Game.Pickups
{
    public class GracePickup : BasePickup
    {
        [SerializeField] private int graceAmount = 1;
        public override void OnPickup(GameObject picker)
        {
            var playerGrace = picker.GetComponent<PlayerGrace>();
            if (playerGrace==null)
            {
                return;
            }

            if(playerGrace.CurrentGrace >= playerGrace.MaxGrace)
            {
                return;
            }

            playerGrace.AddGrace(graceAmount);
            Destroy(gameObject);
        }
    }

}