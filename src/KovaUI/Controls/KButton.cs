using KovaUI.Core;
using System.ComponentModel;
using KovaUI.Drawing;
using KovaUI.Styling;

namespace KovaUI.Controls;

/// <summary>Owner-painted native Button: retains IButtonControl, dialog, mnemonic and accessibility behavior.</summary>
[ToolboxItem(true), DefaultEvent(nameof(Click)), DefaultProperty(nameof(Text))]
public class KButton : Button, IKThemeControl
{
    private KStateTransition? transition;
    private KScalarTransition? pressMotion, clickMotion;
    private bool hover, pressed, defaultButton;
    private KTheme theme = KTheme.Light;
    private KButtonVariant variant;
    private KShape shape;
    private int radius = KMetrics.RadiusMedium, border = 1, gap = KMetrics.SpaceS;
    private Size imageSize = new(18, 18);
    private Color normalForeColor;
    private Font? defaultFont;
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override Font Font { get => base.Font; set => base.Font = value ?? defaultFont; }
    public override void ResetFont() => Font = null;
    private bool ShouldSerializeFont() => !Equals(Font, defaultFont);
    public KButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        base.FlatStyle = FlatStyle.Flat;
        base.BackColor = Color.Transparent;
        Size = new Size(112, KMetrics.HeightMedium);
        Padding = new Padding(KMetrics.SpaceM, KMetrics.SpaceXS, KMetrics.SpaceM, KMetrics.SpaceXS);
        Appearance.Changed += AppearanceChanged;
        FocusAppearance.Changed += AppearanceChanged;
        // Register first so consumer Paint handlers receive the finished Kova face.
        // Keep Button's paint lifecycle, including its inherited Control.Paint event.
        base.Paint += PaintFace;
        base.Font = defaultFont = KTypography.Button;
    }
    protected override Cursor DefaultCursor => Cursors.Hand;
    private float pressDepth;
    [Category("Kova Appearance"), DefaultValue(0f)]
    public float PressDepth { get => pressDepth; set { if (!float.IsFinite(value) || value < 0 || value > 8) throw new ArgumentOutOfRangeException(nameof(value)); pressDepth = value; Invalidate(); } }
    private Color pressColor;
    private int pressOpacity = 16;
    [Category("Kova Appearance"), DefaultValue(typeof(Color), ""), Description("Color de respuesta al presionar. Vacío hereda el color del texto.")]
    public Color PressColor { get => pressColor; set { pressColor = value; Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(16)]
    public int PressOpacity { get => pressOpacity; set { if (value < 0 || value > 255) throw new ArgumentOutOfRangeException(nameof(value)); pressOpacity = value; Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(true)]
    public bool ClickAnimation { get; set; } = true;
    private Color clickColor;
    private int clickOpacity = 28;
    [Category("Kova Appearance"), DefaultValue(typeof(Color), ""), Description("Click wave color. Empty inherits the current text color.")]
    public Color ClickColor { get => clickColor; set { clickColor = value; Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(28), Description("Maximum click wave opacity, from 0 to 255.")]
    public int ClickOpacity { get => clickOpacity; set { if (value < 0 || value > 255) throw new ArgumentOutOfRangeException(nameof(value)); clickOpacity = value; Invalidate(); } }
    protected override void OnClick(EventArgs e)
    {
        if (Enabled && ClickAnimation) (clickMotion ??= new KScalarTransition(this)).Pulse(Animation);
        base.OnClick(e);
    }
    private void AppearanceChanged(object? sender, EventArgs e)
    {
        if (normalForeColor != Appearance.Normal.ForeColor)
        {
            normalForeColor = Appearance.Normal.ForeColor;
            base.ForeColor = normalForeColor;
        }
        Invalidate();
    }
    public override Color ForeColor
    {
        get => base.ForeColor;
        set { if (Appearance is null) base.ForeColor = value; else Appearance.Normal.ForeColor = value; }
    }
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public KAppearance Appearance { get; } = new();
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public KAnimationStyle Animation { get; } = new();
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public KFocusStyle FocusAppearance { get; } = new();
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public KTheme Theme { get => theme; set { theme = value ?? throw new ArgumentNullException(nameof(value)); Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(KButtonVariant.Primary)]
    public KButtonVariant Variant { get => variant; set { variant = value; Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(KShape.Rounded)]
    public KShape Shape { get => shape; set { shape = value; Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(12), Description("Corner radius in logical pixels (96 DPI).")]
    public int CornerRadius { get => radius; set { ArgumentOutOfRangeException.ThrowIfNegative(value); radius = value; Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(1)]
    public int BorderThickness { get => border; set { ArgumentOutOfRangeException.ThrowIfNegative(value); border = value; Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(typeof(Color), "")]
    public Color FillColor { get => Appearance.Normal.FillColor; set => Appearance.Normal.FillColor = value; }
    [Category("Kova Appearance"), DefaultValue(typeof(Color), "")]
    public Color BorderColor { get => Appearance.Normal.BorderColor; set => Appearance.Normal.BorderColor = value; }
    [Category("Kova Appearance"), DefaultValue(typeof(Color), "")]
    public Color GradientColor { get => Appearance.Normal.GradientColor; set => Appearance.Normal.GradientColor = value; }
    [Category("Kova Appearance"), DefaultValue(8)]
    public int ImageSpacing { get => gap; set { ArgumentOutOfRangeException.ThrowIfNegative(value); if (gap == value) return; gap = value; ContentLayoutChanged(nameof(ImageSpacing)); } }
    [Category("Kova Appearance"), DefaultValue(typeof(Size), "18, 18")]
    public Size ImageSize { get => imageSize; set { if (value.Width < 1 || value.Height < 1) throw new ArgumentOutOfRangeException(nameof(value)); if (imageSize == value) return; imageSize = value; ContentLayoutChanged(nameof(ImageSize)); } }
    private void ContentLayoutChanged(string property)
    {
        Parent?.PerformLayout(this, property);
        Invalidate();
    }
    public override void NotifyDefault(bool value) { defaultButton = value; base.NotifyDefault(value); Invalidate(); }
    protected override void OnStyleChanged(EventArgs e)
    {
        // FlatStyle.System switches ButtonBase back to native painting. Keep
        // the Kova paint pipeline even when the property is set via Button.
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        base.OnStyleChanged(e);
    }
    protected override void OnHandleCreated(EventArgs e)
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        base.OnHandleCreated(e);
    }
    protected override CreateParams CreateParams
    {
        get
        {
            // A native System button cannot host Kova's owner-painted face.
            // Normalize that rendering-only request before creating its HWND.
            if (base.FlatStyle == FlatStyle.System) base.FlatStyle = FlatStyle.Flat;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            return base.CreateParams;
        }
    }
    protected override void OnMouseEnter(EventArgs e) { hover = true; base.OnMouseEnter(e); Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; base.OnMouseLeave(e); Invalidate(); }
    protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) pressed = true; base.OnMouseDown(e); Invalidate(); }
    protected override void OnMouseUp(MouseEventArgs e) { pressed = false; base.OnMouseUp(e); Invalidate(); }
    protected override void OnMouseCaptureChanged(EventArgs e) { if (!Capture) pressed = false; base.OnMouseCaptureChanged(e); Invalidate(); }
    protected override void OnKeyDown(KeyEventArgs e) { if (e.KeyCode == Keys.Space && e.Modifiers == Keys.None) pressed = true; base.OnKeyDown(e); Invalidate(); }
    protected override void OnKeyUp(KeyEventArgs e) { pressed = false; base.OnKeyUp(e); Invalidate(); }
    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { pressed = false; base.OnLostFocus(e); Invalidate(); }
    protected override void OnEnabledChanged(EventArgs e) { pressed = false; base.OnEnabledChanged(e); Invalidate(); }
    private void PaintFace(object? sender, PaintEventArgs e)
    {
        // Both native and custom passes use the same double buffer. Restore the
        // background (including transparent corners) before applying our face.
        base.OnPaintBackground(e);
        float scale = DeviceDpi / 96f;
        var style = KRenderer.Resolve(theme, Appearance, variant, false, Enabled, hover, pressed && (hover || !Capture), Focused);
        style = (transition ??= new KStateTransition(this)).Sample(style, Animation);
        KRenderer.Surface(e.Graphics, ClientRectangle, style, radius * scale, border * scale, shape, scale,
            Enabled && (Focused || defaultButton) ? FocusAppearance.Resolve(Appearance.Focused.BorderColor, theme.Focus) : Color.Empty,
            FocusAppearance.Thickness, FocusAppearance.Inset);
        float push = (pressMotion ??= new KScalarTransition(this)).Sample(pressed && (hover || !Capture) ? 1 : 0, Animation);
        if (push > 0 && pressOpacity > 0 && Width > 3 && Height > 3)
        {
            using var pressurePath = KRenderer.Path(new RectangleF(1, 1, Width - 3, Height - 3), radius * scale, shape);
            var ink = pressColor.IsEmpty ? style.Text : pressColor;
            using var pressureBrush = new SolidBrush(Color.FromArgb((int)(pressOpacity * push * ink.A / 255f), ink));
            var pressureState = e.Graphics.Save();
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillPath(pressureBrush, pressurePath);
            e.Graphics.Restore(pressureState);
        }
        float pulse = clickMotion?.Value ?? 0;
        if (ClickAnimation && Animation.Enabled && KAnimationSettings.Enabled && pulse > 0 && clickOpacity > 0)
        {
            using var clip = KRenderer.Path(new RectangleF(1, 1, Math.Max(0, Width - 3), Math.Max(0, Height - 3)), radius * scale, shape);
            var saved = e.Graphics.Save();
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.SetClip(clip, System.Drawing.Drawing2D.CombineMode.Intersect);
            float diameter = Math.Max(Width, Height) * (1.2f - pulse) * 2;
            var waveColor = clickColor.IsEmpty ? style.Text : clickColor;
            float envelope = MathF.Sin(MathF.PI * pulse);
            using var brush = new SolidBrush(Color.FromArgb((int)(clickOpacity * envelope * waveColor.A / 255f), waveColor));
            e.Graphics.FillEllipse(brush, (Width - diameter) / 2, (Height - diameter) / 2, diameter, diameter);
            e.Graphics.Restore(saved);
        }
        var area = Rectangle.FromLTRB(Padding.Left, Padding.Top, Width - Padding.Right, Height - Padding.Bottom);
        area.Offset(0, (int)MathF.Round(Math.Clamp(PressDepth, 0, 8) * scale * push));
        if (area.Width <= 0 || area.Height <= 0) return;
        var textArea = area;
        var img = Image;
        if (img is not null)
        {
            var size = new Size(Math.Min(area.Width, (int)(imageSize.Width * scale)), Math.Min(area.Height, (int)(imageSize.Height * scale)));
            int spacing = string.IsNullOrEmpty(Text) ? 0 : (int)(gap * scale);
            bool before = TextImageRelation != TextImageRelation.TextBeforeImage;
            if (RightToLeft == RightToLeft.Yes) before = !before;
            Rectangle imageArea;
            var alignedImage = Align(area, size, EffectiveAlignment(ImageAlign));
            if (string.IsNullOrEmpty(Text)) imageArea = alignedImage;
            else if (TextImageRelation is TextImageRelation.ImageAboveText or TextImageRelation.TextAboveImage)
            {
                bool top = TextImageRelation == TextImageRelation.ImageAboveText;
                imageArea = new Rectangle(alignedImage.X, top ? area.Top : area.Bottom - size.Height, size.Width, size.Height);
                if (top) { textArea.Y += size.Height + spacing; textArea.Height -= size.Height + spacing; } else textArea.Height -= size.Height + spacing;
            }
            else if (TextImageRelation == TextImageRelation.Overlay) imageArea = alignedImage;
            else
            {
                imageArea = new Rectangle(before ? area.Left : area.Right - size.Width, alignedImage.Y, size.Width, size.Height);
                if (before) textArea.X += size.Width + spacing;
                textArea.Width -= size.Width + spacing;
            }
            if (Enabled) e.Graphics.DrawImage(img, imageArea);
            else
            {
                using var attributes = new System.Drawing.Imaging.ImageAttributes();
                attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix { Matrix33 = .4f });
                e.Graphics.DrawImage(img, imageArea, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attributes);
            }
        }
        var flags = TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.WordBreak;
        if (!UseMnemonic) flags |= TextFormatFlags.NoPrefix; else if (!ShowKeyboardCues) flags |= TextFormatFlags.HidePrefix;
        if (AutoEllipsis) flags |= TextFormatFlags.EndEllipsis;
        var align = EffectiveAlignment(TextAlign);
        if (align is ContentAlignment.TopCenter or ContentAlignment.MiddleCenter or ContentAlignment.BottomCenter) flags |= TextFormatFlags.HorizontalCenter;
        else if (align is ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight) flags |= TextFormatFlags.Right;
        if (align is ContentAlignment.MiddleLeft or ContentAlignment.MiddleCenter or ContentAlignment.MiddleRight) flags |= TextFormatFlags.VerticalCenter;
        else if (align is ContentAlignment.BottomLeft or ContentAlignment.BottomCenter or ContentAlignment.BottomRight) flags |= TextFormatFlags.Bottom;
        if (RightToLeft == RightToLeft.Yes) flags |= TextFormatFlags.RightToLeft;
        if (textArea.Width > 0 && textArea.Height > 0) TextRenderer.DrawText(e.Graphics, Text, Font, textArea, style.Text, flags);
        OnContentPaint(e);
    }
    protected virtual void OnContentPaint(PaintEventArgs e) { }
    private ContentAlignment EffectiveAlignment(ContentAlignment alignment) => RightToLeft != RightToLeft.Yes ? alignment : alignment switch
    {
        ContentAlignment.TopLeft => ContentAlignment.TopRight,
        ContentAlignment.TopRight => ContentAlignment.TopLeft,
        ContentAlignment.MiddleLeft => ContentAlignment.MiddleRight,
        ContentAlignment.MiddleRight => ContentAlignment.MiddleLeft,
        ContentAlignment.BottomLeft => ContentAlignment.BottomRight,
        ContentAlignment.BottomRight => ContentAlignment.BottomLeft,
        _ => alignment
    };
    private static Rectangle Align(Rectangle r, Size s, ContentAlignment a) => new(
        a is ContentAlignment.TopLeft or ContentAlignment.MiddleLeft or ContentAlignment.BottomLeft ? r.Left : a is ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight ? r.Right - s.Width : r.Left + (r.Width - s.Width) / 2,
        a is ContentAlignment.TopLeft or ContentAlignment.TopCenter or ContentAlignment.TopRight ? r.Top : a is ContentAlignment.BottomLeft or ContentAlignment.BottomCenter or ContentAlignment.BottomRight ? r.Bottom - s.Height : r.Top + (r.Height - s.Height) / 2, s.Width, s.Height);
    public override Size GetPreferredSize(Size proposedSize)
    {
        float scale = DeviceDpi / 96f;
        int iw = Image is null ? 0 : (int)(imageSize.Width * scale);
        int ih = Image is null ? 0 : (int)(imageSize.Height * scale);
        int spacing = Image is null || string.IsNullOrEmpty(Text) ? 0 : (int)(gap * scale);
        bool vertical = TextImageRelation is TextImageRelation.ImageAboveText or TextImageRelation.TextAboveImage;
        bool horizontal = TextImageRelation is TextImageRelation.ImageBeforeText or TextImageRelation.TextBeforeImage;
        int textWidth = proposedSize.Width <= 0 ? int.MaxValue : Math.Max(1, proposedSize.Width - Padding.Horizontal - (horizontal ? iw + spacing : 0));
        var flags = TextFormatFlags.WordBreak;
        if (!UseMnemonic) flags |= TextFormatFlags.NoPrefix;
        var measured = string.IsNullOrEmpty(Text) ? Size.Empty : TextRenderer.MeasureText(Text, Font, new Size(textWidth, int.MaxValue), flags);
        int width = vertical || TextImageRelation == TextImageRelation.Overlay ? Math.Max(measured.Width, iw) : measured.Width + iw + spacing;
        int height = vertical ? measured.Height + ih + spacing : Math.Max(measured.Height, ih);
        return new Size(width + Padding.Horizontal, Math.Max(height + Padding.Vertical, (int)(KMetrics.HeightMedium * scale)));
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { transition?.Dispose(); pressMotion?.Dispose(); clickMotion?.Dispose(); Appearance.Changed -= AppearanceChanged; FocusAppearance.Changed -= AppearanceChanged; base.Paint -= PaintFace; }
        base.Dispose(disposing); if (disposing) { defaultFont?.Dispose(); defaultFont = null; }
    }
}
