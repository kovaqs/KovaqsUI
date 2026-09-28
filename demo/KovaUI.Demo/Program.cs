using KovaUI.Controls;
using KovaUI.Styling;

namespace KovaUI.Demo;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (args.Length > 0 && args[0].StartsWith("--suite"))
        {
            using var suite = new SuiteForm();
            if (args[0].Contains("dark")) suite.ApplyDark();
            if (args.Length == 2) suite.Shown += (_, _) => suite.BeginInvoke(() => { KAnimationSettings.Enabled = false; using var bitmap = new Bitmap(suite.Width, suite.Height); suite.DrawToBitmap(bitmap, new Rectangle(0, 0, suite.Width, suite.Height)); bitmap.Save(Path.GetFullPath(args[1])); suite.Close(); });
            Application.Run(suite); return;
        }
        using var form = new DemoForm();
        if (args.Length == 2 && (args[0] == "--snapshot" || args[0] == "--snapshot-dark" || args[0] == "--snapshot-inspector"))
        {
            if (args[0] == "--snapshot-dark") form.SetDark();
            form.Shown += (_, _) => form.BeginInvoke(() =>
            {
                Form target = args[0] == "--snapshot-inspector" ? form.OpenInspector() : form;
                using var bitmap = new Bitmap(target.Width, target.Height);
                target.DrawToBitmap(bitmap, new Rectangle(0, 0, target.Width, target.Height));
                bitmap.Save(Path.GetFullPath(args[1]));
                form.Close();
            });
        }
        Application.Run(form);
    }
}

internal sealed class DemoForm : Form
{
    private readonly KLabel status = new() { AutoSize = true, Text = "Tab para navegar · Espacio / Enter para activar · Alt+A para agregar" };
    private readonly FlowLayoutPanel content = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(36, 28, 36, 24) };
    private readonly List<Control> samples = [];
    private readonly List<Image> ownedImages = [];
    private bool dark;
    private AppearanceInspector? inspector;
    public DemoForm()
    {
        Text = "KovaUI · Control sandbox";
        Font = new Font("Segoe UI", 10f);
        ClientSize = new Size(1030, 870);
        MinimumSize = new Size(780, 640);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = KTheme.Light.Background;
        Controls.Add(content);
        AddLabel("KovaUI", 26, true);
        AddLabel("Controles nativos. Un lenguaje visual propio.", 11);
        AddLabel("01  /  ACCIONES", 9, true);
        var actions = Row();
        var add = Button("&Agregar", KButtonVariant.Primary); actions.Controls.Add(add);
        add.Image = CreateDemoIcon("add", Color.FromArgb(20, 22, 24));
        add.TextImageRelation = TextImageRelation.ImageBeforeText;
        actions.Controls.Add(Button("Editar", KButtonVariant.Secondary));
        actions.Controls.Add(Button("Eliminar", KButtonVariant.Danger));
        actions.Controls.Add(Button("Más opciones", KButtonVariant.Outline));
        actions.Controls.Add(Button("Cancelar", KButtonVariant.Ghost));
        AcceptButton = add;
        AddLabel("02  /  TEXTO Y EDICIÓN NATIVA", 9, true);
        var fields = Row();
        var name = Field("Nombre de usuario"); name.AccessibleName = "Nombre de usuario";
        name.Text = "Andrea Smith";
        name.IconLeft = CreateDemoIcon("search", Color.Gray);
        name.IconRight = CreateDemoIcon("clear", Color.Gray);
        name.IconRightClick += (_, _) => name.Clear();
        name.TextChanged += (_, _) => status.Text = $"TextChanged · {name.TextLength} caracteres";
        fields.Controls.Add(name);
        var password = Field("Contraseña", password: true); password.Text = "contraseña"; fields.Controls.Add(password);
        var ro = Field(""); ro.Text = "Solo lectura · selección disponible"; ro.ReadOnly = true; fields.Controls.Add(ro);
        var second = Row();
        var notes = Field("Notas multilínea…"); notes.Multiline = true; notes.Size = new Size(420, 86); notes.AcceptsReturn = true; notes.ScrollBars = ScrollBars.Vertical; second.Controls.Add(notes);
        notes.Text = "Escribe aquí…\r\nSelección y edición nativas.";
        var disabled = Field("Campo deshabilitado"); disabled.Enabled = false; second.Controls.Add(disabled);
        disabled.Text = "Deshabilitado · color configurable";
        AddLabel("03  /  FORMAS, COLORES Y DEGRADADOS EDITABLES", 9, true);
        var custom = Row();
        var gradient = Button("Degradado", KButtonVariant.Primary);
        gradient.FillColor = Color.FromArgb(255, 164, 80); gradient.GradientColor = Color.FromArgb(255, 116, 14);
        gradient.Appearance.Hover.FillColor = Color.FromArgb(255, 182, 109);
        gradient.Appearance.Pressed.FillColor = Color.FromArgb(231, 112, 23);
        gradient.Appearance.Hover.GradientAngle = 0;
        gradient.Appearance.Pressed.GradientMode = KGradientMode.Solid;
        gradient.ForeColor = Color.FromArgb(30, 30, 30); custom.Controls.Add(gradient);
        var capsule = Button("Cápsula", KButtonVariant.Outline); capsule.Shape = KShape.Capsule; custom.Controls.Add(capsule);
        var square = Button("Rectangular", KButtonVariant.Secondary); square.Shape = KShape.Rectangle; custom.Controls.Add(square);
        var violet = Button("Tu paleta", KButtonVariant.Primary); violet.FillColor = Color.FromArgb(100, 72, 178); violet.ForeColor = Color.White;
        violet.BorderColor = Color.FromArgb(100, 72, 178); violet.Appearance.Hover.FillColor = Color.FromArgb(119, 89, 196); custom.Controls.Add(violet);
        AddLabel("04  /  TAMAÑOS Y ESTADOS", 9, true);
        var sizes = Row();
        foreach (var height in new[] { 32, 40, 48 }) { var b = Button($"{height} px", KButtonVariant.Outline); b.Height = height; sizes.Controls.Add(b); }
        var off = Button("Deshabilitado", KButtonVariant.Primary); off.Enabled = false; sizes.Controls.Add(off);
        var longText = Button("Texto largo adaptable", KButtonVariant.Secondary); longText.AutoSize = true; sizes.Controls.Add(longText);
        AddLabel("05  /  PRUEBAS INTERACTIVAS", 9, true);
        var settings = Row();
        var themeButton = Button("Cambiar tema", KButtonVariant.Outline); settings.Controls.Add(themeButton); samples.Remove(themeButton);
        themeButton.Click += (_, _) => { dark = !dark; ApplyTheme(); };
        var enabledButton = Button("Enabled sí / no", KButtonVariant.Outline); enabledButton.Width = 170; settings.Controls.Add(enabledButton); samples.Remove(enabledButton);
        enabledButton.Click += (_, _) => { foreach (var control in samples) control.Enabled = !control.Enabled; };
        var fontButton = Button("Fuente 10 / 13", KButtonVariant.Outline); settings.Controls.Add(fontButton); samples.Remove(fontButton);
        fontButton.Click += (_, _) => { var previous = Font; Font = new Font("Segoe UI", Font.Size < 12 ? 13 : 10); previous.Dispose(); };
        var inspectButton = Button("Personalizar…", KButtonVariant.Outline); settings.Controls.Add(inspectButton); samples.Remove(inspectButton);
        inspectButton.Click += (_, _) => OpenInspector();
        var suiteButton = Button("Más controles…", KButtonVariant.Outline); settings.Controls.Add(suiteButton); samples.Remove(suiteButton);
        suiteButton.Click += (_, _) => new SuiteForm().Show(this);
        status.Margin = new Padding(0, 18, 0, 0); content.Controls.Add(status);
        ApplyTheme();
    }
    internal AppearanceInspector OpenInspector()
    {
        if (inspector is null || inspector.IsDisposed)
        {
            inspector = new AppearanceInspector(samples);
            inspector.Show(this);
        }
        else inspector.Activate();
        return inspector;
    }
    private void AddLabel(string text, float size, bool bold = false)
    {
        var label = new KLabel { Text = text, TextRole = size >= 20 ? KTextRole.Title : bold ? KTextRole.Section : KTextRole.Subtitle, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), Margin = new Padding(0, 0, 0, size < 10 ? 10 : 14) }; content.Controls.Add(label); samples.Add(label);
    }
    private Image CreateDemoIcon(string glyph, Color color)
    {
        var image = DemoIcons.Create(glyph, color);
        ownedImages.Add(image);
        return image;
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) { foreach (var image in ownedImages) image.Dispose(); ownedImages.Clear(); }
    }
    private FlowLayoutPanel Row()
    {
        var row = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, MaximumSize = new Size(940, 0), Margin = new Padding(0, 0, 0, 20) };
        content.Controls.Add(row); return row;
    }
    private KButton Button(string text, KButtonVariant variant)
    {
        var b = new KButton { Text = text, Variant = variant, Width = 145, Height = 42, Margin = new Padding(0, 0, 12, 8), AccessibleName = text.Replace("&", "") };
        b.Click += (_, _) => status.Text = $"Click · {b.Text.Replace("&", "")} · {DateTime.Now:T}";
        samples.Add(b); return b;
    }
    private KTextBox Field(string placeholder, bool password = false)
    {
        var field = new KTextBox { PlaceholderText = placeholder, UseSystemPasswordChar = password, Width = 290, Height = 44, Margin = new Padding(0, 0, 12, 8) };
        samples.Add(field); return field;
    }
    private void ApplyTheme()
    {
        var theme = dark ? KTheme.Dark : KTheme.Light;
        BackColor = theme.Background;
        KThemeManager.Apply(this, theme);

    }
    public void SetDark() { dark = true; ApplyTheme(); }

}




