using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Boss
{
    public class BossSpawnPoint : MonoBehaviour
    {
        public void OnDrawGizmos()
        {

            Gizmos.DrawWireSphere(transform.position, 4f);
        }
    }

}