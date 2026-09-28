using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace KovaUI.Styling;

/// <summary>Gestor centralizado de recursos tipográficos y colecciones privadas en memoria.</summary>
public static class KFontManager
{
    private const string InterPrefix = "KovaUI.Fonts.Inter.";
    private const string OutfitPrefix = "KovaUI.Fonts.Outfit.";

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

    private static Font CreateFallback(float size, KFontWeight weight) =>
        new("Segoe UI", size, weight is KFontWeight.Bold or KFontWeight.SemiBold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Point);

    private static bool IsFontFailure(Exception ex) => ex is ArgumentException or ExternalException or IOException
        or InvalidOperationException or NotSupportedException or TypeInitializationException or DllNotFoundException
        or EntryPointNotFoundException or UnauthorizedAccessException or OutOfMemoryException;

    private sealed class FontResources
    {
        private PrivateFontCollection? collection;
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
            if (AppContext.TryGetSwitch("KovaUI.DisableEmbeddedFonts", out bool disabled) && disabled)
            {
                InterError = "Las fuentes integradas están desactivadas para este proceso.";
                OutfitError = InterError;
                return;
            }

            collection = new PrivateFontCollection();

            // Carga de Inter
            try
            {
                LoadFamily(collection, InterPrefix, ["Inter-Regular.ttf", "Inter-Medium.ttf", "Inter-SemiBold.ttf", "Inter-Bold.ttf"]);
                var discovered = collection.Families;
                InterFamilies = [
                    Find(discovered, "Inter"),
                    Find(discovered, "Inter Medium"),
                    Find(discovered, "Inter SemiBold"),
                    Find(discovered, "Inter")
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
                LoadFamily(collection, OutfitPrefix, ["Outfit-Regular.ttf", "Outfit-Medium.ttf", "Outfit-SemiBold.ttf", "Outfit-Bold.ttf"]);
                var discovered = collection.Families;
                OutfitFamilies = [
                    Find(discovered, "Outfit"),
                    Find(discovered, "Outfit Medium"),
                    Find(discovered, "Outfit SemiBold"),
                    Find(discovered, "Outfit")
                ];
                OutfitLoaded = true;
            }
            catch (Exception ex) when (IsFontFailure(ex))
            {
                OutfitError = ex.Message;
                OutfitFamilies = [];
            }

            FontFamily Find(FontFamily[] families, string name) => families.First(f => f.Name == name);
        }

        private void LoadFamily(PrivateFontCollection pfc, string prefix, string[] files)
        {
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
                var handle = AddFontMemResourceEx(buffer, (uint)bytes.Length, IntPtr.Zero, out uint count);
                if (handle != IntPtr.Zero) registrations.Add(handle);
                if (handle == IntPtr.Zero || count == 0)
                    throw new InvalidOperationException($"Windows no pudo registrar la fuente privada '{file}'.");
                pfc.AddMemoryFont(buffer, bytes.Length);
            }
        }
    }

    [DllImport("gdi32.dll")] private static extern IntPtr AddFontMemResourceEx(IntPtr data, uint size, IntPtr reserved, out uint count);
    [DllImport("gdi32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool RemoveFontMemResourceEx(IntPtr handle);
}
