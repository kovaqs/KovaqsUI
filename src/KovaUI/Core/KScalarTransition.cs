using System.ComponentModel;
using System.Diagnostics;
using KovaUI.Styling;

namespace KovaUI.Core;

internal sealed class KScalarTransition(Control owner) : IDisposable
{
    private float start, target;
    private long started;
    private int duration;
    private KEasing easing;
    private KAnimationStyle? activeOptions;
    private bool initialized, running;
    internal float Sample(float next, KAnimationStyle options)
    {
        activeOptions = options;
        if (!initialized || !options.Enabled || !KAnimationSettings.Enabled || options.Duration == 0 ||
            !owner.Visible || !owner.IsHandleCreated || owner.Site?.DesignMode == true || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        { initialized = true; target = next; Stop(); return next; }
        if (next != target)
        {
            start = Value; target = next; started = Stopwatch.GetTimestamp(); duration = options.Duration; easing = options.Easing;
            running = true; KAnimationClock.Add(Tick);
        }
        return Value;
    }
    internal void Pulse(KAnimationStyle options)
    {
        Stop();
        activeOptions = options;
        start = 1; target = 0; initialized = true; started = Stopwatch.GetTimestamp(); duration = (int)Math.Min(int.MaxValue, Math.Max(1L, (long)options.Duration * 2)); easing = options.Easing;
        running = options.Enabled && options.Duration > 0 && KAnimationSettings.Enabled && owner.Visible && owner.IsHandleCreated && owner.Site?.DesignMode != true && LicenseManager.UsageMode != LicenseUsageMode.Designtime;
        if (running) KAnimationClock.Add(Tick);
        owner.Invalidate();
    }
    internal float Value
    {
        get
        {
            if (!running) return target;
            float t = Math.Clamp((float)Stopwatch.GetElapsedTime(started).TotalMilliseconds / duration, 0, 1);
            if (t >= 1) { Stop(); return target; }
            float eased = easing switch { KEasing.Linear => t, KEasing.SmoothStep => t * t * (3 - 2 * t), _ => 1 - MathF.Pow(1 - t, 3) };
            return start + (target - start) * eased;
        }
    }
    private void Tick()
    {
        if (owner.IsDisposed || !owner.Visible || !owner.IsHandleCreated) { Stop(); return; }
        if (!KAnimationSettings.Enabled || activeOptions is { Enabled: false } or { Duration: 0 })
        { Stop(); owner.Invalidate(); return; }
        _ = Value; owner.Invalidate();
    }
    private void Stop() { running = false; KAnimationClock.Remove(Tick); }
    public void Dispose() => Stop();
}


