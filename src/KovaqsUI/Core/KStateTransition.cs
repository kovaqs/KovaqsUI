using System.ComponentModel;
using System.Diagnostics;
using KovaqsUI.Drawing;
using KovaqsUI.Styling;

namespace KovaqsUI.Core;

// One timer per UI thread, running only while visible controls are transitioning.
internal sealed class KStateTransition(Control control) : IDisposable
{
    private readonly Control owner = control;
    private PaintStyle start, target;
    private bool initialized, running;
    private long started;
    private int duration;
    private KEasing easing;
    private KAnimationStyle? activeOptions;

    public PaintStyle Sample(PaintStyle next, KAnimationStyle options)
    {
        activeOptions = options;
        if (!initialized || !options.Enabled || !KAnimationSettings.Enabled || options.Duration == 0 ||
            !owner.Visible || !owner.IsHandleCreated || owner.Site?.DesignMode == true || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            initialized = true; target = next; Stop(); return next;
        }
        if (target != next)
        {
            start = Value(); target = next; started = Stopwatch.GetTimestamp();
            duration = options.Duration; easing = options.Easing; running = true;
            KAnimationClock.Add(Tick);
        }
        return Value();
    }
    private PaintStyle Value()
    {
        if (!running) return target;
        float t = (float)(Stopwatch.GetElapsedTime(started).TotalMilliseconds / duration);
        if (t >= 1) { Stop(); return target; }
        t = easing switch { KEasing.EaseOutCubic => 1 - MathF.Pow(1 - t, 3), KEasing.SmoothStep => t * t * (3 - 2 * t), _ => t };
        Color aEnd = start.GradientEnabled && !start.End.IsEmpty ? start.End : start.Fill;
        Color bEnd = target.GradientEnabled && !target.End.IsEmpty ? target.End : target.Fill;
        float angle = ((target.Angle - start.Angle + 540) % 360) - 180;
        return new(Mix(start.Fill, target.Fill, t), Mix(aEnd, bEnd, t), Mix(start.Text, target.Text, t),
            Mix(start.Border, target.Border, t), start.Angle + angle * t, true);
    }
    private static Color Mix(Color a, Color b, float t) => Color.FromArgb(
        (int)MathF.Round(a.A + (b.A - a.A) * t), (int)MathF.Round(a.R + (b.R - a.R) * t),
        (int)MathF.Round(a.G + (b.G - a.G) * t), (int)MathF.Round(a.B + (b.B - a.B) * t));
    private void Tick()
    {
        if (owner.IsDisposed || !owner.Visible || !owner.IsHandleCreated) { Stop(); return; }
        if (!KAnimationSettings.Enabled || activeOptions is { Enabled: false } or { Duration: 0 })
        { Stop(); owner.Invalidate(); return; }
        Value(); owner.Invalidate();
    }
    private void Stop() { running = false; KAnimationClock.Remove(Tick); }
    public void Dispose() => Stop();
}

