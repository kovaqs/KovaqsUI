using System.ComponentModel;
using System.Diagnostics;
using KovaUI.Core;
using KovaUI.Drawing;
using KovaUI.Styling;

namespace KovaUI.Controls;

[ToolboxItem(true), DefaultProperty(nameof(Value))]
public class KProgressBar : ProgressBar, IKThemeControl
{
    private KTheme theme = KTheme.Light;
    private readonly KScalarTransition transition;
    private int radius = 8, border;
    private long marqueeStart = Stopwatch.GetTimestamp();
    private Font? defaultFont;
    [TypeConverter(typeof(KFontConverter))]
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override Font Font { get => base.Font; set => base.Font = KFontManager.ResolveFont(value, defaultFont); }
    public override void ResetFont() => Font = null;
    private bool ShouldSerializeFont() => !Equals(Font, defaultFont);
    public KProgressBar()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(260, 12);
        transition = new(this);
        Track.Changed += Changed; Fill.Changed += Changed;
        base.Font = defaultFont = KTypography.Body;
    }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public KTheme Theme { get => theme; set { theme = value ?? throw new ArgumentNullException(nameof(value)); Invalidate(); } }
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] public KStateStyle Track { get; } = new();
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] public KStateStyle Fill { get; } = new();
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] public KAnimationStyle Animation { get; } = new();
    [Category("Kova Appearance"), DefaultValue(8)] public int CornerRadius { get => radius; set { ArgumentOutOfRangeException.ThrowIfNegative(value); radius = value; Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(0)] public int BorderThickness { get => border; set { ArgumentOutOfRangeException.ThrowIfNegative(value); border = value; Invalidate(); } }
    private void Changed(object? sender, EventArgs e) => Invalidate();
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x40A) m.WParam = IntPtr.Zero; // The shared clock replaces the native marquee timer.
        base.WndProc(ref m);
        // Preserve native range/value/step semantics, also when accessed through ProgressBar.
        if (m.Msg >= 0x400 && m.Msg <= 0x420) Invalidate();
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        using (var background = new SolidBrush(Parent is KPanel panel ? (panel.FillColor.IsEmpty ? panel.Theme.Surface : panel.FillColor) : Parent?.BackColor ?? SystemColors.Control)) e.Graphics.FillRectangle(background, ClientRectangle);
        float scale = DeviceDpi / 96f;
        var track = new PaintStyle(theme.SurfacePressed, Color.Empty, theme.TextPrimary, theme.Border, 90).Apply(Track);
        var fill = new PaintStyle(Enabled ? theme.Primary : theme.DisabledText, Color.Empty, theme.OnPrimary, Color.Transparent, 0).Apply(Fill);
        KRenderer.Surface(e.Graphics, ClientRectangle, track, radius * scale, border * scale, KShape.Rounded, scale, Color.Empty);
        float fraction = transition.Sample(Maximum == Minimum ? 0 : (float)(((double)Value - Minimum) / ((double)Maximum - Minimum)), Animation);
        var area = Rectangle.Inflate(ClientRectangle, -Math.Max(1, (int)(border * scale)), -Math.Max(1, (int)(border * scale)));
        if (area.Width > 0 && area.Height > 0)
        {
            using var path = KRenderer.Path(area, Math.Max(0, radius * scale - 1), KShape.Rounded);
            var saved = e.Graphics.Save(); e.Graphics.SetClip(path, System.Drawing.Drawing2D.CombineMode.Intersect);
            Rectangle bar;
            if (Style == ProgressBarStyle.Marquee)
            {
                bool animate = Enabled && Visible && Animation.Enabled && KAnimationSettings.Enabled && MarqueeAnimationSpeed > 0 && !DesignMode;
                if (animate) KAnimationClock.Add(Tick); else KAnimationClock.Remove(Tick);
                float phase = animate ? (float)(Stopwatch.GetElapsedTime(marqueeStart).TotalMilliseconds % Math.Max(300, MarqueeAnimationSpeed * 20L)) / Math.Max(300, MarqueeAnimationSpeed * 20L) : .5f;
                int width = Math.Max(1, area.Width / 3);
                bar = new Rectangle(area.Left - width + (int)((area.Width + width) * phase), area.Top, width, area.Height);
            }
            else { KAnimationClock.Remove(Tick); bar = new Rectangle(area.Left, area.Top, (int)Math.Round(area.Width * fraction), area.Height); }
            if (RightToLeft == RightToLeft.Yes) bar.X = area.Right - (bar.Right - area.Left);
            KRenderer.Surface(e.Graphics, bar, fill, radius * scale, 0, KShape.Rounded, scale, Color.Empty);
            e.Graphics.Restore(saved);
        }
        base.OnPaint(e);
    }
    private void Tick()
    {
        if (IsDisposed || !Visible || !IsHandleCreated || Style != ProgressBarStyle.Marquee || !Enabled || !Animation.Enabled || !KAnimationSettings.Enabled || MarqueeAnimationSpeed == 0)
        {
            KAnimationClock.Remove(Tick);
            if (!IsDisposed && IsHandleCreated && Visible) Invalidate();
        }
        else Invalidate();
    }
    protected override void OnVisibleChanged(EventArgs e) { base.OnVisibleChanged(e); if (!Visible) KAnimationClock.Remove(Tick); else Invalidate(); }
    protected override void Dispose(bool disposing) { if (disposing) { KAnimationClock.Remove(Tick); transition.Dispose(); Track.Changed -= Changed; Fill.Changed -= Changed; } base.Dispose(disposing); if (disposing) { defaultFont?.Dispose(); defaultFont = null; } }
}
