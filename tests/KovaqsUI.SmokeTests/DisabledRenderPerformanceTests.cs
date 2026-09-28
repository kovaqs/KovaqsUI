using System.Diagnostics;
using System.Runtime.InteropServices;
using KovaqsUI.Controls;

internal static class DisabledRenderPerformanceTests
{
    [DllImport("user32.dll")] private static extern int GetGuiResources(IntPtr process, uint flags);
    internal static void Run(Action<bool, string> check)
    {
        using var form = new Form();
        using var field = new KTextBox { Multiline = true, Size = new Size(240, 120), Text = "Native שלום rendering\r\nSecond line", Enabled = false };
        form.Controls.Add(field); form.Show(); Application.DoEvents();
        using var bitmap = new Bitmap(field.Width, field.Height);
        void Paint() => field.DrawToBitmap(bitmap, field.ClientRectangle);
        for (int i = 0; i < 10; i++) Paint();
        var process = Process.GetCurrentProcess();
        int before = GetGuiResources(process.Handle, 0);
        long allocationBefore = GC.GetAllocatedBytesForCurrentThread();
        var timer = Stopwatch.StartNew();
        for (int i = 0; i < 300; i++) Paint();
        timer.Stop();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocationBefore;
        check(GetGuiResources(process.Handle, 0) - before < 5, "cached disabled painting holds GDI resources stable");
        Console.WriteLine($"Disabled render: 300 frames, {timer.ElapsedMilliseconds} ms, {allocated / 300} managed bytes/frame (DrawToBitmap included).");
        field.Appearance.Disabled.ForeColor = Color.Magenta;
        Paint();
        check(HasInk(bitmap, Color.Magenta), "cached disabled frame refreshes on foreground change");
        field.Text = ""; Paint();
        check(!HasInk(bitmap, Color.Magenta), "cached disabled frame refreshes on text removal");
        for (int i = 0; i < 100; i++)
        {
            field.Enabled = true; field.Text = "Cycle " + i; field.Enabled = false; Paint();
        }
        check(GetGuiResources(process.Handle, 0) - before < 5, "enable/disable and cache replacement release GDI resources");
        form.Close();
    }
    private static bool HasInk(Bitmap bitmap, Color ink)
    {
        for (int y = 0; y < bitmap.Height; y++)
        for (int x = 0; x < bitmap.Width; x++)
        {
            var c = bitmap.GetPixel(x, y);
            if (Math.Abs(c.R - ink.R) < 20 && Math.Abs(c.G - ink.G) < 20 && Math.Abs(c.B - ink.B) < 20) return true;
        }
        return false;
    }
}
