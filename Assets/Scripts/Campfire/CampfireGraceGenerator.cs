using Game.Combat;
using UnityEngine;

namespace Game.Campfire
{
    public class CampfireGraceGenerator : MonoBehaviour
    {
        [SerializeField] private int graceAmount = 3;
        private bool isActive = true;

        public event System.Action<bool> onPlayerEntersCampfire;
        public int GraceAmount { get { return graceAmount; } }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isActive) return;
            var playerGrace = collision.GetComponent<PlayerGrace>();
            if (playerGrace == null)
            {
                return;
            }

            if (playerGrace.CurrentGrace >= playerGrace.MaxGrace)
            {
                return;
            }
            isActive = false;
            onPlayerEntersCampfire?.Invoke(true);
            playerGrace.AddGrace(graceAmount);
        }

    }

}