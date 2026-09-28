using KovaUI.Controls;
using KovaUI.Styling;

namespace KovaUI.Demo;

internal sealed class SuiteForm : Form
{
    private readonly List<Control> samples = [];
    private readonly KPanel card;
    private readonly KProgressBar progress;
    private readonly KLabel value;
    private int quantity = 3;
    private bool dark;
    private readonly Font demoFont = KTypography.Body;
    private readonly Font counterFont = KFonts.Inter(13, KFontWeight.SemiBold);
    public SuiteForm()
    {
        Text = "KovaUI · Nuevos controles"; Font = demoFont; ClientSize = new Size(880, 690);
        AutoScaleMode = AutoScaleMode.Dpi; BackColor = KTheme.Light.Background;
        var title = new KLabel { Text = "Una familia, más posibilidades.", TextRole = KTextRole.Title, AutoSize = true, Location = new Point(32, 25) };
        var intro = new KLabel { Text = "Selección nativa · movimiento discreto · todos los colores editables", AutoSize = true, Location = new Point(35, 80), TextRole = KTextRole.Subtitle };
        Controls.AddRange([title, intro]); samples.AddRange([title, intro]);
        card = new KPanel { Location = new Point(24, 112), Size = new Size(830, 466), Padding = new Padding(20), CornerRadius = 16 };
        card.Shadow.Enabled = true; card.Shadow.Blur = 10; card.Shadow.Opacity = 20;
        Controls.Add(card); samples.Add(card);
        AddLabel("SELECCIÓN", 28, 28);
        var check = new KCheckBox { Text = "Recibir novedades", Checked = true, Location = new Point(30, 56), Size = new Size(240, 36) };
        var mixed = new KCheckBox { Text = "Selección parcial", ThreeState = true, CheckState = CheckState.Indeterminate, Location = new Point(300, 56), Size = new Size(220, 36) };
        var off = new KCheckBox { Text = "Deshabilitado", Enabled = false, Location = new Point(550, 56), Size = new Size(240, 36) };
        card.Controls.AddRange([check, mixed, off]); samples.AddRange([check, mixed, off]);
        var radio = new KRadioButton { Text = "Estándar", Checked = true, Location = new Point(30, 105), Size = new Size(240, 34) };
        var premium = new KRadioButton { Text = "Prioritario", Location = new Point(300, 105), Size = new Size(240, 34) };
        card.Controls.AddRange([radio, premium]); samples.AddRange([radio, premium]);
        AddLabel("DESTINO", 28, 159);
        var combo = new KComboBox { Location = new Point(30, 187), Width = 290, AccessibleName = "Destino" };
        combo.Items.AddRange(["Ciudad de Guatemala", "Antigua Guatemala", "Quetzaltenango"]); combo.SelectedIndex = 0;
        card.Controls.Add(combo); samples.Add(combo);
        AddLabel("CANTIDAD", 420, 159);
        var stepper = new KPanel { Location = new Point(422, 183), Size = new Size(232, 58), CornerRadius = 18, Padding = Padding.Empty };
        card.Controls.Add(stepper); samples.Add(stepper);
        var minus = new KButton { Text = "−", Variant = KButtonVariant.Ghost, BorderThickness = 0, Location = new Point(5, 5), Size = new Size(60, 48), CornerRadius = 14, AccessibleName = "Disminuir cantidad" };
        var plus = new KButton { Text = "+", Variant = KButtonVariant.Ghost, BorderThickness = 0, Location = new Point(166, 5), Size = new Size(60, 48), CornerRadius = 14, AccessibleName = "Aumentar cantidad" };
        value = new KLabel { AutoSize = false, Text = "3", TextAlign = ContentAlignment.MiddleCenter, Font = counterFont, Location = new Point(68, 5), Size = new Size(96, 48), BackColor = Color.Transparent };
        stepper.Controls.AddRange([minus, value, plus]); samples.AddRange([minus, value, plus]);
        stepper.Paint += (_, e) => { using var pen = new Pen(stepper.Theme.Border); e.Graphics.DrawLine(pen, 67, 13, 67, 44); e.Graphics.DrawLine(pen, 164, 13, 164, 44); };
        minus.Click += (_, _) => { quantity = Math.Max(0, quantity - 1); value.Text = quantity.ToString(); };
        plus.Click += (_, _) => { quantity = Math.Min(99, quantity + 1); value.Text = quantity.ToString(); };
        AddLabel("PROGRESO", 28, 276);
        progress = new KProgressBar { Location = new Point(30, 310), Size = new Size(590, 16), Value = 38 };
        card.Controls.Add(progress); samples.Add(progress);
        var advance = new KButton { Text = "Avanzar", Location = new Point(647, 294), Size = new Size(140, 44) };
        advance.Click += (_, _) => progress.Value = progress.Value >= 100 ? 0 : Math.Min(100, progress.Value + 15);
        card.Controls.Add(advance); samples.Add(advance);
        var marquee = new KProgressBar { Location = new Point(30, 353), Size = new Size(590, 8), Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 55 };
        marquee.Fill.FillColor = Color.FromArgb(120, 126, 134); card.Controls.Add(marquee); samples.Add(marquee);
        AddLabel("Presiona y suelta los botones para ver la respuesta y la onda de clic.", 28, 394);
        var customize = new KButton { Text = "Personalizar…", Variant = KButtonVariant.Outline, Location = new Point(40, 602), Size = new Size(172, 42) };
        customize.Click += (_, _) => new AppearanceInspector(samples).Show(this);
        var themeButton = new KButton { Text = "Claro / oscuro", Variant = KButtonVariant.Outline, Location = new Point(228, 602), Size = new Size(172, 42) };
        themeButton.Click += (_, _) => { dark = !dark; ApplyTheme(); };
        var motion = new KButton { Text = "Animaciones sí / no", Variant = KButtonVariant.Outline, Location = new Point(416, 602), Size = new Size(205, 42) };
        motion.Click += (_, _) => { KAnimationSettings.Enabled = !KAnimationSettings.Enabled; Invalidate(true); };
        Controls.AddRange([customize, themeButton, motion]); ApplyTheme();
    }
    private void AddLabel(string text, int x, int y) { var label = new KLabel { Text = text, TextRole = text.Length < 20 ? KTextRole.Section : KTextRole.Caption, Location = new Point(x, y) }; card.Controls.Add(label); samples.Add(label); }
    protected override void Dispose(bool disposing) { base.Dispose(disposing); if (disposing) { demoFont.Dispose(); counterFont.Dispose(); } }
    internal void ApplyDark() { dark = true; ApplyTheme(); }
    private void ApplyTheme()
    {
        var theme = dark ? KTheme.Dark : KTheme.Light; BackColor = theme.Background;
        KThemeManager.Apply(this, theme); ForeColor = theme.TextPrimary;
    }
}
