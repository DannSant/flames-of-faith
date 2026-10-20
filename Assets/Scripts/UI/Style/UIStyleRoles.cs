namespace Game.UI.Style
{
    /// <summary>The only two text sizes of the UI standard.</summary>
    public enum UIFontSize
    {
        Small,
        Big
    }

    public enum UIIconSize
    {
        Small,
        Medium,
        // Input glyphs, sized like text rather than like icons.
        Prompt
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
        Favors
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
