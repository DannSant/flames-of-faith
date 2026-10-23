using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Game.BalanceData.Editor
{
    /// <summary>
    /// Talks to the balance sheet's web app (BalanceData/Sheets/BalanceSync.gs).
    /// - Pull from Sheet: downloads the latest published version, plans the import (any error cancels it), applies it
    ///   after a confirm, re-exports the CSVs and reports from the assets, records the version in
    ///   BalanceData/snapshot.json and pushes the new reports back to the sheet.
    /// - Push Reports / Push All: write the Report_ tabs, or every tab, from the project.
    /// The web app URL and token are per developer, in UserSettings/ (not committed).
    /// </summary>
    public static class BalanceSheetSync
    {
        private const string Menu = "Tools/Flames of Faith/Balance Data/";
        public const string ReportPrefix = "Report_";

        public static string ConfigPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "UserSettings", "BalanceSheetSync.json");
        public static string SnapshotInfoPath => Path.Combine(BalanceDataTools.Folder, "snapshot.json");

        [Serializable]
        private class PushRequest
        {
            public string token;
            public string action;
            public BalanceSnapshotTable[] tables;
        }

        [Serializable]
        private class PushResponse
        {
            public bool ok;
            public string error;
            public string[] written;
        }

        /// <summary>What the repo records about the last version pulled from the sheet.</summary>
        [Serializable]
        public class SnapshotInfo
        {
            public int version;
            public string publishedAt;
            public string author;
            public string note;
            public string hash;
        }

        public static BalanceOverride.Config LoadConfig()
        {
            if (!File.Exists(ConfigPath)) return null;
            var config = JsonUtility.FromJson<BalanceOverride.Config>(File.ReadAllText(ConfigPath));
            return config != null && !string.IsNullOrEmpty(config.url) && !string.IsNullOrEmpty(config.token) ? config : null;
        }

        public static void SaveConfig(BalanceOverride.Config config)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
            File.WriteAllText(ConfigPath, JsonUtility.ToJson(config, true));
        }

        private static BalanceOverride.Config RequireConfig()
        {
            var config = LoadConfig();
            if (config == null)
            {
                EditorUtility.DisplayDialog("Balance sheet", "Set the sheet's web app URL and token first.", "OK");
                BalanceSheetSettingsWindow.Open();
            }
            return config;
        }

        // --- Pull --------------------------------------------------------------------------------------------

        [MenuItem(Menu + "Pull from Sheet", priority = 0)]
        public static void PullMenu()
        {
            var config = RequireConfig();
            if (config == null) return;

            BalanceSnapshot snapshot;
            try
            {
                EditorUtility.DisplayProgressBar("Balance sheet", "Downloading the latest published version...", 0.3f);
                snapshot = BalanceSnapshot.FromJson(Get(BalanceOverride.LatestUrl(config.url, config.token)));
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Balance sheet", "The download failed: " + e.Message, "OK");
                return;
            }
            EditorUtility.ClearProgressBar();

            if (snapshot == null || !string.IsNullOrEmpty(snapshot.error))
            {
                EditorUtility.DisplayDialog("Balance sheet", "The sheet says: " + (snapshot?.error ?? "empty answer"), "OK");
                return;
            }

            var readErrors = new List<string>();
            var plan = BalanceTables.Plan(snapshot.ReadTables(readErrors), new EditorBalanceStore());
            foreach (string error in readErrors) plan.Error("Sheet", error);
            Debug.Log($"[BalanceData] Pull {snapshot.Describe()}\n" + BalanceDataTools.Describe(plan));

            if (plan.HasErrors)
            {
                EditorUtility.DisplayDialog("Balance sheet", $"Version {snapshot.version} can't be imported.\n\n" + BalanceDataTools.Summary(plan), "OK");
                return;
            }
            if (plan.Changes.Count > 0 && !EditorUtility.DisplayDialog("Balance sheet",
                    $"{snapshot.Describe()}\n\n{BalanceDataTools.Summary(plan)}\n\nApply these changes?", "Apply", "Cancel"))
            {
                return;
            }

            plan.Apply();
            BalanceDataTools.Export();
            WriteSnapshotInfo(snapshot);
            string pushed = TryPushReports(config);
            EditorUtility.DisplayDialog("Balance sheet",
                (plan.Changes.Count == 0 ? $"The assets already match version {snapshot.version}." : $"Applied version {snapshot.version}: {plan.Changes.Count} changes.")
                + "\n\nThe CSV files in BalanceData/ were rewritten; commit them with the assets.\n" + pushed, "OK");
            if (plan.Changes.Count > 0) BalanceDataTools.RunGuardTests();
        }

        private static void WriteSnapshotInfo(BalanceSnapshot snapshot)
        {
            var info = new SnapshotInfo
            {
                version = snapshot.version,
                publishedAt = snapshot.publishedAt,
                author = snapshot.author,
                note = snapshot.note,
                hash = snapshot.hash,
            };
            File.WriteAllText(SnapshotInfoPath, JsonUtility.ToJson(info, true) + "\n", new UTF8Encoding(false));
        }

        // --- Push --------------------------------------------------------------------------------------------

        [MenuItem(Menu + "Push Reports to Sheet", priority = 1)]
        public static void PushReportsMenu()
        {
            var config = RequireConfig();
            if (config == null) return;
            EditorUtility.DisplayDialog("Balance sheet", TryPushReports(config), "OK");
        }

        [MenuItem(Menu + "Push All to Sheet", priority = 3)]
        public static void PushAllMenu()
        {
            var config = RequireConfig();
            if (config == null) return;
            if (!EditorUtility.DisplayDialog("Balance sheet",
                    "This replaces every data tab of the sheet with the project's numbers (and refreshes the reports).\n\n" +
                    "Edits the designer made in the sheet and hasn't pulled into Unity yet are lost. Continue?", "Replace", "Cancel"))
            {
                return;
            }

            var tables = BalanceTables.All().Select(ToSnapshotTable)
                .Concat(Reports())
                .ToArray();
            EditorUtility.DisplayDialog("Balance sheet", TryPush(config, "tables", tables), "OK");
        }

        private static IEnumerable<BalanceSnapshotTable> Reports() =>
            BalanceReports.All().Select(report => new BalanceSnapshotTable { name = ReportPrefix + report.Name, csv = Csv.Write(report) });

        private static BalanceSnapshotTable ToSnapshotTable(BalanceTableDefinition definition) => new()
        {
            name = definition.Name,
            csv = Csv.Write(definition.Export()),
            description = definition.Description,
            columnNotes = definition.ColumnNotes.ToArray(),
        };

        /// <summary>Writes only the explanation panel at the side of every data tab, leaving the numbers alone.</summary>
        [MenuItem(Menu + "Push Table Notes to Sheet", priority = 2)]
        public static void PushNotesMenu()
        {
            var config = RequireConfig();
            if (config == null) return;
            var tables = BalanceTables.All()
                .Select(definition => new BalanceSnapshotTable { name = definition.Name, description = definition.Description, columnNotes = definition.ColumnNotes.ToArray() })
                .ToArray();
            EditorUtility.DisplayDialog("Balance sheet", TryPush(config, "notes", tables), "OK");
        }

        private static string TryPushReports(BalanceOverride.Config config) => TryPush(config, "reports", Reports().ToArray());

        private static string TryPush(BalanceOverride.Config config, string action, BalanceSnapshotTable[] tables)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Balance sheet", "Writing to the sheet...", 0.5f);
                string json = Post(config.url, JsonUtility.ToJson(new PushRequest { token = config.token, action = action, tables = tables }));
                var response = JsonUtility.FromJson<PushResponse>(json);
                if (response == null || !response.ok)
                {
                    return "The sheet refused the update: " + (response?.error ?? json);
                }
                Debug.Log($"[BalanceData] Wrote to the sheet: {string.Join(", ", response.written ?? Array.Empty<string>())}");
                return $"Wrote {response.written?.Length ?? 0} tabs to the sheet.";
            }
            catch (Exception e)
            {
                return "Writing to the sheet failed: " + e.Message;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // --- HTTP --------------------------------------------------------------------------------------------

        // Apps Script answers every request with a redirect to the response, and a POST's redirect must be followed
        // with a GET, so redirects are followed by hand. Runs off the main thread so the wait can't deadlock.
        public static string Get(string url) => Send(HttpMethod.Get, url, null);

        public static string Post(string url, string json) => Send(HttpMethod.Post, url, json);

        private static string Send(HttpMethod method, string url, string json)
        {
            return Task.Run(async () =>
            {
                using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(60) };
                var request = new HttpRequestMessage(method, url);
                if (json != null) request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                for (int redirects = 0; redirects < 5; redirects++)
                {
                    using var response = await client.SendAsync(request).ConfigureAwait(false);
                    int status = (int)response.StatusCode;
                    if (status >= 300 && status < 400 && response.Headers.Location != null)
                    {
                        request = new HttpRequestMessage(HttpMethod.Get, response.Headers.Location);
                        continue;
                    }
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new InvalidOperationException($"HTTP {status}: {Truncate(body)}");
                    }
                    if (body.TrimStart().StartsWith("<"))
                    {
                        throw new InvalidOperationException("the web app answered with a web page, not data (is it deployed with access \"Anyone\"?)");
                    }
                    return body;
                }
                throw new InvalidOperationException("too many redirects");
            }).GetAwaiter().GetResult();
        }

        private static string Truncate(string text) => text.Length > 300 ? text.Substring(0, 300) + "..." : text;
    }

    /// <summary>Tools/Flames of Faith/Balance Data/Sheet Settings: the web app URL and token, and a test build's config file.</summary>
    public class BalanceSheetSettingsWindow : EditorWindow
    {
        private string url;
        private string token;
        private string status;

        [MenuItem("Tools/Flames of Faith/Balance Data/Sheet Settings...", priority = 4)]
        public static void Open() => GetWindow<BalanceSheetSettingsWindow>(true, "Balance Sheet Settings");

        private void OnEnable()
        {
            var config = BalanceSheetSync.LoadConfig();
            url = config?.url ?? string.Empty;
            token = config?.token ?? string.Empty;
            minSize = new Vector2(520f, 210f);
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("From the sheet: Balance > Sync settings (owner) shows the token; the web app URL is the " +
                "\".../exec\" one in Apps Script's Deploy > Manage deployments. Saved in UserSettings/, never committed.", MessageType.Info);
            url = EditorGUILayout.TextField("Web app URL", url).Trim();
            token = EditorGUILayout.TextField("Token", token).Trim();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save")) Save();
            if (GUILayout.Button("Test connection")) Test();
            if (GUILayout.Button("Save test build config...")) SaveBuildConfig();
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(status)) EditorGUILayout.HelpBox(status, MessageType.None);
        }

        private BalanceOverride.Config Current() => new() { url = url, token = token, enabled = true };

        private void Save()
        {
            BalanceSheetSync.SaveConfig(Current());
            status = "Saved.";
        }

        private void Test()
        {
            try
            {
                string json = BalanceSheetSync.Get($"{url}?action=ping&token={UnityEngine.Networking.UnityWebRequest.EscapeURL(token)}");
                status = json.Contains("\"ok\":true") ? "Connected. " + json : "The sheet says: " + json;
            }
            catch (Exception e)
            {
                status = "Failed: " + e.Message;
            }
        }

        // The file a test build reads next to its .exe (BalanceOverride). Only for builds you hand to the designer.
        private void SaveBuildConfig()
        {
            string path = EditorUtility.SaveFilePanel("Save next to the test build's .exe", string.Empty, BalanceOverride.ConfigFileName, "json");
            if (string.IsNullOrEmpty(path)) return;
            File.WriteAllText(path, JsonUtility.ToJson(Current(), true));
            status = "Saved " + path + ". Keep it out of release builds.";
        }
    }
}
