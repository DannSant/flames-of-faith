using Game.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// The gold (Key) / silver (Complementary) star next to a class's recommended stats, and its
    /// tooltip line. Shared by the Stats pane rows and the upgrade choice buttons.
    /// </summary>
    public static class StatRoleStar
    {
        public static void Apply(Image star, StatRole role, Color keyColor, Color complementaryColor)
        {
            if (star == null) return;
            star.gameObject.SetActive(role != StatRole.None);
            star.color = role == StatRole.Key ? keyColor : complementaryColor;
        }

        /// <summary>Adds the role line ("Key stat for the Warrior: ...") under a stat description.</summary>
        public static string AppendRoleLine(string description, string roleLine, Color color)
        {
            if (string.IsNullOrEmpty(roleLine)) return description;
            string line = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{roleLine}</color>";
            return string.IsNullOrEmpty(description) ? line : $"{description}\n\n{line}";
        }
    }
}
