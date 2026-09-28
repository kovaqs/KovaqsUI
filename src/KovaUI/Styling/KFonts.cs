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

/// <summary>Métodos de extensión ergonómicos para manipulación segura de fuentes preservando las familias privadas.</summary>
public static class KFontExtensions
{
    /// <summary>Crea una nueva instancia de la fuente con el tamaño especificado, preservando la familia y peso privados si corresponde.</summary>
    public static Font WithSize(this Font font, float size)
    {
        ArgumentNullException.ThrowIfNull(font);
        if (!float.IsFinite(size) || size <= 0) throw new ArgumentOutOfRangeException(nameof(size));

        string familyName = !string.IsNullOrEmpty(font.OriginalFontName) ? font.OriginalFontName : font.Name;
        if (KFontManager.TryMatchPrivateFamily(familyName, out _, out _))
        {
            return KFontManager.CreateFont(familyName, size, font.Style, font.Unit);
        }

        return new Font(font.FontFamily, size, font.Style, font.Unit);
    }

    /// <summary>Crea una nueva instancia de la fuente con el peso especificado dentro de la misma familia tipográfica.</summary>
    public static Font WithWeight(this Font font, KFontWeight weight)
    {
        ArgumentNullException.ThrowIfNull(font);
        if (!Enum.IsDefined(weight)) throw new ArgumentOutOfRangeException(nameof(weight));

        string familyName = !string.IsNullOrEmpty(font.OriginalFontName) ? font.OriginalFontName : font.Name;
        if (KFontManager.TryMatchPrivateFamily(familyName, out string matched, out _))
        {
            return matched == "Outfit" ? KFonts.Outfit(font.Size, weight) : KFonts.Inter(font.Size, weight);
        }

        var style = weight is KFontWeight.Bold or KFontWeight.SemiBold ? font.Style | FontStyle.Bold : font.Style & ~FontStyle.Bold;
        return new Font(font.FontFamily, font.Size, style, font.Unit);
    }
}
