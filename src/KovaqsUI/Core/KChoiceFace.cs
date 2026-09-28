using System.ComponentModel;
using System.Drawing.Drawing2D;
using KovaqsUI.Drawing;
using KovaqsUI.Styling;

namespace KovaqsUI.Core;

internal sealed class KChoiceFace : IDisposable
{
    private readonly ButtonBase owner;
    private readonly Func<CheckState> state;
    private readonly bool radio;
    private readonly KStateTransition colors;
    private readonly KStateTransition labelColors;
    private readonly KScalarTransition mark;
    private bool hover, pressed;
    internal KTheme Theme = KTheme.Light;
    internal KAppearance Appearance { get; } = new();
    internal KStateStyle CheckedStyle { get; } = new();
    internal KFocusStyle Focus { get; } = new();
    internal KAnimationStyle Animation { get; } = new();
    internal int GlyphSize = 20, Gap = 9, Radius = 5, Border = 1;
    private bool ButtonAppearance => (owner is CheckBox check ? check.Appearance : ((RadioButton)owner).Appearance) == System.Windows.Forms.Appearance.Button;
    internal KChoiceFace(ButtonBase owner, Func<CheckState> state, bool radio)
    {
        this.owner = owner; this.state = state; this.radio = radio;
        colors = new(owner); labelColors = new(owner); mark = new(owner);
        Appearance.Changed += Refresh; CheckedStyle.Changed += Refresh; Focus.Changed += Refresh;
        owner.MouseEnter += Enter; owner.MouseLeave += Leave; owner.MouseDown += Down; owner.MouseUp += Up;
        owner.KeyDown += KeyDown; owner.KeyUp += KeyUp; owner.LostFocus += Reset; owner.GotFocus += Refresh;
        owner.EnabledChanged += Reset; owner.MouseCaptureChanged += CaptureChanged;

    }
    private void Refresh(object? s, EventArgs e) => owner.Invalidate();
    private void Enter(object? s, EventArgs e) { hover = true; owner.Invalidate(); }
    private void Leave(object? s, EventArgs e) { hover = false; owner.Invalidate(); }
    private void Down(object? s, MouseEventArgs e) { if (e.Button == MouseButtons.Left) pressed = true; owner.Invalidate(); }
    private void Up(object? s, MouseEventArgs e) => Reset(s, e);
    private void KeyDown(object? s, KeyEventArgs e) { if (e.KeyCode == Keys.Space) pressed = true; owner.Invalidate(); }
    private void KeyUp(object? s, KeyEventArgs e) => Reset(s, e);
    private void Reset(object? s, EventArgs e) { pressed = false; owner.Invalidate(); }
    private void CaptureChanged(object? s, EventArgs e) { if (!owner.Capture) Reset(s, e); }
    internal Size Preferred(Size constraint)
    {
        int glyph = (int)(GlyphSize * owner.DeviceDpi / 96f), gap = (int)(Gap * owner.DeviceDpi / 96f);
        Size image = owner.Image?.Size ?? Size.Empty;
        bool vertical = owner.TextImageRelation is TextImageRelation.ImageAboveText or TextImageRelation.TextAboveImage;
        bool overlay = owner.TextImageRelation == TextImageRelation.Overlay;
        var alignment = owner is CheckBox check ? check.CheckAlign : ((RadioButton)owner).CheckAlign;
        bool stackedGlyph = !ButtonAppearance && alignment is ContentAlignment.TopCenter or ContentAlignment.BottomCenter;
        bool centeredGlyph = !ButtonAppearance && alignment == ContentAlignment.MiddleCenter;
        int glyphWidth = ButtonAppearance || stackedGlyph || centeredGlyph ? 0 : glyph + gap;
        int availableWidth = constraint.Width > 0
            ? Math.Max(1, constraint.Width - owner.Padding.Horizontal - glyphWidth - (vertical || overlay ? 0 : image.Width))
            : int.MaxValue;
        var flags = TextFormatFlags.WordBreak;
        if (!owner.UseMnemonic) flags |= TextFormatFlags.NoPrefix;
        var text = string.IsNullOrEmpty(owner.Text) ? Size.Empty
            : TextRenderer.MeasureText(owner.Text, owner.Font, new Size(availableWidth, int.MaxValue), flags);
        int width = vertical || overlay ? Math.Max(text.Width, image.Width) : text.Width + image.Width;
        int height = vertical ? text.Height + image.Height : Math.Max(text.Height, image.Height);
        return new((stackedGlyph || centeredGlyph ? Math.Max(glyph, width) : glyphWidth + width) + owner.Padding.Horizontal,
            (stackedGlyph ? glyph + gap + height : Math.Max(ButtonAppearance ? 0 : glyph, height)) + owner.Padding.Vertical);
    }
    internal void Paint(PaintEventArgs e)
    {
        float scale = owner.DeviceDpi / 96f;
        // The native control keeps input, grouping and accessibility; replace only its face.
        // OnPaintBackground already reproduces the parent surface for transparent controls.
        // Keep that surface, including gradients, images and custom parent painting.
        bool selected = state() != CheckState.Unchecked;
        var style = KRenderer.Resolve(Theme, Appearance, KButtonVariant.Outline, true, owner.Enabled, hover, pressed, owner.Focused);
        Color label = Appearance.Normal.ForeColor.IsEmpty
            ? owner.ForeColor == SystemColors.ControlText ? Theme.TextPrimary : owner.ForeColor
            : Appearance.Normal.ForeColor;
        if (!owner.Enabled) label = Appearance.Disabled.ForeColor.IsEmpty ? Theme.DisabledText : Appearance.Disabled.ForeColor;
        else
        {
            if (owner.Focused && !Appearance.Focused.ForeColor.IsEmpty) label = Appearance.Focused.ForeColor;
            if (hover && !Appearance.Hover.ForeColor.IsEmpty) label = Appearance.Hover.ForeColor;
            if (pressed && !Appearance.Pressed.ForeColor.IsEmpty) label = Appearance.Pressed.ForeColor;
        }
        label = labelColors.Sample(style with { Text = label }, Animation).Text;
        if (selected && owner.Enabled) style = (style with { Fill = Theme.Primary, Border = Theme.Primary, Text = Theme.OnPrimary }).Apply(CheckedStyle);
        style = colors.Sample(style, Animation);
        int size = Math.Min((int)(GlyphSize * scale), Math.Min(owner.Width, owner.Height));
        var alignment = owner is CheckBox checkbox ? checkbox.CheckAlign : ((RadioButton)owner).CheckAlign;
        bool center = alignment is ContentAlignment.TopCenter or ContentAlignment.MiddleCenter or ContentAlignment.BottomCenter;
        bool top = alignment is ContentAlignment.TopLeft or ContentAlignment.TopCenter or ContentAlignment.TopRight;
        bool bottom = alignment is ContentAlignment.BottomLeft or ContentAlignment.BottomCenter or ContentAlignment.BottomRight;
        bool right = alignment is ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight;
        if (owner.RightToLeft == RightToLeft.Yes) right = !right;
        int x = center ? owner.Padding.Left + (owner.Width - owner.Padding.Horizontal - size) / 2
            : right ? owner.Width - owner.Padding.Right - size : owner.Padding.Left;
        int y = top ? owner.Padding.Top : bottom ? owner.Height - owner.Padding.Bottom - size
            : owner.Padding.Top + (owner.Height - owner.Padding.Vertical - size) / 2;
        var bounds = new Rectangle(x, y, size, size);
        KRenderer.Surface(e.Graphics, ButtonAppearance ? owner.ClientRectangle : bounds, style, Radius * scale, Border * scale, radio && !ButtonAppearance ? KShape.Ellipse : KShape.Rounded, scale,
            owner.Focused && owner.Enabled ? Focus.Resolve(Appearance.Focused.BorderColor, Theme.Focus) : Color.Empty, Focus.Thickness, Focus.Inset);
        float amount = mark.Sample(selected ? 1 : 0, Animation);
        if (!ButtonAppearance && amount > .01f && size > 3)
        {
            var saved = e.Graphics.Save(); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color ink = style.Text;
            using var pen = new Pen(Color.FromArgb((int)(255 * amount), ink), Math.Max(1, size * .105f)) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            float cx = bounds.X + size / 2f, cy = bounds.Y + size / 2f;
            if (radio) { float d = size * .43f * amount; using var brush = new SolidBrush(pen.Color); e.Graphics.FillEllipse(brush, cx-d/2, cy-d/2, d, d); }
            else if (state() == CheckState.Indeterminate) e.Graphics.DrawLine(pen, cx-size*.23f, cy, cx+size*.23f, cy);
            else { e.Graphics.TranslateTransform(cx, cy); e.Graphics.ScaleTransform(amount, amount); e.Graphics.DrawLines(pen, [new PointF(-size*.24f,0),new PointF(-size*.06f,size*.17f),new PointF(size*.25f,-size*.20f)]); }
            e.Graphics.Restore(saved);
        }
        int gap = (int)(Gap * scale);
        var textBounds = Rectangle.FromLTRB(right ? owner.Padding.Left : bounds.Right + gap, owner.Padding.Top,
            right ? bounds.Left - gap : owner.Width - owner.Padding.Right, owner.Height - owner.Padding.Bottom);
        if (center)
        {
            textBounds = Rectangle.FromLTRB(owner.Padding.Left, owner.Padding.Top, owner.Width - owner.Padding.Right, owner.Height - owner.Padding.Bottom);
            if (top) { textBounds.Y = bounds.Bottom + gap; textBounds.Height = owner.Height - owner.Padding.Bottom - textBounds.Y; }
            else if (bottom) textBounds.Height = bounds.Top - gap - textBounds.Top;
        }
        if (ButtonAppearance)
        {
            textBounds = Rectangle.FromLTRB(owner.Padding.Left, owner.Padding.Top, owner.Width - owner.Padding.Right, owner.Height - owner.Padding.Bottom);
            if (selected) label = style.Text;
        }
        DrawImage(e.Graphics, ref textBounds);
        var textAlign = owner is CheckBox check ? check.TextAlign : ((RadioButton)owner).TextAlign;
        var flags = TextFormatFlags.WordBreak;
        if (textAlign is ContentAlignment.MiddleLeft or ContentAlignment.MiddleCenter or ContentAlignment.MiddleRight) flags |= TextFormatFlags.VerticalCenter;
        if (textAlign is ContentAlignment.BottomLeft or ContentAlignment.BottomCenter or ContentAlignment.BottomRight) flags |= TextFormatFlags.Bottom;
        if (textAlign is ContentAlignment.TopCenter or ContentAlignment.MiddleCenter or ContentAlignment.BottomCenter) flags |= TextFormatFlags.HorizontalCenter;
        if (textAlign is ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight) flags |= TextFormatFlags.Right;
        if (!owner.UseMnemonic) flags |= TextFormatFlags.NoPrefix;
        if (owner.AutoEllipsis) flags |= TextFormatFlags.EndEllipsis;
        if (owner.RightToLeft == RightToLeft.Yes) flags |= TextFormatFlags.Right | TextFormatFlags.RightToLeft;
        if (textBounds.Width > 0 && textBounds.Height > 0) TextRenderer.DrawText(e.Graphics, owner.Text, owner.Font, textBounds,
            label, flags);
    }
    private void DrawImage(Graphics graphics, ref Rectangle textBounds)
    {
        if (owner.Image is not { } image || textBounds.Width <= 0 || textBounds.Height <= 0) return;
        var area = textBounds;
        int width = Math.Min(image.Width, area.Width), height = Math.Min(image.Height, area.Height);
        var alignment = owner.ImageAlign;
        bool left = alignment is ContentAlignment.TopLeft or ContentAlignment.MiddleLeft or ContentAlignment.BottomLeft;
        bool right = alignment is ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight;
        if (owner.RightToLeft == RightToLeft.Yes) (left, right) = (right, left);
        int x = left ? area.Left : right ? area.Right - width : area.Left + (area.Width - width) / 2;
        int y = alignment is ContentAlignment.TopLeft or ContentAlignment.TopCenter or ContentAlignment.TopRight ? area.Top
            : alignment is ContentAlignment.BottomLeft or ContentAlignment.BottomCenter or ContentAlignment.BottomRight ? area.Bottom - height : area.Top + (area.Height - height) / 2;
        if (!string.IsNullOrEmpty(owner.Text))
        {
            var relation = owner.TextImageRelation;
            if (relation is TextImageRelation.ImageBeforeText or TextImageRelation.TextBeforeImage)
            {
                bool before = relation == TextImageRelation.ImageBeforeText;
                if (owner.RightToLeft == RightToLeft.Yes) before = !before;
                x = before ? area.Left : area.Right - width;
                if (before) textBounds.X += width;
                textBounds.Width -= width;
            }
            else if (relation is TextImageRelation.ImageAboveText or TextImageRelation.TextAboveImage)
            {
                bool above = relation == TextImageRelation.ImageAboveText;
                y = above ? area.Top : area.Bottom - height;
                if (above) textBounds.Y += height;
                textBounds.Height -= height;
            }
        }
        if (owner.Enabled) graphics.DrawImage(image, new Rectangle(x, y, width, height));
        else
        {
            using var attributes = new System.Drawing.Imaging.ImageAttributes();
            attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix { Matrix33 = .4f });
            graphics.DrawImage(image, new Rectangle(x, y, width, height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
        }
    }
    public void Dispose()
    {
        colors.Dispose(); labelColors.Dispose(); mark.Dispose();
        Appearance.Changed -= Refresh; CheckedStyle.Changed -= Refresh; Focus.Changed -= Refresh;
        owner.MouseEnter -= Enter; owner.MouseLeave -= Leave; owner.MouseDown -= Down; owner.MouseUp -= Up;
        owner.KeyDown -= KeyDown; owner.KeyUp -= KeyUp; owner.LostFocus -= Reset; owner.GotFocus -= Refresh;
        owner.EnabledChanged -= Reset; owner.MouseCaptureChanged -= CaptureChanged;
    }
}




