namespace KovaUI.Styling;

public enum KFontWeight { Regular, Medium, SemiBold, Bold }

/// <summary>Punto de acceso estático para fuentes privadas del proceso. Cada Font devuelto pertenece al llamador y debe desecharse.</summary>
public static class KFonts
{
    public static bool InterLoaded => KFontManager.InterLoaded;
    public static string? InterLoadError => KFontManager.InterLoadError;
    public static string InterLicense => KFontManager.InterLicense;

    public static bool OutfitLoaded => KFontManager.OutfitLoaded;
    public static string? OutfitLoadError => KFontManager.OutfitLoadError;
    public static string OutfitLicense => KFontManager.OutfitLicense;

    /// <summary>Crea una fuente Inter medida en puntos. Medium y SemiBold utilizan caras estáticas reales.</summary>
    public static Font Inter(float size, KFontWeight weight = KFontWeight.Regular) => KFontManager.CreateInter(size, weight);

    /// <summary>Crea una fuente Outfit medida en puntos. Medium y SemiBold utilizan caras estáticas reales.</summary>
    public static Font Outfit(float size, KFontWeight weight = KFontWeight.Regular) => KFontManager.CreateOutfit(size, weight);
}
