namespace KovaUI.Styling;

/// <summary>Configuración tipográfica centralizada en puntos basada en Inter. Cada getter retorna un nuevo Font propio que el llamador debe liberar.</summary>
public static class KTypography
{
    public static Font Body => KFonts.Inter(10f, KFontWeight.Regular);
    public static Font BodyMedium => KFonts.Inter(10f, KFontWeight.Medium);
    public static Font Label => KFonts.Inter(10f, KFontWeight.Medium);
    public static Font Button => KFonts.Inter(10f, KFontWeight.Medium);
    public static Font Title => KFonts.Inter(23f, KFontWeight.SemiBold);
    public static Font Subtitle => KFonts.Inter(11f, KFontWeight.Regular);
    public static Font Caption => KFonts.Inter(9f, KFontWeight.Regular);
    public static Font Section => KFonts.Inter(9f, KFontWeight.SemiBold);
}
