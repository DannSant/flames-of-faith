using UnityEngine;

namespace Game.Misc
{
    public class SelfRotate : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 360f; // degrees per second

        void Update()
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }
}
