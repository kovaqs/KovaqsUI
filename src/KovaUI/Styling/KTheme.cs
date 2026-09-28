namespace KovaUI.Styling;

public enum KButtonVariant { Primary, Secondary, Outline, Ghost, Danger }
public enum KShape { Rounded, Rectangle, Capsule, Ellipse }

/// <summary>Immutable theme tokens. Assign a new theme to change a subtree without global event subscriptions.</summary>
public sealed record KTheme
{
    public Color Background { get; init; } = Color.FromArgb(248, 248, 248);
    public Color Surface { get; init; } = Color.White;
    public Color SurfaceHover { get; init; } = Color.FromArgb(244, 244, 244);
    public Color SurfacePressed { get; init; } = Color.FromArgb(233, 233, 233);
    public Color Border { get; init; } = Color.FromArgb(218, 218, 218);
    public Color TextPrimary { get; init; } = Color.FromArgb(13, 21, 23);
    public Color TextSecondary { get; init; } = Color.FromArgb(119, 119, 119);
    public Color Primary { get; init; } = Color.FromArgb(255, 126, 28);
    public Color PrimaryHover { get; init; } = Color.FromArgb(242, 115, 20);
    public Color PrimaryPressed { get; init; } = Color.FromArgb(221, 99, 12);
    public Color OnPrimary { get; init; } = Color.FromArgb(20, 22, 24);
    public Color Secondary { get; init; } = Color.FromArgb(13, 21, 23);
    public Color SecondaryHover { get; init; } = Color.FromArgb(39, 48, 51);
    public Color SecondaryPressed { get; init; } = Color.FromArgb(0, 8, 10);
    public Color OnSecondary { get; init; } = Color.White;
    public Color Danger { get; init; } = Color.FromArgb(208, 65, 30);
    public Color Focus { get; init; } = Color.FromArgb(46, 114, 218);
    public Color DisabledFill { get; init; } = Color.FromArgb(238, 238, 238);
    public Color DisabledText { get; init; } = Color.FromArgb(146, 146, 146);
    public Color DisabledBorder { get; init; } = Color.FromArgb(224, 224, 224);
    public static KTheme Light { get; } = new();
    public static KTheme Dark { get; } = new()
    {
        Background = Color.FromArgb(24, 27, 30), Surface = Color.FromArgb(34, 38, 42),
        SurfaceHover = Color.FromArgb(44, 49, 54), SurfacePressed = Color.FromArgb(53, 59, 65),
        Border = Color.FromArgb(70, 76, 82), TextPrimary = Color.FromArgb(240, 242, 244),
        TextSecondary = Color.FromArgb(177, 182, 187), Focus = Color.FromArgb(116, 173, 255),
        DisabledFill = Color.FromArgb(44, 47, 50), DisabledText = Color.FromArgb(122, 128, 134),
        DisabledBorder = Color.FromArgb(61, 65, 69), Danger = Color.FromArgb(255, 140, 110)
    };
}

public static class KMetrics
{
    public const int RadiusSmall = 8, RadiusMedium = 12, RadiusLarge = 16;
    public const int SpaceXS = 4, SpaceS = 8, SpaceM = 12, SpaceL = 16, SpaceXL = 24;
    public const int HeightSmall = 32, HeightMedium = 40, HeightLarge = 48;
}

public interface IKThemeControl { KTheme Theme { get; set; } }
public static class KThemeManager
{
    public static void Apply(Control root, KTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        if (root is IKThemeControl themed) { themed.Theme = theme; if (root is not Panel) return; }
        foreach (Control child in root.Controls) Apply(child, theme);
    }
}


