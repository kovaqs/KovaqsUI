using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace KovaUI.Styling;

public enum KFontWeight { Regular, Medium, SemiBold, Bold }

/// <summary>Private, process-local fonts. Each returned Font belongs to the caller and must be disposed.</summary>
public static class KFonts
{
    private const string Prefix = "KovaUI.Fonts.Outfit.";
    private static readonly Lazy<FontResources> Resources = new(() => new FontResources(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static bool OutfitLoaded => Resources.Value.Loaded;
    public static string? OutfitLoadError => Resources.Value.Error;
    /// <summary>Original copyright notice and OFL text, also available when distributing only the DLL.</summary>
    public static string OutfitLicense
    {
        get { using var stream = typeof(KFonts).Assembly.GetManifestResourceStream(Prefix + "OFL.txt")!; using var reader = new StreamReader(stream); return reader.ReadToEnd(); }
    }

    /// <summary>Creates an owned font measured in points. Medium and SemiBold use real static faces, not synthetic bold.</summary>
    public static Font Outfit(float size, KFontWeight weight = KFontWeight.Regular)
    {
        if (!float.IsFinite(size) || size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
        if (!Enum.IsDefined(weight)) throw new ArgumentOutOfRangeException(nameof(weight));
        var resources = Resources.Value;
        lock (resources)
        {
            if (resources.Loaded)
            {
                try { return new Font(resources.Families[(int)weight], size, weight == KFontWeight.Bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Point); }
                catch (Exception ex) when (IsFontFailure(ex)) { /* A restricted designer may reject private fonts. */ }
            }
            return new Font("Segoe UI", size, weight is KFontWeight.Bold or KFontWeight.SemiBold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Point);
        }
    }

    private static bool IsFontFailure(Exception ex) => ex is ArgumentException or ExternalException or IOException
        or InvalidOperationException or NotSupportedException or TypeInitializationException or DllNotFoundException
        or EntryPointNotFoundException or UnauthorizedAccessException or OutOfMemoryException;

    // Retained for the process lifetime: disposing a collection while a Font or native HWND still
    // uses it is unsafe. This is a single bounded allocation, not a per-control or per-paint cache.
    private sealed class FontResources
    {
        private PrivateFontCollection? collection;
        private readonly List<IntPtr> buffers = [];
        private readonly List<IntPtr> registrations = [];
        internal FontFamily[] Families = [];
        internal bool Loaded;
        internal string? Error;
        internal FontResources()
        {
            if (AppContext.TryGetSwitch("KovaUI.DisableEmbeddedFonts", out bool disabled) && disabled)
            { Error = "Las fuentes integradas están desactivadas para este proceso."; return; }
            FontFamily[] discovered = [];
            try
            {
                collection = new PrivateFontCollection();
                foreach (var weight in Enum.GetValues<KFontWeight>())
                {
                    using var stream = typeof(KFonts).Assembly.GetManifestResourceStream(Prefix + $"Outfit-{weight}.ttf")
                        ?? throw new InvalidOperationException("No se encuentra el recurso tipográfico integrado.");
                    using var memory = new MemoryStream(); stream.CopyTo(memory);
                    var bytes = memory.ToArray();
                    var buffer = Marshal.AllocHGlobal(bytes.Length); buffers.Add(buffer);
                    Marshal.Copy(bytes, 0, buffer, bytes.Length);
                    var handle = AddFontMemResourceEx(buffer, (uint)bytes.Length, IntPtr.Zero, out uint count);
                    if (handle != IntPtr.Zero) registrations.Add(handle);
                    if (handle == IntPtr.Zero || count == 0) throw new InvalidOperationException("Windows no pudo registrar la fuente privada.");
                    collection.AddMemoryFont(buffer, bytes.Length);
                }
                discovered = collection.Families;
                Families = [Find("Outfit"), Find("Outfit Medium"), Find("Outfit SemiBold"), Find("Outfit")];
                Loaded = true;
                FontFamily Find(string name) => discovered.First(f => f.Name == name);
            }
            catch (Exception ex) when (IsFontFailure(ex))
            {
                Error = ex.Message;
                foreach (var family in discovered) family.Dispose();
                collection?.Dispose(); collection = null;
                foreach (var handle in registrations) RemoveFontMemResourceEx(handle);
                registrations.Clear();
                foreach (var buffer in buffers) Marshal.FreeHGlobal(buffer);
                buffers.Clear(); Families = [];
            }
        }
    }

    [DllImport("gdi32.dll")] private static extern IntPtr AddFontMemResourceEx(IntPtr data, uint size, IntPtr reserved, out uint count);
    [DllImport("gdi32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool RemoveFontMemResourceEx(IntPtr handle);
}
