using Game.Control;
using UnityEngine;

namespace Game.Misc {
    public class DestoyOnCollision : MonoBehaviour
    {
        [SerializeField] private LayerMask obstacleLayer;
        private void OnTriggerEnter2D(Collider2D other)
        {   
            // Destroy if other has Obstacle Layer set
            if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
            {
                Destroy(gameObject);
                return;
            }

            if (other.GetComponent<PlayerController>()!=null)
            {
                Destroy(gameObject);
            }
        }
    }
}
