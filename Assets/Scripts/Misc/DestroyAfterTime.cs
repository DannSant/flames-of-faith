using UnityEngine;

namespace Game.Misc
{
    public class DestroyAfterTime : MonoBehaviour
    {
        [SerializeField] private float timeToDestroy = 1f;

        private void Start()
        {
            // Destroy the GameObject after the specified time
            Destroy(gameObject, timeToDestroy);
        }
    }
}
