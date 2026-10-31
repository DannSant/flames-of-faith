using System.Linq;
using Game.Combat;
using Game.Control;
using Game.Effects.Abilities;
using Game.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The real assets at the hard cap: a primary still lands inside its cooldown, and a special's cooldown
    /// outlasts its action, because specials keep speed 1. A value failing here is a balance question.
    /// </summary>
    public class AttackSpeedHardCapTests
    {
        private const string ArcherPrefab = "Assets/Prefabs/Player/PlayerArcher.prefab";

        private static PlayerStatSettings Settings => Resources.Load<PlayerStatSettings>("Progression/PlayerStatSettings");

        private static WeaponData Weapon(string name)
        {
            var weapon = Resources.Load<WeaponData>("Weapons/" + name);
            Assert.IsNotNull(weapon, $"Missing Resources/Weapons/{name}");
            return weapon;
        }

        private static float CooldownAtHardCap(WeaponData weapon) =>
            Settings.GetAttackDelay(Settings.AttackSpeedHardCap, weapon.attackCooldownBase, weapon.attackSpeedScale);

        private static AnimatorController Controller(string className)
        {
            var data = Resources.Load<CharacterClassData>("CharacterData/" + className);
            Assert.IsNotNull(data, $"Missing Resources/CharacterData/{className}");
            return (AnimatorController)data.animatorController;
        }

        private static T Ability<T>() where T : ScriptableObject
        {
            var guid = AssetDatabase.FindAssets("t:" + typeof(T).Name).FirstOrDefault();
            Assert.IsNotNull(guid, $"No {typeof(T).Name} asset");
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }

        private static float SpecialClipLength(string className)
        {
            var state = AttackAnimationSpeedTests.StateTriggeredBy(Controller(className), "SpecialAttack");
            return Clips(state.motion).Max(c => c.length);
        }

        private static AnimationClip[] Clips(Motion motion) => motion switch
        {
            AnimationClip clip => new[] { clip },
            BlendTree tree => tree.children.SelectMany(c => Clips(c.motion)).ToArray(),
            _ => new AnimationClip[0],
        };

        // Seconds from the start of the primary attack to its shot or hit, at animation speed 1
        private static float TimeToHit(AnimatorState state)
        {
            var clips = Clips(state.motion);
            var bow = state.behaviours.OfType<BowAttackAnimBehavior>().FirstOrDefault();
            if (bow != null)
            {
                return new SerializedObject(bow).FindProperty("attackDelay").floatValue * clips.Max(c => c.length);
            }
            var starts = clips.SelectMany(c => c.events).Where(e => e.functionName == "AttackStartAnimEvent").ToList();
            Assert.IsNotEmpty(starts, $"{state.name}: nothing starts the hit");
            return starts.Max(e => e.time);
        }

        [TestCase("Warrior", "Sword")]
        [TestCase("Archer", "Bow")]
        [TestCase("Augur", "Scepter")]
        public void Primary_LandsInsideItsCooldown(string className, string weaponName)
        {
            var weapon = Weapon(weaponName);
            float cooldown = CooldownAtHardCap(weapon);
            float speed = StatsCalculations.CalculateAttackAnimationSpeed(weapon.attackCooldownBase, cooldown);
            var state = AttackAnimationSpeedTests.StateTriggeredBy(Controller(className), "Attack");

            Assert.Greater(cooldown, TimeToHit(state) / speed, $"{className}: the next attack would cut the hit off");
        }

        [Test]
        public void Whirlwind_CooldownOutlastsItsSpin()
        {
            float spin = Ability<WhirlwindAbility>().Tiers.Max(t => t.spinDuration);

            Assert.Greater(CooldownAtHardCap(Weapon("SwordSpecial")), spin);
        }

        [Test]
        public void Barrage_CooldownOutlastsItsVolley()
        {
            var bow = AssetDatabase.LoadAssetAtPath<GameObject>(ArcherPrefab).GetComponentInChildren<BowWeapon>(true);
            Assert.IsNotNull(bow, $"No BowWeapon on {ArcherPrefab}");
            float interval = new SerializedObject(bow).FindProperty("barrageInterval").floatValue;
            int arrows = Ability<BarrageAbility>().Tiers.Max(t => t.targetArrowCount);

            Assert.Greater(CooldownAtHardCap(Weapon("BowSpecial")), SpecialClipLength("Archer") + arrows * interval);
        }

        [Test]
        public void FireTornadoes_CooldownOutlastsTheCast()
        {
            Assert.Greater(CooldownAtHardCap(Weapon("ScepterSpecial")), SpecialClipLength("Augur"));
        }
    }
}
