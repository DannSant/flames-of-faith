using Game.Effects;
using UnityEngine;

namespace Game.Combat.Projectiles
{
    public interface IEffectStackModifier
    {
        void ModifyEffect(int stackCount, EffectStackBehavior effectStackBehavior);
    }
}