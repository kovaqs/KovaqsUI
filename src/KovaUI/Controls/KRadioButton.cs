using System.ComponentModel;
using KovaUI.Core;
using KovaUI.Styling;

namespace KovaUI.Controls;

[ToolboxItem(true), DefaultEvent(nameof(CheckedChanged)), DefaultProperty(nameof(Text))]
public class KRadioButton : RadioButton, IKThemeControl
{
    private readonly KChoiceFace face;
    private Font? defaultFont;
    [TypeConverter(typeof(KFontConverter))]
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override Font Font { get => base.Font; set => base.Font = KFontManager.ResolveFont(value, defaultFont); }
    public override void ResetFont() => Font = null;
    private bool ShouldSerializeFont() => !Equals(Font, defaultFont);
    public KRadioButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent; Size = new Size(180, 32); Padding = new Padding(2);
        face = new(this, () => Checked ? CheckState.Checked : CheckState.Unchecked, true);
        base.Paint += PaintFace;
        base.Font = defaultFont = KTypography.Label;
    }
    private void PaintFace(object? sender, PaintEventArgs e) { base.OnPaintBackground(e); face?.Paint(e); }
    protected override void OnStyleChanged(EventArgs e)
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        base.OnStyleChanged(e);
    }
    protected override CreateParams CreateParams
    {
        get
        {
            if (base.FlatStyle == FlatStyle.System) base.FlatStyle = FlatStyle.Flat;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            return base.CreateParams;
        }
    }
    protected override Cursor DefaultCursor => Cursors.Hand;
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public KAppearance AppearanceStates => face.Appearance;
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public KStateStyle CheckedState => face.CheckedStyle;
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public KAnimationStyle Animation => face.Animation;
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public KFocusStyle FocusAppearance => face.Focus;
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public KTheme Theme { get => face.Theme; set { face.Theme = value ?? throw new ArgumentNullException(nameof(value)); Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(20)]
    public int GlyphSize { get => face.GlyphSize; set { ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value); face.GlyphSize = value; Parent?.PerformLayout(this, nameof(GlyphSize)); Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(9)]
    public int TextSpacing { get => face.Gap; set { ArgumentOutOfRangeException.ThrowIfNegative(value); face.Gap = value; Parent?.PerformLayout(this, nameof(TextSpacing)); Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(5)]
    public int CornerRadius { get => face.Radius; set { ArgumentOutOfRangeException.ThrowIfNegative(value); face.Radius = value; Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(1)]
    public int BorderThickness { get => face.Border; set { ArgumentOutOfRangeException.ThrowIfNegative(value); face.Border = value; Invalidate(); } }
    public override Size GetPreferredSize(Size proposedSize) => face is null ? base.GetPreferredSize(proposedSize) : face.Preferred(proposedSize);
    protected override void OnCheckedChanged(EventArgs e) { base.OnCheckedChanged(e); Invalidate(); }
    protected override void Dispose(bool disposing) { if (disposing) { base.Paint -= PaintFace; face?.Dispose(); } base.Dispose(disposing); if (disposing) { defaultFont?.Dispose(); defaultFont = null; } }
}
