using UnityEngine;

namespace Game.Combat.Elemental
{
    public enum ElementalType
    {
        None = 0,
        Fire = 1, // Causes burn damage over time
        Frost = 2, // Slows down the target's movement speed and makes the target receive more damage from attacks
        Chaos = 3, // On damage or death, there is chance that the target will cause damage to nearby enemies and will spread the chaos effect to them. If they are already affected by chaos, they will also cause damage and spread the effect.
        LeyCharge = 4, // When the affected target receives damage, also inflict damage to the nearest enemy and applies the debuff to them.
        Holy = 5 // Chance to stun the target for a short duration, making them unable to move or attack. When this effect is triggered, the player will heal for a small amount of health and 1 Grace point.
    }
}
