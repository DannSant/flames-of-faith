using Game.Scene;
using UnityEngine;

namespace Game.Misc
{
    public class RotateTowardsPlayer : MonoBehaviour
    {
        Transform playerTransform;
        private void Start()
        {
            playerTransform = PlayerManager.Instance.CurrentPlayer.transform;

            RotateToFacePlayer();
        }

        private void RotateToFacePlayer()
        {
            if(playerTransform == null) return;
            Vector2 direction = playerTransform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + 180f);

        }
    }

}