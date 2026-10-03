using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Marks a GameObject (a Corrupted enemy, the boss, or a projectile/explosion they spawned) as a source of
    /// Corrupted Damage. Damage dealt to the player only reports the attacker GameObject, so the flag travels on it.
    /// </summary>
    public class CorruptedDamageSource : MonoBehaviour
    {
        public static void Mark(GameObject target)
        {
            if (target != null && !target.TryGetComponent<CorruptedDamageSource>(out _))
            {
                target.AddComponent<CorruptedDamageSource>();
            }
        }

        // Checks parents too, so hitboxes that are children of a Corrupted enemy count as well
        public static bool IsCorrupted(GameObject source)
        {
            return source != null && source.GetComponentInParent<CorruptedDamageSource>() != null;
        }
    }
}
