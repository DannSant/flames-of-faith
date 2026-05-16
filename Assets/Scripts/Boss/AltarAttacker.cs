using UnityEngine;

namespace Game.Boss
{
    public class AltarAttacker : MonoBehaviour
    {
        [SerializeField] private float damageAmount = 10f;

        public float DamageAmount => damageAmount;
    }
}
