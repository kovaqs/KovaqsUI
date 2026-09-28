using System.ComponentModel;
using System.Drawing.Drawing2D;
using KovaqsUI.Drawing;
using KovaqsUI.Styling;

namespace KovaqsUI.Controls;

[ToolboxItem(true)]
public class KPanel : Panel, IKThemeControl
{
    private KTheme theme = KTheme.Light;
    private int radius = 12, border = 1;
    private Bitmap? shadowCache;
    private Font? defaultFont;
    [TypeConverter(typeof(KFontConverter))]
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override Font Font { get => base.Font; set => base.Font = KFontManager.ResolveFont(value, defaultFont); }
    public override void ResetFont() => Font = null;
    private bool ShouldSerializeFont() => !Equals(Font, defaultFont);
    public KPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent; Padding = new Padding(12); Size = new Size(300, 160);
        Surface.Changed += Changed; Shadow.Changed += ShadowChanged;
        base.Font = defaultFont = KTypography.Body;
    }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public KTheme Theme { get => theme; set { theme = value ?? throw new ArgumentNullException(nameof(value)); Invalidate(); } }
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] public KStateStyle Surface { get; } = new();
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] public KShadowStyle Shadow { get; } = new();
    [Category("Kova Appearance"), DefaultValue(12)] public int CornerRadius { get => radius; set { ArgumentOutOfRangeException.ThrowIfNegative(value); radius = value; ClearShadow(); } }
    [Category("Kova Appearance"), DefaultValue(1)] public int BorderThickness { get => border; set { ArgumentOutOfRangeException.ThrowIfNegative(value); border = value; Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(typeof(Color), "")] public Color FillColor { get => Surface.FillColor; set => Surface.FillColor = value; }
    [Category("Kova Appearance"), DefaultValue(typeof(Color), "")] public Color BorderColor { get => Surface.BorderColor; set => Surface.BorderColor = value; }
    [Category("Kova Appearance"), DefaultValue(typeof(Color), "")] public Color GradientColor { get => Surface.GradientColor; set => Surface.GradientColor = value; }
    private int ShadowMargin => Shadow?.Enabled == true ? (int)Math.Ceiling((Shadow.Blur + Math.Max(Math.Abs(Shadow.Offset.X), Math.Abs(Shadow.Offset.Y))) * DeviceDpi / 96f) : 0;
    public override Rectangle DisplayRectangle { get { var r = base.DisplayRectangle; r.Inflate(-ShadowMargin, -ShadowMargin); r.Width = Math.Max(0, r.Width); r.Height = Math.Max(0, r.Height); return r; } }
    private void Changed(object? s, EventArgs e) => Invalidate();
    private void ShadowChanged(object? s, EventArgs e) { ClearShadow(); PerformLayout(); }
    private void ClearShadow() { shadowCache?.Dispose(); shadowCache = null; Invalidate(); }
    protected override void OnSizeChanged(EventArgs e) { ClearShadow(); base.OnSizeChanged(e); }
    protected override void OnDpiChangedAfterParent(EventArgs e) { ClearShadow(); base.OnDpiChangedAfterParent(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        float scale = DeviceDpi / 96f;
        var r = Rectangle.Inflate(ClientRectangle, -ShadowMargin, -ShadowMargin);
        if (Shadow.Enabled && Shadow.Opacity > 0 && Width > 0 && Height > 0 && r.Width > 1 && r.Height > 1)
        {
            if (shadowCache is null)
            {
                shadowCache = new Bitmap(Width, Height);
                using var g = Graphics.FromImage(shadowCache); g.SmoothingMode = SmoothingMode.AntiAlias;
                int blur = Math.Max(1, (int)(Shadow.Blur * scale));
                for (int i = blur; i >= 0; i--)
                {
                    var outline = new RectangleF(r.X + Shadow.Offset.X * scale - i / 2f, r.Y + Shadow.Offset.Y * scale - i / 2f, r.Width + i, r.Height + i);
                    using var path = KRenderer.Path(outline, radius * scale + i / 2f, KShape.Rounded);
                    using var brush = new SolidBrush(Color.FromArgb(Math.Max(1, Shadow.Opacity / (blur + 1)), Shadow.Color));
                    g.FillPath(brush, path);
                }
            }
            e.Graphics.DrawImageUnscaled(shadowCache, 0, 0);
        }
        var style = new PaintStyle(theme.Surface, Color.Empty, theme.TextPrimary, theme.Border, 90).Apply(Surface);
        KRenderer.Surface(e.Graphics, r, style, radius * scale, border * scale, KShape.Rounded, scale, Color.Empty);
        base.OnPaint(e);
    }
    protected override void Dispose(bool disposing) { if (disposing) { shadowCache?.Dispose(); Surface.Changed -= Changed; Shadow.Changed -= ShadowChanged; } base.Dispose(disposing); if (disposing) { defaultFont?.Dispose(); defaultFont = null; } }
}
