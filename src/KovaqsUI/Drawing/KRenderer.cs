using System.Drawing.Drawing2D;
using KovaqsUI.Styling;

namespace KovaqsUI.Drawing;

internal readonly record struct PaintStyle(Color Fill, Color End, Color Text, Color Border, float Angle, bool GradientEnabled = true)
{
    public PaintStyle Apply(KStateStyle s) => new(
        s.FillColor.IsEmpty ? Fill : s.FillColor, s.GradientColor.IsEmpty ? End : s.GradientColor,
        s.ForeColor.IsEmpty ? Text : s.ForeColor, s.BorderColor.IsEmpty ? Border : s.BorderColor,
        s.GradientAngle ?? Angle,
        s.GradientMode == KGradientMode.Inherit ? GradientEnabled : s.GradientMode == KGradientMode.Linear);
}

internal static class KRenderer
{
    internal static GraphicsPath Path(RectangleF r, float radius, KShape shape)
    {
        var p = new GraphicsPath();
        if (r.Width <= 0 || r.Height <= 0) return p;
        if (shape == KShape.Ellipse) { p.AddEllipse(r); return p; }
        float d = shape == KShape.Rectangle ? 0 : Math.Min(Math.Min(r.Width, r.Height), radius * 2);
        if (shape == KShape.Capsule) d = Math.Min(r.Width, r.Height);
        if (d < 1) { p.AddRectangle(r); return p; }
        p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure(); return p;
    }
    internal static void Surface(Graphics g, Rectangle bounds, PaintStyle style, float radius, float thickness, KShape shape, float scale, Color focus, float focusThickness = 1.5f, float focusInset = 3)
    {
        if (bounds.Width < 2 || bounds.Height < 2) return;
        var saved = g.Save();
        try
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            thickness = Math.Min(thickness, Math.Min(bounds.Width, bounds.Height) / 2f);
            float inset = Math.Max(.5f, thickness / 2);
            var r = new RectangleF(bounds.X + inset, bounds.Y + inset, bounds.Width - inset * 2 - 1, bounds.Height - inset * 2 - 1);
            // At two physical pixels the inset consumes the drawable interior.
            // Never create a GDI+ gradient brush over a degenerate rectangle.
            if (r.Width <= 0 || r.Height <= 0) return;
            using var path = Path(r, radius, shape);
            using Brush brush = !style.GradientEnabled || style.End.IsEmpty || style.End == style.Fill
                ? new SolidBrush(style.Fill) : new LinearGradientBrush(r, style.Fill, style.End, style.Angle);
            g.FillPath(brush, path);
            if (thickness > 0) { using var pen = new Pen(style.Border, thickness); g.DrawPath(pen, path); }
            if (!focus.IsEmpty)
            {
                r.Inflate(-focusInset * scale, -focusInset * scale);
                if (r.Width > 0 && r.Height > 0) { using var fp = Path(r, Math.Max(0, radius - focusInset * scale), shape); using var pen = new Pen(focus, focusThickness * scale); g.DrawPath(pen, fp); }
            }
        }
        finally { g.Restore(saved); }
    }
    internal static PaintStyle Resolve(KTheme t, KAppearance a, KButtonVariant variant, bool field, bool enabled, bool hover, bool pressed, bool focused)
    {
        bool primary = !field && variant == KButtonVariant.Primary;
        bool secondary = !field && variant == KButtonVariant.Secondary;
        var s = new PaintStyle(primary ? t.Primary : secondary ? t.Secondary : !field && variant == KButtonVariant.Ghost ? Color.Transparent : t.Surface, Color.Empty,
            primary ? t.OnPrimary : secondary ? t.OnSecondary : !field && variant == KButtonVariant.Danger ? t.Danger : t.TextPrimary,
            primary ? t.Primary : secondary ? t.Secondary : variant == KButtonVariant.Ghost && !field ? Color.Transparent : t.Border, 90);
        s = s.Apply(a.Normal);
        if (!enabled) return (s with { Fill = t.DisabledFill, End = Color.Empty, Text = t.DisabledText, Border = t.DisabledBorder }).Apply(a.Disabled);
        if (hover || pressed)
        {
            // Explicit normal colors persist; theme interaction colors are only defaults.
            if (a.Normal.FillColor.IsEmpty) s = s with { Fill = primary ? (pressed ? t.PrimaryPressed : t.PrimaryHover) : secondary ? (pressed ? t.SecondaryPressed : t.SecondaryHover) : pressed ? t.SurfacePressed : t.SurfaceHover };
        }
        if (focused) s = s.Apply(a.Focused);
        if (hover) s = s.Apply(a.Hover);
        if (pressed) s = s.Apply(a.Pressed);
        return s;
    }
}



