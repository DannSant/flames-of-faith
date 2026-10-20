namespace Game.UI.Style
{
    /// <summary>The two text sizes of the UI standard, plus Display for rare headline exceptions (YOU DIED).</summary>
    public enum UIFontSize
    {
        Small,
        Big,
        // Only for headline exceptions agreed in the standard (the death screen title)
        Display
    }

    public enum UIIconSize
    {
        Small,
        Medium,
        // 28x28: input glyphs, the coin next to a number, stars inside small buttons.
        Tiny
    }

    public enum UIButtonSize
    {
        Small,
        Big
    }

    public enum UIColorRole
    {
        // The style leaves the color alone (e.g. a script colors the text at runtime).
        None,
        Text,
        Muted,
        Grace,
        Corruption,
        Favors,
        // Yellow: highlights neutral elements (titles like PAUSE, section headers, previews)
        Highlight,
        // Light green: improved stats and positive highlights not related to Grace
        Positive,
        // Red: negative stats, warnings, the death title (not Corruption)
        Negative
    }

    public enum UIFrameKind
    {
        // Decorated frame around windows and HUD panels.
        Panel,
        // Thin frame around bars.
        Bar,
        // Item slot (inventory, shop...). Its tint can be changed at runtime (item quality).
        Slot
    }
}
