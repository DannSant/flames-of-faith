using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.BalanceData
{
    /// <summary>
    /// Lets the designer play his own numbers in a test build, without Unity. When a build finds
    /// <see cref="ConfigFileName"/> next to its executable, it downloads the latest version published from the balance
    /// sheet and applies it to the loaded assets (in memory, for the session) with the same importer the Editor uses.
    /// A version with any error is not applied at all, and the errors are shown. Builds without the file (releases)
    /// do nothing. Never runs in the Editor, where changing assets at runtime would save them.
    /// </summary>
    public class BalanceOverride : MonoBehaviour
    {
        public const string ConfigFileName = "balance-override.json";
        private const float MessageSeconds = 12f;
        private const int ErrorsShown = 8;

        [Serializable]
        public class Config
        {
            public string url;
            public string token;
            public bool enabled = true;
        }

        private Config config;
        private string message;
        private bool failed;
        private float hideAt;

        /// <summary>The applied version's description, or null (no override, not loaded yet, or failed).</summary>
        public static string AppliedVersion { get; private set; }

        public static string ConfigPath => Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? ".", ConfigFileName);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Application.isEditor || !File.Exists(ConfigPath)) return;

            Config loaded;
            try
            {
                loaded = JsonUtility.FromJson<Config>(File.ReadAllText(ConfigPath));
            }
            catch (Exception e)
            {
                Debug.LogError($"[BalanceOverride] {ConfigFileName} can't be read: {e.Message}");
                return;
            }
            if (loaded == null || !loaded.enabled || string.IsNullOrEmpty(loaded.url)) return;

            var go = new GameObject(nameof(BalanceOverride));
            DontDestroyOnLoad(go);
            var component = go.AddComponent<BalanceOverride>();
            component.config = loaded;
            component.StartCoroutine(component.Run());
        }

        public static string LatestUrl(string url, string token) =>
            $"{url}?action=latest&token={UnityWebRequest.EscapeURL(token ?? string.Empty)}";

        private IEnumerator Run()
        {
            Show("Balance data: downloading the latest published version...", false);
            using var request = UnityWebRequest.Get(LatestUrl(config.url, config.token));
            request.timeout = 30;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Show($"Balance data: download failed ({request.error}). Playing with the built-in numbers.", true);
                yield break;
            }

            var result = ApplySnapshot(request.downloadHandler.text, out var lines);
            foreach (string line in lines) Debug.Log("[BalanceOverride] " + line);
            Show(result, failed);
        }

        /// <summary>Plans and, if it has no errors, applies a snapshot. Returns the message to show.</summary>
        private string ApplySnapshot(string json, out List<string> lines)
        {
            lines = new List<string>();
            BalanceSnapshot snapshot;
            try
            {
                snapshot = BalanceSnapshot.FromJson(json);
            }
            catch (Exception e)
            {
                failed = true;
                return $"Balance data: the sheet's answer can't be read ({e.Message}). Playing with the built-in numbers.";
            }
            if (snapshot == null || !string.IsNullOrEmpty(snapshot.error))
            {
                failed = true;
                return $"Balance data: {snapshot?.error ?? "empty answer"}. Playing with the built-in numbers.";
            }

            var readErrors = new List<string>();
            var tables = snapshot.ReadTables(readErrors);
            var plan = BalanceTables.Plan(tables, new RuntimeBalanceStore());
            var errors = readErrors.Concat(plan.Errors).ToList();
            if (errors.Count > 0)
            {
                failed = true;
                lines.AddRange(errors);
                return $"Balance data {snapshot.Describe()} has {errors.Count} errors, so none of it is used:\n"
                    + string.Join("\n", errors.Take(ErrorsShown))
                    + (errors.Count > ErrorsShown ? $"\n... and {errors.Count - ErrorsShown} more (see Player.log)" : string.Empty);
            }

            plan.Apply();
            AppliedVersion = snapshot.Describe();
            lines.AddRange(plan.Changes);
            return $"Balance data {snapshot.Describe()}\n{plan.Changes.Count} values differ from this build's own.";
        }

        private void Show(string text, bool isError)
        {
            message = text;
            failed |= isError;
            hideAt = Time.unscaledTime + MessageSeconds;
            Debug.Log("[BalanceOverride] " + text);
        }

        private void OnGUI()
        {
            // Errors stay up; anything else goes after a few seconds, so it never sits over the HUD
            if (string.IsNullOrEmpty(message) || (!failed && Time.unscaledTime >= hideAt)) return;
            string text = message;
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                fontSize = 14,
                normal = { textColor = failed ? new Color(1f, 0.55f, 0.55f) : Color.white },
            };
            float width = Mathf.Min(720f, Screen.width - 20f);
            float height = style.CalcHeight(new GUIContent(text), width);
            GUI.Box(new Rect(10f, 10f, width, height), text, style);
        }
    }
}
