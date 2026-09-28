using System.Runtime.InteropServices;
using KovaUI.Controls;

internal static class TextPointerStateTests
{
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);
    internal static void Run(Action<bool, string> check)
    {
        using var form = new Form();
        using var field = new PointerField();
        field.Appearance.Normal.FillColor = Color.White;
        field.Appearance.Focused.FillColor = Color.White;
        field.Appearance.Hover.FillColor = Color.White;
        field.Appearance.Pressed.FillColor = Color.Coral;
        form.Controls.Add(field); form.Show(); Application.DoEvents();
        field.PressFrame();
        check(field.Editor.BackColor.ToArgb() == Color.Coral.ToArgb(), "text frame activates configurable pressed state");
        field.ReleaseFrame();
        check(field.Editor.BackColor.ToArgb() == Color.White.ToArgb(), "text frame releases pressed state");
        SendMessage(field.Editor.Handle, 0x0201, new IntPtr(1), new IntPtr(5 | (5 << 16)));
        check(field.Editor.BackColor.ToArgb() == Color.Coral.ToArgb(), "native editor activates shared pressed state");
        SendMessage(field.Editor.Handle, 0x0202, IntPtr.Zero, new IntPtr(5 | (5 << 16)));
        check(field.Editor.BackColor.ToArgb() == Color.White.ToArgb(), "native editor releases shared pressed state");
        SendMessage(field.Editor.Handle, 0x0201, new IntPtr(1), new IntPtr(5 | (5 << 16)));
        field.Editor.Capture = false;
        check(field.Editor.BackColor.ToArgb() == Color.White.ToArgb(), "losing native mouse capture clears pressed state");
        field.PressFrame(); field.Enabled = false; field.Enabled = true;
        check(field.Editor.BackColor.ToArgb() == Color.White.ToArgb(), "disabling clears text pressed state");
        form.Close();
    }
    private sealed class PointerField : KTextBox
    {
        internal void PressFrame() => OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 3, 3, 0));
        internal void ReleaseFrame() => OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, 3, 3, 0));
    }
}
