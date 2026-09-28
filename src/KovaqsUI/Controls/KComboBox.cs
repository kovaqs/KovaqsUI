using System.ComponentModel;
using System.Drawing.Drawing2D;
using KovaqsUI.Core;
using KovaqsUI.Drawing;
using KovaqsUI.Styling;

namespace KovaqsUI.Controls;

/// <summary>Native ComboBox with owner-drawn items and a configurable frame.</summary>
[ToolboxItem(true), DefaultEvent(nameof(SelectedIndexChanged)), DefaultProperty(nameof(Items))]
public class KComboBox : ComboBox, IKThemeControl
{
    private KTheme theme = KTheme.Light;
    private KStateTransition? transition;
    private KScalarTransition? arrowTransition;
    private Color arrowColor = Color.Empty, disabledArrowColor = Color.Empty;
    private float arrowSize = 7, arrowThickness = 1.5f;
    private bool hover;
    private int radius = 8, border = 1;
    private Font? defaultFont;
    [TypeConverter(typeof(KFontConverter))]
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override Font Font { get => base.Font; set => base.Font = KFontManager.ResolveFont(value, defaultFont); }
    public override void ResetFont() => Font = null;
    private bool ShouldSerializeFont() => !Equals(Font, defaultFont);
    public KComboBox()
    {
        base.DrawMode = DrawMode.OwnerDrawFixed;
        base.DropDownStyle = ComboBoxStyle.DropDownList;
        ItemHeight = 28; Size = new Size(240, 36);
        Appearance.Changed += Changed; ItemAppearance.Changed += Changed; SelectedItemAppearance.Changed += Changed;
        Changed(this, EventArgs.Empty);
        base.Font = defaultFont = KTypography.Body;
    }
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] public KAppearance Appearance { get; } = new();
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] public KStateStyle ItemAppearance { get; } = new();
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] public KStateStyle SelectedItemAppearance { get; } = new();
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] public KAnimationStyle Animation { get; } = new();
    [Category("Kova Arrow"), DefaultValue(typeof(Color), "")]
    [Description("Color de la flecha. Vacío hereda el color de texto del estado actual.")]
    public Color ArrowColor { get => arrowColor; set { arrowColor = value; Invalidate(); } }
    [Category("Kova Arrow"), DefaultValue(typeof(Color), "")]
    public Color DisabledArrowColor { get => disabledArrowColor; set { disabledArrowColor = value; Invalidate(); } }
    [Category("Kova Arrow"), DefaultValue(7f)]
    [Description("Ancho de la flecha en píxeles lógicos; cero la oculta.")]
    public float ArrowSize { get => arrowSize; set { if (!float.IsFinite(value) || value < 0 || value > 64) throw new ArgumentOutOfRangeException(nameof(value)); arrowSize = value; Invalidate(); } }
    [Category("Kova Arrow"), DefaultValue(1.5f)]
    public float ArrowThickness { get => arrowThickness; set { if (!float.IsFinite(value) || value < 0 || value > 16) throw new ArgumentOutOfRangeException(nameof(value)); arrowThickness = value; Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(8)] public int CornerRadius { get => radius; set { ArgumentOutOfRangeException.ThrowIfNegative(value); radius = value; Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(1)] public int BorderThickness { get => border; set { ArgumentOutOfRangeException.ThrowIfNegative(value); border = value; Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(typeof(Color), "")] public Color FillColor { get => Appearance.Normal.FillColor; set => Appearance.Normal.FillColor = value; }
    [Category("Kova Appearance"), DefaultValue(typeof(Color), "")] public Color BorderColor { get => Appearance.Normal.BorderColor; set => Appearance.Normal.BorderColor = value; }
    [Category("Kova Appearance"), DefaultValue(typeof(Color), "")] public Color GradientColor { get => Appearance.Normal.GradientColor; set => Appearance.Normal.GradientColor = value; }
    [DefaultValue(ComboBoxStyle.DropDownList)] public new ComboBoxStyle DropDownStyle { get => base.DropDownStyle; set => base.DropDownStyle = value; }
    [DefaultValue(DrawMode.OwnerDrawFixed)] public new DrawMode DrawMode { get => base.DrawMode; set => base.DrawMode = value; }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public KTheme Theme { get => theme; set { theme = value ?? throw new ArgumentNullException(nameof(value)); Changed(this, EventArgs.Empty); } }
    public override Color ForeColor
    {
        get => base.ForeColor;
        set { if (Appearance is null) base.ForeColor = value; else Appearance.Normal.ForeColor = value; }
    }
    public override void ResetForeColor() => ForeColor = Color.Empty;
    private bool ShouldSerializeForeColor() => !Appearance.Normal.ForeColor.IsEmpty;
    public override Color BackColor
    {
        get => base.BackColor;
        set
        {
            if (!value.IsEmpty && value.A != 255) throw new ArgumentException("El editor nativo requiere un fondo opaco.", nameof(value));
            if (Appearance is null) base.BackColor = value; else Appearance.Normal.FillColor = value;
        }
    }
    public override void ResetBackColor() => BackColor = Color.Empty;
    private bool ShouldSerializeBackColor() => !Appearance.Normal.FillColor.IsEmpty;
    private void Changed(object? s, EventArgs e)
    {
        var fill = Appearance.Normal.FillColor.IsEmpty ? theme.Surface : Appearance.Normal.FillColor;
        // The native EDIT surface is opaque even when the painted frame uses alpha.
        base.BackColor = fill.A == 255 ? fill : Color.FromArgb(255, fill);
        base.ForeColor = Appearance.Normal.ForeColor.IsEmpty ? theme.TextPrimary : Appearance.Normal.ForeColor;
        Invalidate();
    }
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        bool selected = (e.State & DrawItemState.Selected) != 0;
        var style = new PaintStyle(selected ? theme.SurfaceHover : theme.Surface, Color.Empty, Enabled ? theme.TextPrimary : theme.DisabledText, Color.Transparent, 90).Apply(ItemAppearance);
        if (selected) style = style.Apply(SelectedItemAppearance);
        if (e.Bounds.Width > 0 && e.Bounds.Height > 0)
        {
            using Brush brush = style.End.IsEmpty || !style.GradientEnabled ? new SolidBrush(style.Fill) : new LinearGradientBrush(e.Bounds, style.Fill, style.End, style.Angle);
            e.Graphics.FillRectangle(brush, e.Bounds);
            string? text = e.Index >= 0 && e.Index < Items.Count ? GetItemText(Items[e.Index]) : Text;
            var area = Rectangle.Inflate(e.Bounds, -8, 0);
            TextRenderer.DrawText(e.Graphics, text, Font, area, style.Text, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix |
                (RightToLeft == RightToLeft.Yes ? TextFormatFlags.RightToLeft | TextFormatFlags.Right : 0));
        }
        base.OnDrawItem(e);
    }
    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (m.Msg == 0xF && IsHandleCreated)
        { using var g = Graphics.FromHwnd(Handle); PaintFrame(g); }
        else if (m.Msg == 0x318 && m.WParam != IntPtr.Zero)
        { using var g = Graphics.FromHdc(m.WParam); PaintFrame(g); }
    }
    private void PaintFrame(Graphics g)
    {
        if (DropDownStyle == ComboBoxStyle.Simple) return; // Native embedded-list layout.
        float scale = DeviceDpi / 96f;
        var style = KRenderer.Resolve(theme, Appearance, KButtonVariant.Outline, true, Enabled, hover, DroppedDown, Focused);
        style = (transition ??= new KStateTransition(this)).Sample(style, Animation);
        var saved = g.Save();
        int arrowWidth = Math.Max(20, (int)(28 * scale));
        if (DropDownStyle == ComboBoxStyle.DropDown)
        {
            // Preserve the real editable child, including caret and text selection.
            var edit = Rectangle.FromLTRB(4, 3, Width - arrowWidth - 2, Height - 3);
            if (RightToLeft == RightToLeft.Yes) edit = Rectangle.FromLTRB(arrowWidth + 2, 3, Width - 4, Height - 3);
            g.ExcludeClip(edit);
        }
        using (var back = new SolidBrush(Parent is KPanel panel ? (panel.FillColor.IsEmpty ? panel.Theme.Surface : panel.FillColor) : Parent?.BackColor ?? SystemColors.Control)) g.FillRectangle(back, ClientRectangle);
        KRenderer.Surface(g, ClientRectangle, style, radius * scale, border * scale, KShape.Rounded, scale, Color.Empty);
        bool rtl = RightToLeft == RightToLeft.Yes;
        if (DropDownStyle == ComboBoxStyle.DropDownList)
        {
            var area = new Rectangle(rtl ? arrowWidth : (int)(10*scale), 1, Math.Max(0, Width-arrowWidth-(int)(10*scale)), Height-2);
            TextRenderer.DrawText(g, Text, Font, area, style.Text, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | (rtl ? TextFormatFlags.RightToLeft | TextFormatFlags.Right : 0));
        }
        float x = rtl ? arrowWidth/2f : Width-arrowWidth/2f, y = Height/2f;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        float opening = (arrowTransition ??= new KScalarTransition(this)).Sample(DroppedDown ? 1 : 0, Animation);
        Color ink = Enabled ? (arrowColor.IsEmpty ? style.Text : arrowColor)
            : disabledArrowColor.IsEmpty ? style.Text : disabledArrowColor;
        if (arrowSize > 0 && arrowThickness > 0)
        {
            using var pen = new Pen(ink, arrowThickness * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            float d = Math.Min(arrowSize * scale, Math.Max(0, Math.Min(arrowWidth, Height) - arrowThickness * scale - 4 * scale)) / 2;
            g.TranslateTransform(x, y);
            g.RotateTransform(180 * opening);
            g.DrawLines(pen, [new PointF(-d, -d/2), new PointF(0, d/2), new PointF(d, -d/2)]);
        }
        g.Restore(saved);
    }
    protected override void OnMouseEnter(EventArgs e) { hover = true; base.OnMouseEnter(e); Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; base.OnMouseLeave(e); Invalidate(); }
    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
    protected override void OnDropDown(EventArgs e) { base.OnDropDown(e); Invalidate(); }
    protected override void OnDropDownClosed(EventArgs e) { base.OnDropDownClosed(e); Invalidate(); }
    protected override void Dispose(bool disposing) { if (disposing) { transition?.Dispose(); arrowTransition?.Dispose(); Appearance.Changed -= Changed; ItemAppearance.Changed -= Changed; SelectedItemAppearance.Changed -= Changed; } base.Dispose(disposing); if (disposing) { defaultFont?.Dispose(); defaultFont = null; } }
}
