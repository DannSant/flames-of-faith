using Game.Combat;
using Game.Control;
using Game.Effects;
using Game.Effects.Abilities;
using Game.Effects.EffectBehaviors;
using Game.Metaprogression;
using Game.Progression;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Guards each class prefab's starting kit: 3 ability scrolls (one per slot), an Emblem, all
    /// starting-only. A missing piece fails the test; it never skips.
    ///
    /// The "base tier matches" tests pin tier 0 to the values the class had before its abilities
    /// became items. They change on purpose when the base tiers get their new balance values.
    /// </summary>
    public class ClassKitTests
    {
        private const string WarriorPrefab = "Assets/Prefabs/Player/PlayerWarrior.prefab";
        private const string ArcherPrefab = "Assets/Prefabs/Player/PlayerArcher.prefab";
        private const string AugurPrefab = "Assets/Prefabs/Player/PlayerAugur.prefab";

        private static GameObject LoadPrefab(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, $"Missing prefab {path}");
            return prefab;
        }

        /// <summary>The class's base kit: tier 0 of every node of its skill tree.</summary>
        private static List<Effect> GetStartingItems(GameObject prefab)
        {
            var classManager = prefab.GetComponentInChildren<PlayerClassManager>(true);
            Assert.IsNotNull(classManager, $"{prefab.name} has no PlayerClassManager");

            var tree = Resources.LoadAll<SkillTreeDefinition>(SkillTrees.ResourcesPath)
                .FirstOrDefault(t => t.CharacterClass == classManager.CharacterClass);
            Assert.IsNotNull(tree, $"No skill tree for {classManager.CharacterClass} ({prefab.name})");

            return tree.Nodes.Select(n => n.GetTier(0)).Where(e => e != null).ToList();
        }

        private static List<AbilityBehavior> GetAbilities(List<Effect> items) =>
            items.SelectMany(e => e.Behaviors).OfType<AbilityBehavior>().ToList();

        /// <summary>The kit's abilities; fails when the kit has none.</summary>
        private static List<AbilityBehavior> GetAuthoredAbilities(GameObject prefab)
        {
            var abilities = GetAbilities(GetStartingItems(prefab));
            Assert.IsNotEmpty(abilities, $"{prefab.name}: no ability items in the starting kit");
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
        [TestCase(AugurPrefab)]
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
        [TestCase(AugurPrefab)]
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
        [TestCase(AugurPrefab)]
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

        // --- Augur -------------------------------------------------------------------------

        [Test]
        public void Augur_BaseTierMatchesTheScepter()
        {
            var prefab = LoadPrefab(AugurPrefab);
            var abilities = GetAuthoredAbilities(prefab);
            var scepter = prefab.GetComponentInChildren<ScepterWeapon>(true);
            Assert.IsNotNull(scepter, "PlayerAugur has no ScepterWeapon");

            var weaponData = GetSerializedReference<WeaponData>(scepter, "weaponData");
            var specialData = GetSerializedReference<WeaponData>(scepter, "specialWeaponData");

            var fireball = GetAbility<FireballAbility>(abilities).GetTier(0);
            Assert.AreEqual(weaponData.baseDamage, fireball.baseDamage, "Fireball base damage");
            Assert.AreEqual(0, fireball.impactExplosionDamage, "Fireball base impact explosion");

            var tornadoes = GetAbility<FireTornadoesAbility>(abilities).GetTier(0);
            Assert.AreEqual(specialData.baseDamage, tornadoes.baseDamage, "Fire Tornadoes base damage");
            // The scepter used to spawn exactly 4 (N/E/S/W).
            Assert.AreEqual(4, tornadoes.tornadoCount, "Fire Tornadoes base count");
        }

        [Test]
        public void Augur_BaseTierMatchesTheDash()
        {
            var prefab = LoadPrefab(AugurPrefab);
            var abilities = GetAuthoredAbilities(prefab);
            var dash = prefab.GetComponent<AugurDash>();
            Assert.IsNotNull(dash, "PlayerAugur has no AugurDash");

            var flame = GetSerializedReference<GameObject>(dash, "flameTrailSegmentPrefab").GetComponentInChildren<DamageSourceBase>(true);
            Assert.IsNotNull(flame, "The flame prefab has no DamageSourceBase");

            var flameDash = GetAbility<FlameDashAbility>(abilities).GetTier(0);
            Assert.AreEqual(GetSerializedProperty(dash, "dashDuration").floatValue, flameDash.dashDuration, 0.0001f, "Flame Dash dash duration");
            Assert.AreEqual(GetSerializedProperty(dash, "maxCharges").intValue, flameDash.maxCharges, "Flame Dash charges");
            Assert.AreEqual(GetSerializedProperty(dash, "trailSpawnInterval").floatValue, flameDash.flameSpawnInterval, 0.0001f, "Flame Dash flame interval");
            Assert.AreEqual(flame.BaseDamage, flameDash.flameDamage, "Flame Dash flame damage");
        }

        // --- Emblems -----------------------------------------------------------------------

        [TestCase(WarriorPrefab, StatType.MeleeDamage)]
        [TestCase(ArcherPrefab, StatType.RangedDamage)]
        [TestCase(AugurPrefab, StatType.MagicDamage)]
        public void Kit_HasOneEmblemGrantingTheMainStat(string prefabPath, StatType mainStat)
        {
            var items = GetStartingItems(LoadPrefab(prefabPath));
            var levelUps = items.SelectMany(e => e.Behaviors).OfType<MainStatOnLevelUpBehavior>().ToList();
            Assert.AreEqual(1, levelUps.Count, $"{prefabPath}: expected exactly one Emblem (level-up stat behavior), found {levelUps.Count}");
            var stat = (StatType)GetSerializedProperty(levelUps[0], "stat").enumValueIndex;
            Assert.AreEqual(mainStat, stat, $"{prefabPath}: the Emblem grants the wrong stat");
            Assert.AreEqual(1, GetSerializedProperty(levelUps[0], "levelsPerPoint").intValue, $"{prefabPath}: +1 every level");
        }

        [TestCase(WarriorPrefab)]
        [TestCase(ArcherPrefab)]
        [TestCase(AugurPrefab)]
        public void Kit_StatBonusRulesLiveOnTheEmblemOnly(string prefabPath)
        {
            var items = GetStartingItems(LoadPrefab(prefabPath));
            var emblem = items.FirstOrDefault(e => e.Behaviors.OfType<MainStatOnLevelUpBehavior>().Any());
            Assert.IsNotNull(emblem, $"{prefabPath}: no Emblem in the starting kit");

            Assert.IsTrue(emblem.HasStatDependencies, $"{emblem.name} has no stat bonus rules");
            foreach (var item in items.Where(e => e != emblem))
            {
                Assert.IsFalse(item.HasStatDependencies, $"{item.name} still has stat bonus rules (they belong on {emblem.name})");
            }
        }

        // --- Main stat points (pure) -------------------------------------------------------

        [TestCase(1, 2, 1, 1)]
        [TestCase(1, 4, 1, 3)]
        [TestCase(1, 2, 2, 1)]
        [TestCase(2, 3, 2, 0)]
        [TestCase(3, 7, 2, 2)]
        [TestCase(5, 5, 1, 0)]
        [TestCase(5, 1, 1, 0)]
        [TestCase(1, 5, 0, 0)]
        public void MainStatPoints_CountByLevelNumber(int previous, int next, int perPoint, int expected)
        {
            Assert.AreEqual(expected, StatsCalculations.CalculateMainStatPointsEarned(previous, next, perPoint));
        }
    }
}
