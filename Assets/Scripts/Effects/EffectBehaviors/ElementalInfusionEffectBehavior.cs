using Game.Combat.Elemental;
using Game.Combat;
using UnityEngine;

namespace Game.Effects.EffectBehaviors
{
    public enum ElementalCondition
    {
        Always,
        OnlyWeaponDamage,
        OnlyEffectDamage,
        OnlyMelee,
        OnlyRanged,
        OnlyMagic,
        WeaponAndMelee,
        WeaponAndRanged,
        WeaponAndMagic,
        EffectAndMelee,
        EffectAndRanged,
        EffectAndMagic
    }

    [System.Serializable]
    public struct ElementalMetadata
    {
        public DamageOriginType origin;
        public WeaponClass weaponClass;
    }

    [CreateAssetMenu(menuName = "Effects/Behaviors/Elemental Infusion",
    fileName = "ElementalInfusionBehavior")]
    public class ElementalInfusionEffectBehavior : EffectBehavior
    {
        [Header("Element to Apply")]
        public ElementalType element = ElementalType.None;

        [Header("When Should It Apply?")]
        public ElementalCondition condition = ElementalCondition.Always;

        public ElementalDebuffData elementalDebuffData;

        public override void Initialize(GameObject owner, EffectStore store, Effect parentEffect)
        {
            base.Initialize(owner, store, parentEffect);
            // No setup needed (effect is passive until queried)
        }

        /// <summary>
        /// Determines whether this elemental infusion should apply
        /// given the metadata of the attack.
        /// </summary>
        public bool ShouldApply(ElementalMetadata meta)
        {
            switch (condition)
            {
                case ElementalCondition.Always:
                    return true;

                case ElementalCondition.OnlyWeaponDamage:
                    return meta.origin == DamageOriginType.Weapon;

                case ElementalCondition.OnlyEffectDamage:
                    return meta.origin == DamageOriginType.Effect;

                case ElementalCondition.OnlyMelee:
                    return meta.weaponClass == WeaponClass.Melee;

                case ElementalCondition.OnlyRanged:
                    return meta.weaponClass == WeaponClass.Ranged;

                case ElementalCondition.OnlyMagic:
                    return meta.weaponClass == WeaponClass.Magic;
                case ElementalCondition.WeaponAndMelee:
                    return meta.origin == DamageOriginType.Weapon && meta.weaponClass == WeaponClass.Melee;
                case ElementalCondition.WeaponAndRanged:
                    return meta.origin == DamageOriginType.Weapon && meta.weaponClass == WeaponClass.Ranged;
                case ElementalCondition.WeaponAndMagic:
                    return meta.origin == DamageOriginType.Weapon && meta.weaponClass == WeaponClass.Magic;
                case ElementalCondition.EffectAndMelee:
                    return meta.origin == DamageOriginType.Effect && meta.weaponClass == WeaponClass.Melee;
                case ElementalCondition.EffectAndRanged:
                    return meta.origin == DamageOriginType.Effect && meta.weaponClass == WeaponClass.Ranged;
                case ElementalCondition.EffectAndMagic:
                    return meta.origin == DamageOriginType.Effect && meta.weaponClass == WeaponClass.Magic;
                default:
                    return false;
            }
        }
    }

}