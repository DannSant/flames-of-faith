using Game.UI.Navigation;
using TMPro;
using UnityEditor;
using UnityEngine;
using static Game.UI.Style.Editor.UIBuildHelpers;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// UI revamp, step 3: restyles the pause menu and the death screen in place as center windows
    /// (CenterWindowLayout). PAUSE: Big, bold, yellow. YOU DIED: the Display headline exception, red.
    /// Resume and Retry take the shared progress spot; the other buttons stack above them.
    /// Batchmode: -executeMethod Game.UI.Style.Editor.PauseDeathRestyler.Restyle
    /// </summary>
    public static class PauseDeathRestyler
    {
        public const string PausePath = "Assets/Prefabs/UI/Systems/PauseMenu.prefab";
        public const string DeathPath = "Assets/Prefabs/UI/Systems/DeathscreenPane.prefab";
        private const string UIMainScene = "Assets/Scenes/UIMain.unity";

        [MenuItem("Tools/Flames of Faith/UI Revamp/Restyle Pause and Death Screen")]
        public static void Restyle()
        {
            var settings = UIRevampTools.LoadSettings();
            if (settings == null)
            {
                Debug.LogError($"[PauseDeathRestyler] {UIRevampTools.SettingsPath} not found. Run Apply Foundation first.");
                return;
            }

            UpgradeRestyler.EditPrefab(PausePath, root => RestyleWindow(root, settings, UIColorRole.Highlight, UIFontSize.Big, "ResumeButton", "ExitButton"));
            UpgradeRestyler.EditPrefab(DeathPath, root => RestyleWindow(root, settings, UIColorRole.Negative, UIFontSize.Display, "RetryButton", "ExitButton"));
            UIRevampTools.RunOnScenes(new[] { UIMainScene }, (scene, path) => UpgradeRestyler.SnapSceneInstances(scene, PausePath, DeathPath));

            AssetDatabase.SaveAssets();
            Debug.Log("[PauseDeathRestyler] Pause menu and death screen restyled.");
        }

        /// <summary>The window, its title, and its buttons from the progress spot upward (first = progress button).</summary>
        private static void RestyleWindow(GameObject root, UISettings s, UIColorRole titleColor, UIFontSize titleSize, params string[] buttonsFromBottom)
        {
            CenterWindowLayout.PlaceWindow((RectTransform)root.transform, s);

            var mainPanel = Find(root.transform, "MainPanel");
            CenterWindowLayout.FrameWindow(mainPanel, s);
            CenterWindowLayout.UseStandardMarker(mainPanel.GetComponent<UIWindow>());

            var title = Find(mainPanel, "Title").GetComponent<TextMeshProUGUI>();
            CenterWindowLayout.PlaceTitle(title, s, titleColor, titleSize);

            for (int slot = 0; slot < buttonsFromBottom.Length; slot++)
            {
                CenterWindowLayout.PlaceStackedButton(Find(mainPanel, buttonsFromBottom[slot]), s, slot);
            }
        }
    }
}
