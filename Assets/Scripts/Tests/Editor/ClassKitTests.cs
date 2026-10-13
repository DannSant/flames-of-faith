using Game.Combat;
using Game.Control;
using Game.Effects;
using Game.Effects.Abilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Guards each class prefab's starting kit. Ignored until the kit's ability items are authored.
    ///
    /// The "base tier matches" tests pin tier 0 to the values the class had before its abilities
    /// became items. They change on purpose when the base tiers get their new balance values.
    /// </summary>
    public class ClassKitTests
    {
        private const string WarriorPrefab = "Assets/Prefabs/Player/PlayerWarrior.prefab";
        private const string ArcherPrefab = "Assets/Prefabs/Player/PlayerArcher.prefab";

        private static GameObject LoadPrefab(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, $"Missing prefab {path}");
            return prefab;
        }

        private static List<Effect> GetStartingItems(GameObject prefab)
        {
            var store = prefab.GetComponent<EffectStore>();
            Assert.IsNotNull(store, $"{prefab.name} has no EffectStore");

            var list = new SerializedObject(store).FindProperty("startingEffects");
            var items = new List<Effect>();
            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue is Effect effect)
                {
                    items.Add(effect);
                }
            }
            return items;
        }

        private static List<AbilityBehavior> GetAbilities(List<Effect> items) =>
            items.SelectMany(e => e.Behaviors).OfType<AbilityBehavior>().ToList();

        /// <summary>The kit's abilities, or Assert.Ignore while none are authored.</summary>
        private static List<AbilityBehavior> GetAuthoredAbilities(GameObject prefab)
        {
            var abilities = GetAbilities(GetStartingItems(prefab));
            if (abilities.Count == 0)
            {
                Assert.Ignore($"{prefab.name}: no ability items in the starting kit yet.");
            }
            return abilities;
        }

        private static T GetAbility<T>(List<AbilityBehavior> abilities) where T : AbilityBehavior
        {
            var ability = abilities.OfType<T>().FirstOrDefault();
            Assert.IsNotNull(ability, $"The kit has no {typeof(T).Name}");
            return ability;
        }

        private static SerializedProperty GetSerializedProperty(Object owner, string field)
        {
            var property = new SerializedObject(owner).FindProperty(field);
            Assert.IsNotNull(property, $"{owner.name} has no field {field}");
            return property;
        }

        private static T GetSerializedReference<T>(Object owner, string field) where T : Object
        {
            var value = new SerializedObject(owner).FindProperty(field)?.objectReferenceValue as T;
            Assert.IsNotNull(value, $"{owner.name}.{field} is not set");
            return value;
        }

        // --- Every class -------------------------------------------------------------------

        [TestCase(WarriorPrefab)]
        [TestCase(ArcherPrefab)]
        public void Kit_HasExactlyOneAbilityPerSlot(string prefabPath)
        {
            var abilities = GetAuthoredAbilities(LoadPrefab(prefabPath));

            foreach (AbilitySlot slot in System.Enum.GetValues(typeof(AbilitySlot)))
            {
                int count = abilities.Count(a => a.Slot == slot);
                Assert.AreEqual(1, count, $"{prefabPath}: expected one {slot} ability, found {count}");
            }
        }

        [TestCase(WarriorPrefab)]
        [TestCase(ArcherPrefab)]
        public void Kit_ItemsAreStartingOnly(string prefabPath)
        {
            var prefab = LoadPrefab(prefabPath);
            GetAuthoredAbilities(prefab);

            foreach (var item in GetStartingItems(prefab))
            {
                Assert.IsTrue(item.IsStartingOnly, $"{prefab.name}: kit item {item.name} isn't flagged Starting Only");
            }
        }

        [TestCase(WarriorPrefab)]
        [TestCase(ArcherPrefab)]
        public void Kit_AbilityTablesHaveABaseTier(string prefabPath)
        {
            foreach (var ability in GetAuthoredAbilities(LoadPrefab(prefabPath)))
            {
                Assert.Greater(ability.TierCount, 0, $"{ability.name} has an empty tier table");
            }
        }

        // --- Warrior -----------------------------------------------------------------------

        [Test]
        public void Warrior_BaseTierMatchesTheSword()
        {
            var prefab = LoadPrefab(WarriorPrefab);
            var abilities = GetAuthoredAbilities(prefab);
            var sword = prefab.GetComponentInChildren<SwordWeapon>(true);
            Assert.IsNotNull(sword, "PlayerWarrior has no SwordWeapon");

            var weaponData = GetSerializedReference<WeaponData>(sword, "weaponData");
            var specialData = GetSerializedReference<WeaponData>(sword, "specialWeaponData");

            var strike = GetAbility<CrusaderStrikeAbility>(abilities).GetTier(0);
            Assert.AreEqual(weaponData.baseDamage, strike.baseDamage, "Crusader Strike base damage");
            Assert.AreEqual(0f, strike.bleedChanceBonus, "Crusader Strike base bleed bonus");

            var whirlwind = GetAbility<WhirlwindAbility>(abilities).GetTier(0);
            Assert.AreEqual(specialData.baseDamage, whirlwind.baseDamage, "Whirlwind base damage");
        }

        [Test]
        public void Warrior_BaseTierMatchesTheDashHitbox()
        {
            var prefab = LoadPrefab(WarriorPrefab);
            var abilities = GetAuthoredAbilities(prefab);
            var dash = prefab.GetComponent<WarriorDash>();
            Assert.IsNotNull(dash, "PlayerWarrior has no WarriorDash");

            var hitbox = GetSerializedReference<GameObject>(dash, "dashHitbox").GetComponent<DamageSourceBase>();
            Assert.IsNotNull(hitbox, "The dash hitbox has no DamageSourceBase");

            var charge = GetAbility<CrusaderChargeAbility>(abilities).GetTier(0);
            Assert.AreEqual(hitbox.BaseDamage, charge.damage, "Crusader Charge base damage");
            Assert.AreEqual(hitbox.KnockbackForce, charge.knockbackForce, 0.0001f, "Crusader Charge base knockback");
        }

        // --- Archer ------------------------------------------------------------------------

        [Test]
        public void Archer_BaseTierMatchesTheBow()
        {
            var prefab = LoadPrefab(ArcherPrefab);
            var abilities = GetAuthoredAbilities(prefab);
            var bow = prefab.GetComponentInChildren<BowWeapon>(true);
            Assert.IsNotNull(bow, "PlayerArcher has no BowWeapon");

            var weaponData = GetSerializedReference<WeaponData>(bow, "weaponData");
            var specialData = GetSerializedReference<WeaponData>(bow, "specialWeaponData");

            var holyShot = GetAbility<HolyShotAbility>(abilities).GetTier(0);
            Assert.AreEqual(weaponData.baseDamage, holyShot.baseDamage, "Holy Shot base damage");
            Assert.AreEqual(0, holyShot.extraArrows, "Holy Shot base extra arrows");

            var barrage = GetAbility<BarrageAbility>(abilities).GetTier(0);
            Assert.AreEqual(specialData.baseDamage, barrage.baseDamage, "Barrage base damage");
            Assert.AreEqual(specialData.projectileAmount, barrage.ringArrowCount, "Barrage ring arrows");
            Assert.AreEqual(GetSerializedProperty(bow, "barrageArrowCount").intValue, barrage.targetArrowCount, "Barrage target arrows");
        }

        [Test]
        public void Archer_BaseTierMatchesTheDash()
        {
            var prefab = LoadPrefab(ArcherPrefab);
            var abilities = GetAuthoredAbilities(prefab);
            var dash = prefab.GetComponent<ArcherDash>();
            Assert.IsNotNull(dash, "PlayerArcher has no ArcherDash");

            var trap = GetSerializedReference<GameObject>(dash, "trapPrefab").GetComponentInChildren<DamageSourceBase>(true);
            Assert.IsNotNull(trap, "The trap prefab has no DamageSourceBase");

            var trapStep = GetAbility<TrapStepAbility>(abilities).GetTier(0);
            Assert.AreEqual(GetSerializedProperty(dash, "dashDuration").floatValue, trapStep.dashDuration, 0.0001f, "Trap Step dash duration");
            Assert.AreEqual(trap.BaseDamage, trapStep.trapDamage, "Trap Step trap damage");
            Assert.AreEqual(GetSerializedProperty(dash, "maxActiveTraps").intValue, trapStep.maxTraps, "Trap Step max traps");
        }
    }
}
