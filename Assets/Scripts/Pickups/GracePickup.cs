using Game.Combat;
using UnityEngine;

namespace Game.Pickups
{
    public class GracePickup : BasePickup
    {
        [SerializeField] private int graceAmount = 1;

        public override bool CanBePickedUp(GameObject picker)
        {
            var playerGrace = picker.GetComponent<PlayerGrace>();
            if (playerGrace.IsAtMaxGrace())
            {
                return false;
            }else
            {
                return true;
            }
        }

        public override void OnPickup(GameObject picker)
        {
            var playerGrace = picker.GetComponent<PlayerGrace>();
            if (playerGrace==null)
            {
                return;
            }

            if(playerGrace.IsAtMaxGrace())
            {
                return;
            }

            playerGrace.AddGrace(graceAmount);
            Destroy(gameObject);
        }
    }

}