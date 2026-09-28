using System.Runtime.InteropServices;
using KovaqsUI.Controls;

internal static class DpiRegressionTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hwnd, uint message, IntPtr wParam, ref NativeRect rectangle);
    internal static void Run(Action<bool, string> check)
    {
        using var form = new Form { AutoScaleMode = AutoScaleMode.Dpi, ClientSize = new Size(500, 250) };
        using var button = new KButton { Text = "Scale", Location = new Point(20, 20) };
        using var field = new KTextBox { Text = "Text", Location = new Point(20, 80) };
        form.Controls.AddRange([button, field]); form.Show(); Application.DoEvents();
        int originalDpi = form.DeviceDpi;
        var originalField = field.Size;
        var originalButton = button.Size;
        Console.WriteLine($"Environment: {Screen.AllScreens.Length} screen(s), native initial DPI={originalDpi}.");
        foreach (int dpi in new[] { 120, 144, 192, 96, originalDpi })
        {
            float ratio = (float)dpi / form.DeviceDpi;
            var bounds = form.Bounds;
            var suggested = new NativeRect { Left = bounds.Left, Top = bounds.Top, Right = bounds.Left + (int)(bounds.Width * ratio), Bottom = bounds.Top + (int)(bounds.Height * ratio) };
            SendMessage(form.Handle, 0x02E0, new IntPtr(dpi | (dpi << 16)), ref suggested);
            Application.DoEvents();
            Console.WriteLine($"DPI message {dpi}: form={form.DeviceDpi}, button={button.DeviceDpi}, field={field.DeviceDpi}, editor={field.Editor.DeviceDpi}; field={field.Size}, editor={field.Editor.Bounds}");
            check(form.DeviceDpi == dpi, $"WinForms processes synthetic WM_DPICHANGED ({dpi})");
            check(field.Editor.Right <= field.ClientSize.Width && field.Editor.Bottom <= field.ClientSize.Height,
                $"native editor remains inside frame after DPI message ({dpi})");
        }
        check(Math.Abs(field.Width - originalField.Width) <= 2 && Math.Abs(field.Height - originalField.Height) <= 2
            && Math.Abs(button.Width - originalButton.Width) <= 2 && Math.Abs(button.Height - originalButton.Height) <= 2,
            "synthetic DPI round trip restores control sizes within rounding tolerance");
        Console.WriteLine("Synthetic DPI messages do not change the monitor DPI; child DeviceDpi may remain at the native value. Physical monitor migration remains unverified.");
        form.Close();
    }
}
