using System.ComponentModel;
using System.Runtime.InteropServices;
using KovaqsUI.Controls;
using KovaqsUI.Styling;

internal static class Program
{
    private static int checks;
    [DllImport("user32.dll")] private static extern int GetGuiResources(IntPtr process, uint flags);
    [STAThread]
    private static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (args.Contains("--font-design"))
        {
            LicenseManager.CurrentContext = new System.ComponentModel.Design.DesigntimeLicenseContext();
            using var host = new Form();
            host.Controls.AddRange([new KButton(), new KTextBox(), new KCheckBox(), new KRadioButton(), new KComboBox(), new KPanel(), new KProgressBar(), new KLabel()]);
            bool safe = LicenseManager.UsageMode == LicenseUsageMode.Designtime && host.Controls.Cast<Control>().All(c => c.Font.Name.StartsWith("Inter"));
            if (!safe)
                Console.WriteLine($"INFO: InterLoaded={KFonts.InterLoaded}; error={KFontManager.InterLoadError}; fonts={string.Join(", ", host.Controls.Cast<Control>().Select(c => c.Font.Name))}");
            Console.WriteLine(safe ? "PASS: Inter inicializa bajo LicenseUsageMode.Designtime en los ocho controles" : "FAIL: tipografía en diseño");
            return safe ? 0 : 1;
        }
        if (args.Contains("--font-fallback") || args.Contains("--font-fallback-legacy"))
        {
            AppContext.SetSwitch(args.Contains("--font-fallback-legacy")
                ? "KovaUI.DisableEmbeddedFonts"
                : "KovaqsUI.DisableEmbeddedFonts", true);
            using var button = new KButton();
            using var title = new KLabel { TextRole = KTextRole.Title };
            using var field = new KTextBox();
            bool safe = !KFonts.InterLoaded && button.Font.Name == "Segoe UI" && title.Font.Name == "Segoe UI" && field.Editor.Font.Name == "Segoe UI";
            Console.WriteLine(safe ? "PASS: fallback seguro sin fuentes privadas" : "FAIL: fallback");
            return safe ? 0 : 1;
        }
        KAnimationSettings.Enabled = false;
        try
        {
            using var form = new TestForm { Size = new Size(720, 480) };
            using var button = new TestButton { Text = "&Apply", Location = new Point(20, 20) };
            using var field = new KTextBox { Location = new Point(20, 90), AccessibleName = "Name" };
            form.Controls.AddRange([button, field]);
            form.Show(); Application.DoEvents();
            int clicks = 0; button.Click += (_, _) => clicks++;
            button.PerformClick(); Check(clicks == 1, "PerformClick");
            button.Enabled = false; button.PerformClick(); Check(clicks == 1, "disabled suppresses click"); button.Enabled = true;
            button.Mnemonic('a'); Check(clicks == 2, "mnemonic activation");
            form.AcceptButton = button; Check(form.AcceptButton == button, "IButtonControl / AcceptButton");
            button.DialogResult = DialogResult.OK; Check(((IButtonControl)button).DialogResult == DialogResult.OK, "DialogResult"); button.DialogResult = DialogResult.None;
            button.Focus(); button.KeyPressForTest(Keys.Space); Check(clicks == 3, "Space activation");
            form.DialogKey(Keys.Enter); Check(clicks == 4, "Enter / default button");
            using var cancel = new KButton { Text = "Cancel", Location = new Point(180, 20) };
            form.Controls.Add(cancel); form.CancelButton = cancel;
            int cancellations = 0; cancel.Click += (_, _) => cancellations++;
            form.DialogKey(Keys.Escape); Check(cancellations == 1, "Escape / cancel button");
            int changes = 0; field.TextChanged += (_, _) => changes++;
            field.Text = "Unicode: á 日本語 🟠"; Check(changes == 1 && field.Editor.Text == field.Text, "TextChanged once / Unicode");
            field.Editor.Text = "abcdef"; Check(changes == 2 && field.Text == "abcdef", "native editor synchronizes wrapper");
            field.Select(1, 3); Check(field.SelectedText == "bcd", "selection");
            field.SelectedText = "XYZ"; Check(field.Text == "aXYZef", "replace selected text");
            field.SelectAll(); Check(field.SelectionLength == field.TextLength, "SelectAll");
            field.ReadOnly = true; Check(field.Editor.ReadOnly, "ReadOnly"); field.ReadOnly = false;
            field.UseSystemPasswordChar = true; Check(field.Editor.UseSystemPasswordChar, "password"); field.UseSystemPasswordChar = false;
            field.Multiline = true; field.Size = new Size(300, 120); field.Lines = ["one", "two"];
            Check(field.Editor.Multiline && field.Lines.Length == 2 && field.Editor.Height > 50, "multiline layout");
            field.AcceptsTab = true; field.AcceptsReturn = true; field.MaxLength = 100; field.ShortcutsEnabled = true;
            Check(field.Editor.AcceptsTab && field.Editor.AcceptsReturn && field.Editor.MaxLength == 100, "native configuration");
            field.Focus(); Check(field.ContainsFocus && field.Editor.Focused, "focus forwarding");
            Check(field.Editor.AccessibleName == "Name", "native accessibility name");
            var appearance = TypeDescriptor.GetProperties(button)[nameof(KButton.Appearance)]!;
            Check(appearance.SerializationVisibility == DesignerSerializationVisibility.Content, "designer nested serialization");
            var color = TypeDescriptor.GetProperties(button.Appearance.Hover)["FillColor"]!;
            Check(!color.ShouldSerializeValue(button.Appearance.Hover), "designer omits empty override");
            button.Appearance.Hover.FillColor = Color.Blue;
            Check(color.ShouldSerializeValue(button.Appearance.Hover), "designer persists changed override");
            color.ResetValue(button.Appearance.Hover); Check(button.Appearance.Hover.FillColor.IsEmpty, "designer reset");
            KThemeManager.Apply(form, KTheme.Dark); Check(button.Theme == KTheme.Dark && field.Theme == KTheme.Dark, "theme subtree");
            button.FillColor = Color.Purple; KThemeManager.Apply(form, KTheme.Light); Check(button.FillColor == Color.Purple, "theme preserves local override");
            button.GradientColor = Color.Orange;
            using var bitmap = new Bitmap(300, 160);
            foreach (var shape in Enum.GetValues<KShape>()) { button.Shape = shape; button.DrawToBitmap(bitmap, button.ClientRectangle); }
            foreach (var scale in new[] { 1f, 1.25f, 1.5f, 2f })
            {
                button.Size = new Size((int)(112 * scale), (int)(40 * scale)); button.DrawToBitmap(bitmap, button.ClientRectangle);
            }
            button.Size = new Size(1, 1); button.DrawToBitmap(bitmap, button.ClientRectangle); button.Size = new Size(112, 40);
            field.Size = new Size(1, 1); field.DrawToBitmap(bitmap, field.ClientRectangle); field.Size = new Size(260, 40);
            Check(true, "shapes, size stress and tiny bounds render");
            for (int i = 0; i < 50; i++) button.DrawToBitmap(bitmap, button.ClientRectangle);
            int before = GetGuiResources(System.Diagnostics.Process.GetCurrentProcess().Handle, 0);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++) { button.DrawToBitmap(bitmap, button.ClientRectangle); field.DrawToBitmap(bitmap, field.ClientRectangle); }
            watch.Stop();
            int after = GetGuiResources(System.Diagnostics.Process.GetCurrentProcess().Handle, 0);
            Check(after - before < 15, $"GDI handles stable ({before} → {after})");
            RenderingRegressionTests.Run(Check);
            NavigationRegressionTests.Run(Check);
            ButtonLayoutRegressionTests.Run(Check);
            TextBehaviorRegressionTests.Run(Check);
            NativeEditRegressionTests.Run(Check);
            FocusStateRegressionTests.Run(Check);
            DisabledRenderPerformanceTests.Run(Check);
            DesignerRegressionTests.Run(Check);
            AccessibilityRegressionTests.Run(Check);
            ButtonNativePropertyTests.Run(Check);
            DpiRegressionTests.Run(Check);
            TextPointerStateTests.Run(Check);
            AnimationRegressionTests.Run(Check);
            TypographyTests.Run(Check);
            SuiteRegressionTests.Run(Check);
            Console.WriteLine($"PASS: {checks} checks; 2000 renders in {watch.ElapsedMilliseconds} ms. Size scaling is not a physical per-monitor DPI test.");
            form.Close(); return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
    }
    private static void Check(bool condition, string name) { if (!condition) throw new Exception("FAIL: " + name); checks++; Console.WriteLine("PASS: " + name); }
    private sealed class TestButton : KButton
    {
        public bool Mnemonic(char c) => ProcessMnemonic(c);
        public void KeyPressForTest(Keys key) { OnKeyDown(new KeyEventArgs(key)); OnKeyUp(new KeyEventArgs(key)); }
    }
    private sealed class TestForm : Form { public bool DialogKey(Keys key) => ProcessDialogKey(key); }
}
