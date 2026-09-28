using System.Buffers;

namespace KovaqsUI.Core;

// UI-thread scheduler shared by color, geometry and indeterminate animations.
internal static class KAnimationClock
{
    [ThreadStatic] private static HashSet<Action>? callbacks;
    [ThreadStatic] private static System.Windows.Forms.Timer? timer;
    internal static void Add(Action callback)
    {
        callbacks ??= [];
        callbacks.Add(callback);
        if (timer is null)
        {
            timer = new System.Windows.Forms.Timer { Interval = 15 };
            timer.Tick += Tick;
        }
        timer.Start();
    }
    private static void Tick(object? sender, EventArgs e)
    {
        if (callbacks is null || callbacks.Count == 0) return;
        int count = callbacks.Count;
        var snapshot = ArrayPool<Action>.Shared.Rent(count);
        try
        {
            callbacks.CopyTo(snapshot);
            for (int i = 0; i < count; i++)
                if (callbacks.Contains(snapshot[i])) snapshot[i]();
        }
        finally { ArrayPool<Action>.Shared.Return(snapshot, clearArray: true); }
    }
    internal static void Remove(Action callback)
    {
        callbacks?.Remove(callback);
        if (callbacks?.Count == 0) timer?.Stop();
    }
}
