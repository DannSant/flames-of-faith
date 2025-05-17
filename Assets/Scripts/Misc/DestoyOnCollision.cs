using Game.Control;
using UnityEngine;

namespace Game.Misc {
    public class DestoyOnCollision : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>()!=null)
            {
                Destroy(gameObject);
            }
        }
    }
}
