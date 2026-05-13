using UnityEngine;

namespace Game.Boss
{
    public class BossAdd : MonoBehaviour
    {
        private BossController ownerBoss;

        public void Initialize(BossController boss)
        {
            ownerBoss = boss;
        }

        private void OnDestroy()
        {
            if (ownerBoss != null)
            {
                ownerBoss.NotifyAddDied(gameObject);
            }
        }
    }

}