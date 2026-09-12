using System;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Anything that can report "I just dealt damage to this target". Implemented by
    /// <see cref="DamageSourceBase"/> for direct hits and by <see cref="AreaDamageOnHit"/> for each
    /// secondary target, so an on-hit side effect can listen to direct hits, propagated hits, or both
    /// without knowing which kind of source it is sitting next to.
    /// </summary>
    public interface IDamageDealtNotifier
    {
        event Action<float, GameObject> OnDamageDealtEvent;
    }
}
