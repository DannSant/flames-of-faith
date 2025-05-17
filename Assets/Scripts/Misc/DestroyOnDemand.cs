using UnityEngine;

namespace Game.Misc
{
    public class DestroyOnDemand : MonoBehaviour
    {
        /**
         * Destroys the game object this script is attached to when called.
         * */
        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
