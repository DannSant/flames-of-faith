using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Style.Editor.UIBuildHelpers;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// The "wallet" row shared by the Inventory's title row and the map window: [Favor icon] 42 (+5), then the
    /// existing coins group, right-aligned. Both groups get a hover / gamepad tooltip target (a clear Image for
    /// raycasts and a TooltipTriggerUI that their controller sets up at runtime).
    /// </summary>
    public static class CurrencyRowBuilder
    {
        // Placeholder until the art team draws a Favor icon: the stat star, tinted Favors gold
        public const string FavorIconPath = "Assets/Sprites/UI/StatRoleStar.png";

        public const string WalletName = "Wallet";
        public const string FavorsName = "Favors";
        public const string FavorsIconName = "FavorsIcon";
        public const string FavorsTextName = "FavorsText";

        /// <summary>
        /// Puts <paramref name="coins"/> in a right-aligned Wallet row (placed by the caller) after a new Favors group.
        /// Returns the Favors text.
        /// </summary>
        public static TextMeshProUGUI Build(RectTransform wallet, RectTransform coins, UISettings s)
        {
            var row = GetOrAdd<HorizontalLayoutGroup>(wallet.gameObject);
            row.padding = new RectOffset(0, 0, 0, 0);
            row.spacing = 2f * s.Spacing;
            row.childAlignment = TextAnchor.MiddleRight;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            coins.SetParent(wallet, false);
            coins.SetAsLastSibling();
            MakeTooltipTarget(coins.gameObject);

            var favors = FindOrCreate(wallet, FavorsName);
            favors.SetSiblingIndex(0);
            var favorsRow = GetOrAdd<HorizontalLayoutGroup>(favors.gameObject);
            favorsRow.padding = new RectOffset(0, 0, 0, 0);
            favorsRow.spacing = 4f;
            favorsRow.childAlignment = TextAnchor.MiddleRight;
            favorsRow.childControlWidth = true;
            favorsRow.childControlHeight = true;
            favorsRow.childForceExpandWidth = false;
            favorsRow.childForceExpandHeight = false;
            MakeTooltipTarget(favors.gameObject);

            var icon = FindOrCreateImage(favors, FavorsIconName);
            icon.transform.SetSiblingIndex(0);
            icon.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(FavorIconPath);
            if (icon.sprite == null) Debug.LogError($"[CurrencyRowBuilder] No sprite at {FavorIconPath}");
            icon.color = s.FavorsColor;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var iconLayout = GetOrAdd<LayoutElement>(icon.gameObject);
            iconLayout.ignoreLayout = false;
            iconLayout.minWidth = iconLayout.preferredWidth = s.TinyIconSize;
            iconLayout.minHeight = iconLayout.preferredHeight = s.TinyIconSize;
            StyleIcon(icon.gameObject, UIIconSize.Tiny, s);

            var text = FindOrCreateText(favors, FavorsTextName);
            text.transform.SetSiblingIndex(1);
            text.alignment = TextAlignmentOptions.MidlineRight;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.margin = Vector4.zero;
            text.raycastTarget = false;
            text.text = "42 (+5)";
            StyleText(text, UIFontSize.Small, false, UIColorRole.Favors, s);
            return text;
        }

        // Hover needs a raycast graphic on the group itself, so the whole icon + number shows the tooltip
        private static void MakeTooltipTarget(GameObject group)
        {
            var hitArea = GetOrAdd<Image>(group);
            hitArea.sprite = null;
            hitArea.color = Color.clear;
            hitArea.raycastTarget = true;
            GetOrAdd<TooltipTriggerUI>(group);
        }
    }
}
