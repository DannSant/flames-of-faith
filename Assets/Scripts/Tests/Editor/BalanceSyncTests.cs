using Game.AI.Behaviors;
using Game.BalanceData;
using Game.Enemies;
using Game.Progression;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The pieces that let the balance importer run in builds (the designer's test build applies the sheet's numbers
    /// at startup): reflection field access must match what the Inspector shows, and a published snapshot of the
    /// current data must import as no change.
    /// </summary>
    public class BalanceSyncTests
    {
        private readonly List<Object> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in created) Object.DestroyImmediate(o);
            created.Clear();
        }

        private T Make<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            created.Add(asset);
            return asset;
        }

        // --- Pushes are split into small requests ----------------------------------------------------------------

        // Push All sent every tab in one request and timed out ("A task was canceled", 2026-10-25)
        [Test]
        public void Push_SplitsTabsIntoBatches_InOrder_WithNoneLost()
        {
            var tabs = Enumerable.Range(0, 58).ToList();
            var batches = Game.BalanceData.Editor.BalanceSheetSync.Batches(tabs, 6);

            Assert.AreEqual(10, batches.Count);
            Assert.IsTrue(batches.All(b => b.Length >= 1 && b.Length <= 6));
            CollectionAssert.AreEqual(tabs, batches.SelectMany(b => b));
        }

        [Test]
        public void Push_NothingToWrite_SendsNoRequest()
        {
            Assert.IsEmpty(Game.BalanceData.Editor.BalanceSheetSync.Batches(new int[0], 6));
        }

        // --- Field discovery and names match the Inspector ------------------------------------------------------

        [Test]
        public void DesignFields_AreTheInspectorsFieldsInItsOrder()
        {
            var types = BalanceTables.LoadAll<AIBehavior>(BalanceTables.BehaviorsFolder).Select(b => b.GetType()).Distinct().ToList();
            Assert.IsNotEmpty(types);
            foreach (var type in types)
            {
                var instance = (ScriptableObject)ScriptableObject.CreateInstance(type);
                created.Add(instance);
                var expected = new List<string>();
                var property = new SerializedObject(instance).GetIterator();
                property.NextVisible(true);
                do
                {
                    if (property.propertyPath == "m_Script" || property.propertyPath == "stopDuringGameplayFreeze" || property.propertyPath == "avoidObstacles") continue;
                    switch (property.propertyType)
                    {
                        case SerializedPropertyType.Integer:
                        case SerializedPropertyType.Float:
                        case SerializedPropertyType.Boolean:
                        case SerializedPropertyType.Enum:
                            expected.Add(property.propertyPath);
                            break;
                        case SerializedPropertyType.Vector2:
                            expected.Add(property.propertyPath + ".x");
                            expected.Add(property.propertyPath + ".y");
                            break;
                    }
                }
                while (property.NextVisible(false));

                CollectionAssert.AreEqual(expected, BalanceTables.DesignFields(type), type.Name);
            }
        }

        [Test]
        public void Headers_MatchTheInspectorsNames()
        {
            foreach (var definition in BalanceTables.All())
            {
                foreach (string column in definition.Columns)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(column), definition.Name);
                }
            }
            foreach (string name in new[] { "healthBase", "xpTier", "amountOfEnemiesWithLongCooldown", "evasionPerPointBeforeSoftCap",
                         "directElementalDebuffData", "m_SomeField", "_privateThing", "kConstantValue", "maxHP", "HPValue", "level2Boss", "vector3Thing" })
            {
                Assert.AreEqual(ObjectNames.NicifyVariableName(name), BalanceTableDefinition.Nicify(name), name);
            }
        }

        // --- Reflection reads and writes -------------------------------------------------------------------------

        [Test]
        public void FieldAccess_WritesAStructInsideAList()
        {
            var settings = Make<EnemyDamageSettings>();
            FieldAccess.Set(settings, "expectedCurve.Array.data[1].armor", 9f);

            Assert.AreEqual(9f, settings.ExpectedCurve[1].armor, "The struct in the list changed in place");
            Assert.AreEqual(9f, FieldAccess.Get(settings, "expectedCurve.Array.data[1].armor"));
        }

        [Test]
        public void FieldAccess_WritesAVectorComponent()
        {
            var flee = Make<AIBehaviorFleeLogic>();
            FieldAccess.Set(flee, "strafeFlipInterval.y", 7f);

            Assert.AreEqual(7f, FieldAccess.Get(flee, "strafeFlipInterval.y"));
            Assert.IsTrue(FieldAccess.TryDescribe(typeof(AIBehaviorFleeLogic), "strafeFlipInterval.x", out var type, out _));
            Assert.AreEqual(typeof(float), type);
        }

        [Test]
        public void FieldAccess_ResizeCopiesTheLastElement()
        {
            var settings = Make<ExperienceSettings>();
            int before = FieldAccess.Count(settings, "denominations");
            FieldAccess.Resize(settings, "denominations", before + 1);

            Assert.AreEqual(before + 1, FieldAccess.Count(settings, "denominations"));
            Assert.AreEqual(FieldAccess.Get(settings, $"denominations.Array.data[{before - 1}].multiplier"),
                FieldAccess.Get(settings, $"denominations.Array.data[{before}].multiplier"), "Like Unity growing an array");
            FieldAccess.Set(settings, $"denominations.Array.data[{before}].multiplier", 9f);
            Assert.AreNotEqual(9f, FieldAccess.Get(settings, $"denominations.Array.data[{before - 1}].multiplier"), "A copy, not the same object");
        }

        // --- A published snapshot ------------------------------------------------------------------------------

        [Test]
        public void SnapshotOfTheCurrentData_ImportsAsNoChange()
        {
            var snapshot = new BalanceSnapshot
            {
                version = 3,
                author = "designer",
                note = "nothing",
                tables = BalanceTables.All().Select(d => new BalanceSnapshotTable { name = d.Name, csv = Csv.Write(d.Export()) }).ToArray(),
            };
            var parsed = BalanceSnapshot.FromJson(JsonUtility.ToJson(snapshot));
            Assert.AreEqual(3, parsed.version);

            var errors = new List<string>();
            var plan = BalanceTables.Plan(parsed.ReadTables(errors), new RuntimeBalanceStore());
            Assert.IsEmpty(errors);
            Assert.IsEmpty(plan.Errors, string.Join("\n", plan.Errors));
            Assert.IsEmpty(plan.Changes, string.Join("\n", plan.Changes));
        }

        [Test]
        public void SnapshotMissingATable_IsAnError()
        {
            var tables = BalanceTables.All().Skip(1).ToDictionary(d => d.Name, d => d.Export());
            var plan = BalanceTables.Plan(tables, new RuntimeBalanceStore());
            Assert.That(plan.Errors, Has.Some.Contains("Enemies: the table is missing"));
        }

        [Test]
        public void ErrorAnswer_IsReadAsAnError()
        {
            var snapshot = BalanceSnapshot.FromJson("{\"error\":\"wrong or missing token\"}");
            Assert.AreEqual("wrong or missing token", snapshot.error);
        }

        [Test]
        public void LatestUrl_EscapesTheToken()
        {
            StringAssert.AreEqualIgnoringCase("https://x/exec?action=latest&token=a%2Bb", BalanceOverride.LatestUrl("https://x/exec", "a+b"));
        }
    }
}
