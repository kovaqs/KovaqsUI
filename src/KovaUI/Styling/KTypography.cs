namespace KovaUI.Styling;

/// <summary>Central typography in points. Every getter returns a new owned Font; callers must dispose it.</summary>
public static class KTypography
{
    public static Font Body => KFonts.Outfit(10f, KFontWeight.Regular);
    public static Font BodyMedium => KFonts.Outfit(10f, KFontWeight.Medium);
    public static Font Label => KFonts.Outfit(10f, KFontWeight.Medium);
    public static Font Button => KFonts.Outfit(10f, KFontWeight.Medium);
    public static Font Title => KFonts.Outfit(23f, KFontWeight.SemiBold);
    public static Font Subtitle => KFonts.Outfit(11f, KFontWeight.Regular);
    public static Font Caption => KFonts.Outfit(9f, KFontWeight.Regular);
    public static Font Section => KFonts.Outfit(9f, KFontWeight.SemiBold);
}
