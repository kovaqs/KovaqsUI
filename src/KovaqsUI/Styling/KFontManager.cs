using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace KovaqsUI.Styling;

/// <summary>Gestor centralizado de recursos tipográficos y colecciones privadas en memoria.</summary>
public static class KFontManager
{
    private const string InterPrefix = "KovaqsUI.Fonts.Inter.";
    private const string OutfitPrefix = "KovaqsUI.Fonts.Outfit.";

    private static readonly Lazy<FontResources> Resources = new(() => new FontResources(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static bool InterLoaded => Resources.Value.InterLoaded;
    public static string? InterLoadError => Resources.Value.InterError;
    public static bool OutfitLoaded => Resources.Value.OutfitLoaded;
    public static string? OutfitLoadError => Resources.Value.OutfitError;

    public static string InterLicense
    {
        get
        {
            using var stream = typeof(KFontManager).Assembly.GetManifestResourceStream(InterPrefix + "OFL.txt")
                ?? throw new InvalidOperationException("No se encuentra el recurso de licencia para Inter.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }

    public static string OutfitLicense
    {
        get
        {
            using var stream = typeof(KFontManager).Assembly.GetManifestResourceStream(OutfitPrefix + "OFL.txt")
                ?? throw new InvalidOperationException("No se encuentra el recurso de licencia para Outfit.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }

    /// <summary>Determina si un nombre tipográfico corresponde a una de las familias privadas integradas en KovaqsUI.</summary>
    public static bool IsPrivateFamily(string? name) => TryMatchPrivateFamily(name, out _, out _);

    /// <summary>Identifica la familia base y el peso correspondiente a partir de un nombre de fuente o cadena de especificación.</summary>
    public static bool TryMatchPrivateFamily(string? fontName, out string matchedFamily, out KFontWeight suggestedWeight)
    {
        matchedFamily = string.Empty;
        suggestedWeight = KFontWeight.Regular;
        if (string.IsNullOrWhiteSpace(fontName)) return false;

        var cleaned = fontName.Split(',')[0].Trim();

        if (cleaned.Equals("Inter", StringComparison.OrdinalIgnoreCase) ||
            cleaned.Equals("Inter Regular", StringComparison.OrdinalIgnoreCase))
        {
            matchedFamily = "Inter";
            suggestedWeight = KFontWeight.Regular;
            return true;
        }
        if (cleaned.Equals("Inter Medium", StringComparison.OrdinalIgnoreCase))
        {
            matchedFamily = "Inter";
            suggestedWeight = KFontWeight.Medium;
            return true;
        }
        if (cleaned.Equals("Inter SemiBold", StringComparison.OrdinalIgnoreCase) ||
            cleaned.Equals("Inter Semibold", StringComparison.OrdinalIgnoreCase) ||
            cleaned.Equals("Inter Semi Bold", StringComparison.OrdinalIgnoreCase))
        {
            matchedFamily = "Inter";
            suggestedWeight = KFontWeight.SemiBold;
            return true;
        }
        if (cleaned.Equals("Inter Bold", StringComparison.OrdinalIgnoreCase))
        {
            matchedFamily = "Inter";
            suggestedWeight = KFontWeight.Bold;
            return true;
        }

        if (cleaned.Equals("Outfit", StringComparison.OrdinalIgnoreCase) ||
            cleaned.Equals("Outfit Regular", StringComparison.OrdinalIgnoreCase))
        {
            matchedFamily = "Outfit";
            suggestedWeight = KFontWeight.Regular;
            return true;
        }
        if (cleaned.Equals("Outfit Medium", StringComparison.OrdinalIgnoreCase))
        {
            matchedFamily = "Outfit";
            suggestedWeight = KFontWeight.Medium;
            return true;
        }
        if (cleaned.Equals("Outfit SemiBold", StringComparison.OrdinalIgnoreCase) ||
            cleaned.Equals("Outfit Semibold", StringComparison.OrdinalIgnoreCase) ||
            cleaned.Equals("Outfit Semi Bold", StringComparison.OrdinalIgnoreCase))
        {
            matchedFamily = "Outfit";
            suggestedWeight = KFontWeight.SemiBold;
            return true;
        }
        if (cleaned.Equals("Outfit Bold", StringComparison.OrdinalIgnoreCase))
        {
            matchedFamily = "Outfit";
            suggestedWeight = KFontWeight.Bold;
            return true;
        }

        return false;
    }

    /// <summary>Crea una instancia de Font a partir de una familia, tamaño, estilo y unidad.</summary>
    public static Font CreateFont(string familyName, float size, FontStyle style = FontStyle.Regular, GraphicsUnit unit = GraphicsUnit.Point)
    {
        if (!float.IsFinite(size) || size <= 0) size = 10f;
        if (unit != GraphicsUnit.Point && unit != GraphicsUnit.Pixel) unit = GraphicsUnit.Point;

        if (!TryMatchPrivateFamily(familyName, out string matched, out KFontWeight weight))
        {
            try
            {
                return new Font(familyName, size, style, unit);
            }
            catch (Exception ex) when (IsFontFailure(ex))
            {
                return CreateFallback(size, KFontWeight.Regular, style, unit);
            }
        }

        if (style.HasFlag(FontStyle.Bold) && weight == KFontWeight.Regular)
        {
            weight = KFontWeight.Bold;
        }

        var res = Resources.Value;
        lock (res)
        {
            bool loaded = matched == "Outfit" ? res.OutfitLoaded : res.InterLoaded;
            if (loaded)
            {
                try
                {
                    var families = matched == "Outfit" ? res.OutfitFamilies : res.InterFamilies;
                    var family = families[(int)weight];

                    FontStyle effectiveStyle = FontStyle.Regular;
                    if (weight == KFontWeight.Bold || style.HasFlag(FontStyle.Bold)) effectiveStyle |= FontStyle.Bold;
                    if (style.HasFlag(FontStyle.Italic)) effectiveStyle |= FontStyle.Italic;
                    if (style.HasFlag(FontStyle.Underline)) effectiveStyle |= FontStyle.Underline;
                    if (style.HasFlag(FontStyle.Strikeout)) effectiveStyle |= FontStyle.Strikeout;

                    if (!family.IsStyleAvailable(effectiveStyle))
                    {
                        effectiveStyle = (weight == KFontWeight.Bold || style.HasFlag(FontStyle.Bold)) && family.IsStyleAvailable(FontStyle.Bold)
                            ? FontStyle.Bold
                            : FontStyle.Regular;
                    }

                    return new Font(family, size, effectiveStyle, unit);
                }
                catch (Exception ex) when (IsFontFailure(ex)) { /* Entorno o diseñador restringido */ }
            }
            return CreateFallback(size, weight, style, unit);
        }
    }

    /// <summary>Resuelve y repara instancias de Font que hayan sido degradadas por GDI+ a Microsoft Sans Serif al reconstruirse por nombre.</summary>
    public static Font ResolveFont(Font? incoming, Font? defaultFont = null)
    {
        if (incoming is null)
        {
            return defaultFont ?? CreateInter(10f);
        }

        if (ReferenceEquals(incoming, defaultFont))
        {
            return incoming;
        }

        string? original = incoming.OriginalFontName;
        string current = incoming.Name;

        string candidate = !string.IsNullOrWhiteSpace(original) ? original : current;
        if (TryMatchPrivateFamily(candidate, out _, out _))
        {
            bool isDegraded = !string.Equals(current, candidate, StringComparison.OrdinalIgnoreCase)
                || string.Equals(current, "Microsoft Sans Serif", StringComparison.OrdinalIgnoreCase);

            if (isDegraded)
            {
                var resolved = CreateFont(candidate, incoming.Size, incoming.Style, incoming.Unit);
                try { incoming.Dispose(); } catch { }
                return resolved;
            }
        }

        return incoming;
    }

    /// <summary>Crea un Font de Inter medido en puntos. Cada llamada retorna una instancia propia que debe liberarse.</summary>
    public static Font CreateInter(float size, KFontWeight weight = KFontWeight.Regular)
    {
        if (!float.IsFinite(size) || size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
        if (!Enum.IsDefined(weight)) throw new ArgumentOutOfRangeException(nameof(weight));
        var res = Resources.Value;
        lock (res)
        {
            if (res.InterLoaded)
            {
                try
                {
                    return new Font(res.InterFamilies[(int)weight], size, weight == KFontWeight.Bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Point);
                }
                catch (Exception ex) when (IsFontFailure(ex)) { /* Entorno o diseñador restringido */ }
            }
            return CreateFallback(size, weight);
        }
    }

    /// <summary>Crea un Font de Outfit medido en puntos. Cada llamada retorna una instancia propia que debe liberarse.</summary>
    public static Font CreateOutfit(float size, KFontWeight weight = KFontWeight.Regular)
    {
        if (!float.IsFinite(size) || size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
        if (!Enum.IsDefined(weight)) throw new ArgumentOutOfRangeException(nameof(weight));
        var res = Resources.Value;
        lock (res)
        {
            if (res.OutfitLoaded)
            {
                try
                {
                    return new Font(res.OutfitFamilies[(int)weight], size, weight == KFontWeight.Bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Point);
                }
                catch (Exception ex) when (IsFontFailure(ex)) { /* Entorno o diseñador restringido */ }
            }
            return CreateFallback(size, weight);
        }
    }

    private static Font CreateFallback(float size, KFontWeight weight, FontStyle style = FontStyle.Regular, GraphicsUnit unit = GraphicsUnit.Point)
    {
        var effectiveStyle = style;
        if (weight is KFontWeight.Bold or KFontWeight.SemiBold)
        {
            effectiveStyle |= FontStyle.Bold;
        }
        return new Font("Segoe UI", size, effectiveStyle, unit);
    }

    private static bool IsFontFailure(Exception ex) => ex is ArgumentException or ExternalException or IOException
        or InvalidOperationException or NotSupportedException or TypeInitializationException or DllNotFoundException
        or EntryPointNotFoundException or UnauthorizedAccessException or OutOfMemoryException;

    private sealed class FontResources
    {
        private readonly List<PrivateFontCollection> collections = [];
        private readonly List<IntPtr> buffers = [];
        private readonly List<IntPtr> registrations = [];

        internal FontFamily[] InterFamilies = [];
        internal FontFamily[] OutfitFamilies = [];
        internal bool InterLoaded;
        internal bool OutfitLoaded;
        internal string? InterError;
        internal string? OutfitError;

        internal FontResources()
        {
            // Accept the previous switch name so existing host configuration keeps working.
            bool disabled = (AppContext.TryGetSwitch("KovaqsUI.DisableEmbeddedFonts", out bool current) && current)
                || (AppContext.TryGetSwitch("KovaUI.DisableEmbeddedFonts", out bool legacy) && legacy);
            if (disabled)
            {
                InterError = "Las fuentes integradas están desactivadas para este proceso.";
                OutfitError = InterError;
                return;
            }

            // Carga de Inter
            try
            {
                var interCollection = new PrivateFontCollection();
                collections.Add(interCollection);
                LoadInterFonts(interCollection);
                var discovered = interCollection.Families;
                var regular = discovered.First(f => f.Name == "Inter");
                InterFamilies = [
                    regular,
                    discovered.First(f => f.Name == "Inter Medium"),
                    discovered.First(f => f.Name == "Inter SemiBold"),
                    regular
                ];
                InterLoaded = true;
            }
            catch (Exception ex) when (IsFontFailure(ex))
            {
                InterError = ex.Message;
                InterFamilies = [];
            }

            // Carga de Outfit
            try
            {
                var regular = LoadFamily(OutfitPrefix, "Outfit", "Outfit-Regular.ttf", "Outfit-Bold.ttf");
                OutfitFamilies = [
                    regular,
                    LoadFamily(OutfitPrefix, "Outfit Medium", "Outfit-Medium.ttf"),
                    LoadFamily(OutfitPrefix, "Outfit SemiBold", "Outfit-SemiBold.ttf"),
                    regular
                ];
                OutfitLoaded = true;
            }
            catch (Exception ex) when (IsFontFailure(ex))
            {
                OutfitError = ex.Message;
                OutfitFamilies = [];
            }
        }

        private void LoadInterFonts(PrivateFontCollection collection)
        {
            // Keep Inter's established loading order and shared collection.
            foreach (var file in new[] { "Inter-Regular.ttf", "Inter-Medium.ttf", "Inter-SemiBold.ttf", "Inter-Bold.ttf" })
            {
                using var stream = typeof(KFontManager).Assembly.GetManifestResourceStream(InterPrefix + file)
                    ?? throw new InvalidOperationException($"No se encuentra el recurso tipográfico integrado '{file}'.");
                using var memory = new MemoryStream();
                stream.CopyTo(memory);
                var bytes = memory.ToArray();
                var buffer = Marshal.AllocHGlobal(bytes.Length);
                buffers.Add(buffer);
                Marshal.Copy(bytes, 0, buffer, bytes.Length);
                var handle = AddFontMemResourceEx(buffer, (uint)bytes.Length, IntPtr.Zero, out uint count);
                if (handle != IntPtr.Zero) registrations.Add(handle);
                if (handle == IntPtr.Zero || count == 0)
                    throw new InvalidOperationException($"Windows no pudo registrar la fuente privada '{file}'.");
                collection.AddMemoryFont(buffer, bytes.Length);
            }
        }

        private FontFamily LoadFamily(string prefix, string name, params string[] files)
        {
            // Separate collections prevent GDI+ from intermittently omitting
            // Medium and SemiBold when multiple private families share one collection.
            var collection = new PrivateFontCollection();
            collections.Add(collection);
            var pendingRegistrations = new List<(IntPtr Buffer, int Length, string File)>();
            foreach (var file in files)
            {
                using var stream = typeof(KFontManager).Assembly.GetManifestResourceStream(prefix + file)
                    ?? throw new InvalidOperationException($"No se encuentra el recurso tipográfico integrado '{file}'.");
                using var memory = new MemoryStream();
                stream.CopyTo(memory);
                var bytes = memory.ToArray();
                var buffer = Marshal.AllocHGlobal(bytes.Length);
                buffers.Add(buffer);
                Marshal.Copy(bytes, 0, buffer, bytes.Length);
                collection.AddMemoryFont(buffer, bytes.Length);
                pendingRegistrations.Add((buffer, bytes.Length, file));
            }
            var families = collection.Families;
            // GDI+ can expose a weight-specific TTF under its base family name.
            // The collection contains only this weight, so its sole family is safe to use.
            var family = families.FirstOrDefault(f => f.Name == name)
                ?? (families.Length == 1 ? families[0] : null)
                ?? throw new InvalidOperationException($"No se encuentra la familia '{name}' después de cargar {string.Join(", ", files)}. Disponibles: {string.Join(", ", families.Select(f => f.Name))}.");
            foreach (var (buffer, length, file) in pendingRegistrations)
            {
                var handle = AddFontMemResourceEx(buffer, (uint)length, IntPtr.Zero, out uint count);
                if (handle != IntPtr.Zero) registrations.Add(handle);
                if (handle == IntPtr.Zero || count == 0)
                    throw new InvalidOperationException($"Windows no pudo registrar la fuente privada '{file}'.");
            }
            return family;
        }
    }

    [DllImport("gdi32.dll")] private static extern IntPtr AddFontMemResourceEx(IntPtr data, uint size, IntPtr reserved, out uint count);
    [DllImport("gdi32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool RemoveFontMemResourceEx(IntPtr handle);
}
