using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Game.BalanceData.Editor
{
    /// <summary>
    /// Tools/Flames of Faith/Balance Data: export the balance tables to CSV files in BalanceData/ (repo root, next to
    /// Database/), import them back (dry run first: every error is listed and nothing changes), and write the read-only
    /// reports. Each has a batchmode entry for -executeMethod.
    /// </summary>
    public static class BalanceDataTools
    {
        private const string Menu = "Tools/Flames of Faith/Balance Data/";
        private const int DialogLines = 20;

        public static string Folder => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "BalanceData");
        public static string ReportsFolder => Path.Combine(Folder, "Reports");

        private static readonly UTF8Encoding Utf8 = new(false);

        // --- Export ----------------------------------------------------------------------------------------

        [MenuItem(Menu + "Export CSV")]
        public static void ExportMenu()
        {
            int count = Export();
            EditorUtility.DisplayDialog("Balance data", $"Wrote {count} tables and the reports to {Folder}.", "OK");
        }

        /// <summary>Batchmode: -executeMethod Game.BalanceData.Editor.BalanceDataTools.ExportBatch</summary>
        public static void ExportBatch() => Export();

        public static int Export()
        {
            Directory.CreateDirectory(Folder);
            var tables = BalanceTables.All();
            foreach (var definition in tables)
            {
                WriteTable(Folder, definition.Export());
            }
            WriteReports();
            Debug.Log($"[BalanceData] Exported {tables.Count} tables to {Folder}");
            return tables.Count;
        }

        [MenuItem(Menu + "Write Reports")]
        public static void WriteReports()
        {
            Directory.CreateDirectory(ReportsFolder);
            foreach (var report in BalanceReports.All())
            {
                WriteTable(ReportsFolder, report);
            }
            Debug.Log($"[BalanceData] Wrote the reports to {ReportsFolder}");
        }

        private static void WriteTable(string folder, BalanceTable table) =>
            File.WriteAllText(Path.Combine(folder, table.Name + ".csv"), Csv.Write(table), Utf8);

        // --- Import ----------------------------------------------------------------------------------------

        /// <summary>Reads every table's CSV and plans the import; nothing changes.</summary>
        public static ImportPlan PlanImport(string folder = null)
        {
            folder ??= Folder;
            var tables = new Dictionary<string, BalanceTable>();
            var readErrors = new List<(string table, string message)>();
            foreach (var definition in BalanceTables.All())
            {
                string path = Path.Combine(folder, definition.Name + ".csv");
                if (!File.Exists(path)) continue; // reported as a missing table by the plan
                try
                {
                    tables[definition.Name] = Csv.Read(definition.Name, File.ReadAllText(path, Utf8));
                }
                catch (FormatException e)
                {
                    readErrors.Add((definition.Name, e.Message));
                    tables[definition.Name] = null;
                }
            }

            var readable = tables.Where(kv => kv.Value != null).ToDictionary(kv => kv.Key, kv => kv.Value);
            var plan = BalanceTables.Plan(readable, new EditorBalanceStore());
            foreach (var (table, message) in readErrors) plan.Error(table, message);
            return plan;
        }

        [MenuItem(Menu + "Import CSV (Dry Run)")]
        public static void DryRunMenu()
        {
            var plan = PlanImport();
            Debug.Log("[BalanceData] Dry run\n" + Describe(plan));
            EditorUtility.DisplayDialog("Balance data: dry run", Summary(plan), "OK");
        }

        [MenuItem(Menu + "Import CSV")]
        public static void ImportMenu()
        {
            var plan = PlanImport();
            Debug.Log("[BalanceData] Import\n" + Describe(plan));
            if (plan.HasErrors || plan.Changes.Count == 0)
            {
                EditorUtility.DisplayDialog("Balance data", Summary(plan), "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Balance data: import", Summary(plan) + "\n\nApply these changes?", "Apply", "Cancel"))
            {
                return;
            }
            plan.Apply();
            WriteReports();
            Debug.Log($"[BalanceData] Applied {plan.Changes.Count} changes. Running the guard tests...");
            RunGuardTests();
        }

        /// <summary>
        /// Batchmode: -executeMethod Game.BalanceData.Editor.BalanceDataTools.ImportBatch. Applies the CSVs, or fails
        /// (non-zero exit) listing the errors. Run the EditMode tests after it (-runTests).
        /// </summary>
        public static void ImportBatch()
        {
            var plan = PlanImport();
            Debug.Log("[BalanceData] Import\n" + Describe(plan));
            if (plan.HasErrors)
            {
                throw new InvalidOperationException($"Balance data import failed with {plan.Errors.Count} errors (see above)");
            }
            plan.Apply();
            WriteReports();
        }

        /// <summary>Batchmode: -executeMethod Game.BalanceData.Editor.BalanceDataTools.DryRunBatch</summary>
        public static void DryRunBatch()
        {
            var plan = PlanImport();
            Debug.Log("[BalanceData] Dry run\n" + Describe(plan));
            if (plan.HasErrors)
            {
                throw new InvalidOperationException($"Balance data has {plan.Errors.Count} errors (see above)");
            }
        }

        public static string Describe(ImportPlan plan)
        {
            var text = new StringBuilder();
            text.AppendLine(plan.HasErrors ? $"{plan.Errors.Count} errors, nothing will change:" : $"{plan.Changes.Count} changes:");
            foreach (string line in plan.HasErrors ? plan.Errors : plan.Changes) text.AppendLine("  " + line);
            return text.ToString();
        }

        public static string Summary(ImportPlan plan)
        {
            if (!plan.HasErrors && plan.Changes.Count == 0) return "The assets already match the CSV files.";
            var lines = plan.HasErrors ? plan.Errors : plan.Changes;
            string head = plan.HasErrors ? $"{plan.Errors.Count} errors; nothing will change:" : $"{plan.Changes.Count} changes:";
            string more = lines.Count > DialogLines ? $"\n... and {lines.Count - DialogLines} more (see the Console)" : string.Empty;
            return head + "\n" + string.Join("\n", lines.Take(DialogLines)) + more;
        }

        // --- Guard tests -----------------------------------------------------------------------------------

        private static readonly string[] GuardTests =
        {
            "BalanceDataTests", "BalanceTablesTests", "DataHygieneTests", "DamageSourceDataTests", "EnemyDamageTuningTests",
            "ExperienceLevelUpTests",
        };

        /// <summary>Runs the EditMode tests that guard the balance data and logs the result.</summary>
        [MenuItem(Menu + "Run Guard Tests")]
        public static void RunGuardTests()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new GuardTestCallbacks(api));
            api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                groupNames = new[] { $"^Game\\.Tests\\.({string.Join("|", GuardTests)})\\." },
            }));
        }

        private class GuardTestCallbacks : ICallbacks
        {
            private readonly TestRunnerApi api;
            private readonly List<string> failures = new();

            public GuardTestCallbacks(TestRunnerApi api) => this.api = api;

            public void RunStarted(ITestAdaptor testsToRun) { }

            public void TestStarted(ITestAdaptor test) { }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (!result.HasChildren && result.TestStatus == TestStatus.Failed)
                {
                    failures.Add($"{result.FullName}: {result.Message}");
                }
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                api.UnregisterCallbacks(this);
                if (failures.Count == 0)
                {
                    Debug.Log($"[BalanceData] Guard tests passed ({result.PassCount}).");
                }
                else
                {
                    Debug.LogError($"[BalanceData] {failures.Count} guard tests failed:\n" + string.Join("\n", failures));
                }
            }
        }
    }
}
