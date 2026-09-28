using System.ComponentModel;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Text;
using KovaqsUI.Controls;
using KovaqsUI.Styling;

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
        foreach (var weight in Enum.GetValues<KFontWeight>())
        {
            using var font = KFonts.Outfit(10, weight);
            var dc = CreateCompatibleDC(IntPtr.Zero); var handle = font.ToHfont(); var previous = SelectObject(dc, handle);
            try
            {
                var name = new StringBuilder(128); GetTextFace(dc, name.Capacity, name);
                GetTextMetrics(dc, out var metrics);
                int expected = weight switch { KFontWeight.Regular => 400, KFontWeight.Medium => 500, KFontWeight.SemiBold => 600, _ => 700 };
                check(name.ToString().StartsWith("Outfit") && metrics.Weight == expected,
                    $"GDI selects the real Outfit {weight} face ({name}, weight {metrics.Weight})");
            }
            finally { SelectObject(dc, previous); DeleteObject(handle); DeleteDC(dc); }
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

        // Pruebas de preservación tipográfica al cambiar tamaño y peso (evitar degradación a Microsoft Sans Serif)
        var converter = new KFontConverter();

        // 1. Simulación de PropertyGrid cambiando tamaño de fuente privada
        var propValues = new System.Collections.Hashtable
        {
            ["Name"] = "Inter",
            ["Size"] = 14f,
            ["Unit"] = GraphicsUnit.Point,
            ["Bold"] = false,
            ["Italic"] = false,
            ["Strikeout"] = false,
            ["Underline"] = false
        };
        using (var convertedFont = (Font)converter.CreateInstance(null, propValues))
        {
            check(convertedFont.Name == "Inter" && Math.Abs(convertedFont.Size - 14f) < 0.01f,
                "KFontConverter.CreateInstance preserva Inter al cambiar tamaño a 14pt");
        }

        // 2. Simulación de PropertyGrid con peso Medium
        propValues["Name"] = "Inter Medium";
        propValues["Size"] = 16f;
        using (var mediumFont = (Font)converter.CreateInstance(null, propValues))
        {
            check(mediumFont.Name == "Inter Medium" && Math.Abs(mediumFont.Size - 16f) < 0.01f,
                "KFontConverter.CreateInstance preserva Inter Medium al cambiar tamaño a 16pt");
        }

        // 3. Simulación de PropertyGrid con Outfit
        propValues["Name"] = "Outfit";
        propValues["Size"] = 18f;
        using (var outfitFont = (Font)converter.CreateInstance(null, propValues))
        {
            check(outfitFont.Name == "Outfit" && Math.Abs(outfitFont.Size - 18f) < 0.01f,
                "KFontConverter.CreateInstance preserva Outfit al cambiar tamaño a 18pt");
        }

        // 4. ConvertFrom desde texto
        using (var parsedInter = (Font)converter.ConvertFrom(null, null, "Inter, 13pt")!)
        {
            check(parsedInter.Name.StartsWith("Inter") && Math.Abs(parsedInter.Size - 13f) < 0.01f,
                "KFontConverter.ConvertFrom('Inter, 13pt') preserva Inter");
        }

        // 5. Verificación de TypeConverter y resolución en cada control
        foreach (var control in controls)
        {
            var prop = TypeDescriptor.GetProperties(control)["Font"]!;
            check(prop.Converter is KFontConverter, $"{control.GetType().Name} declara KFontConverter como convertidor de Font");

            // Simulación de código generado por diseñador o usuario: new Font("Inter", 15f)
            // En un sistema sin Inter instalada, new Font("Inter", 15f) tiene Name == "Microsoft Sans Serif"
            using var rawDegraded = new Font("Inter", 15f);
            if (!installedInter)
            {
                check(rawDegraded.Name == "Microsoft Sans Serif" && rawDegraded.OriginalFontName == "Inter",
                    "GDI+ efectivamente degrada new Font('Inter') a Microsoft Sans Serif en ausencia de instalación en Windows");
            }
            control.Font = rawDegraded;
            check(control.Font.Name.StartsWith("Inter") && Math.Abs(control.Font.Size - 15f) < 0.01f,
                $"{control.GetType().Name}.Font repara automáticamente la degradación a Microsoft Sans Serif restaurando Inter");

            // Simulación de new Font(control.Font.Name, 17f)
            using var fromName = new Font(control.Font.Name, 17f);
            control.Font = fromName;
            check(control.Font.Name.StartsWith("Inter") && Math.Abs(control.Font.Size - 17f) < 0.01f,
                $"{control.GetType().Name}.Font preserva Inter al reconstruir mediante new Font(control.Font.Name, 17f)");

            // Métodos de extensión WithSize y WithWeight
            using var resized = control.Font.WithSize(20f);
            control.Font = resized;
            check(control.Font.Name.StartsWith("Inter") && Math.Abs(control.Font.Size - 20f) < 0.01f,
                $"{control.GetType().Name}.Font.WithSize(20f) aplica tamaño preservando Inter");

            using var weighted = control.Font.WithWeight(KFontWeight.Bold);
            control.Font = weighted;
            check(control.Font.Name.StartsWith("Inter") && control.Font.Bold,
                $"{control.GetType().Name}.Font.WithWeight(Bold) aplica peso en negrita");

            // Asignación intencional de Microsoft Sans Serif debe respetarse
            using var intentionalMsSans = new Font("Microsoft Sans Serif", 9f);
            control.Font = intentionalMsSans;
            check(control.Font.Name == "Microsoft Sans Serif" && Math.Abs(control.Font.Size - 9f) < 0.01f,
                $"{control.GetType().Name}.Font respeta asignación deliberada de Microsoft Sans Serif");

            // Reset restores default
            prop.ResetValue(control);
        }
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
