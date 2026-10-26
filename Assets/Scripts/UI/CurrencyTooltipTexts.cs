using System;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// The coins' and Favors' display and tooltip texts, shared by the Inventory and the map window. Player-facing
    /// terms: the coins are Golden Chronos, a run is a Pilgrimage (see the glossary in CLAUDE.md).
    /// </summary>
    [Serializable]
    public class CurrencyTooltipTexts
    {
        [SerializeField] private string coinsTitle = "Golden Chronos";
        [TextArea(2, 4)]
        [SerializeField] private string coinsText = "Gold to spend on items and skills at Shops. You lose it when your Pilgrimage ends.";
        [SerializeField] private string favorsTitle = "Favors";
        [Tooltip("{0} = banked Favors, {1} = Favors earned this Pilgrimage.")]
        [TextArea(3, 6)]
        [SerializeField] private string favorsFormat =
            "{0} banked: spend them between Pilgrimages on permanent upgrades and new characters, in the Skill Tree.\n" +
            "{1} earned this Pilgrimage: they're banked when it ends.";

        public string CoinsTitle => coinsTitle;
        public string CoinsText => coinsText;
        public string FavorsTitle => favorsTitle;

        public string FormatFavors(int banked, int earnedThisRun) =>
            string.Format(favorsFormat, banked, FormatSigned(earnedThisRun));

        /// <summary>The number next to the Favor icon: banked, then this Pilgrimage's in brackets when there are any, e.g. "42 (+5)".</summary>
        public static string FormatFavorsAmount(int banked, int earnedThisRun) =>
            earnedThisRun == 0 ? banked.ToString() : $"{banked} ({FormatSigned(earnedThisRun)})";

        private static string FormatSigned(int value) => value > 0 ? $"+{value}" : value.ToString();
    }
}
