using System.ComponentModel;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Text;
using KovaUI.Controls;
using KovaUI.Styling;

internal static class TypographyTests
{
    internal static void Run(Action<bool, string> check)
    {
        using var installed = new InstalledFontCollection();
        var systemFamilies = installed.Families;
        bool installedInter = systemFamilies.Any(f => f.Name.StartsWith("Inter", StringComparison.Ordinal));
        foreach (var family in systemFamilies) family.Dispose();
        Console.WriteLine($"INFO: Inter installed in Windows: {installedInter}");

        Parallel.For(0, 16, i => { using var f = KFonts.Inter(10 + i % 3, (KFontWeight)(i % 4)); });
        check(KFonts.InterLoaded, "Inter resources load once and support concurrent callers");
        check(KFonts.OutfitLoaded, "Outfit resources load once and support concurrent callers");

        foreach (var weight in Enum.GetValues<KFontWeight>())
        {
            using var font = KFonts.Inter(10, weight);
            var dc = CreateCompatibleDC(IntPtr.Zero); var handle = font.ToHfont(); var previous = SelectObject(dc, handle);
            try
            {
                var name = new StringBuilder(128); GetTextFace(dc, name.Capacity, name);
                GetTextMetrics(dc, out var metrics);
                int expected = weight switch { KFontWeight.Regular => 400, KFontWeight.Medium => 500, KFontWeight.SemiBold => 600, _ => 700 };
                check(name.ToString().StartsWith("Inter") && metrics.Weight == expected,
                    $"GDI selects the real Inter {weight} face ({name}, weight {metrics.Weight})");
            }
            finally { SelectObject(dc, previous); DeleteObject(handle); DeleteDC(dc); }
            foreach (int dpi in new[] { 96, 120, 144, 192 })
            {
                using var bitmap = new Bitmap(100, 100); bitmap.SetResolution(dpi, dpi);
                using var graphics = Graphics.FromImage(bitmap);
                check(font.Unit == GraphicsUnit.Point && Math.Abs(font.GetHeight(graphics) / font.GetHeight(96) - dpi / 96f) < .01f,
                    $"Inter {weight} point metrics scale at {dpi} DPI");
            }
        }
        using var form = new Form();
        Control[] controls = [new KButton(), new KTextBox(), new KCheckBox(), new KRadioButton(), new KComboBox(), new KPanel(), new KProgressBar(), new KLabel()];
        form.Controls.AddRange(controls);
        foreach (var control in controls)
        {
            var property = TypeDescriptor.GetProperties(control)["Font"]!;
            check(control.Font.Name.StartsWith("Inter") && !property.ShouldSerializeValue(control), $"{control.GetType().Name} has a nonserialized Inter default");
            using var custom = new Font("Arial", 12);
            control.Font = custom;
            check(property.ShouldSerializeValue(control) && control.Font == custom, $"{control.GetType().Name} preserves a custom Font");
            property.ResetValue(control);
            check(control.Font.Name.StartsWith("Inter") && !property.ShouldSerializeValue(control), $"{control.GetType().Name} restores Inter through designer reset");
        }
        var field = controls.OfType<KTextBox>().Single();
        check(field.Editor.Font.Equals(field.Font), "native text editor inherits Inter");
        check(KFonts.InterLicense.Contains("The Inter Project Authors") && KFonts.InterLicense.Contains("SIL OPEN FONT LICENSE"), "standalone DLL exposes original Inter font notices");
        check(KFonts.OutfitLicense.Contains("Copyright 2021 The Outfit Project Authors") && KFonts.OutfitLicense.Contains("SIL OPEN FONT LICENSE"), "standalone DLL exposes original Outfit font notices");
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct Metrics
    {
        public int Height, Ascent, Descent, Internal, External, Average, Maximum, Weight, Overhang, AspectX, AspectY;
        public char First, Last, Default, Break;
        public byte Italic, Underlined, Strikeout, Pitch, Charset;
    }
    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr dc, IntPtr value);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr value);
    [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr dc);
    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)] private static extern int GetTextFace(IntPtr dc, int count, StringBuilder name);
    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)] private static extern bool GetTextMetrics(IntPtr dc, out Metrics metrics);
}
