using System.Runtime.InteropServices;
using KovaUI.Controls;

internal static class NativeEditRegressionTests
{
    private const uint WmChar = 0x0102;
    private const uint EmReplaceSel = 0x00C2;
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SendMessageW")]
    private static extern IntPtr ReplaceSelection(IntPtr window, uint message, IntPtr wParam, string text);

    internal static void Run(Action<bool, string> check)
    {
        using var form = new Form();
        using var field = new KTextBox { MaxLength = 4 };
        form.Controls.Add(field);
        form.Show(); Application.DoEvents(); field.Focus();
        field.Enabled = false; field.Enabled = true; field.Focus();
        Type(field, "abcdef");
        check(field.Text == "abcd", "native character input respects MaxLength");
        field.SelectAll(); field.ReadOnly = true;
        Type(field, "x");
        check(field.Text == "abcd", "native character input cannot change readonly text");
        field.ReadOnly = false; field.Clear(); field.CharacterCasing = CharacterCasing.Upper;
        Type(field, "ábc");
        check(field.Text == "ÁBC", "native character input applies CharacterCasing with Unicode");
        field.CharacterCasing = CharacterCasing.Normal;
        field.Clear(); field.MaxLength = 100;
        field.KeyPress += (_, e) => { if (e.KeyChar == '#') e.Handled = true; };
        Type(field, "a#b");
        check(field.Text == "ab", "wrapper KeyPress can suppress native character input");
        field.Text = "Before"; field.SelectAll(); field.ClearUndo();
        ReplaceSelection(field.Editor.Handle, EmReplaceSel, new IntPtr(1), "After");
        check(field.Text == "After" && field.CanUndo, "native replacement records undo history");
        field.Undo();
        check(field.Text == "Before", "Undo restores native edit content");
        form.Close();
    }
    private static void Type(KTextBox field, string text)
    {
        // Deliver only to this test's EDIT HWND. Never touch global keyboard or clipboard state.
        foreach (char c in text) SendMessage(field.Editor.Handle, WmChar, new IntPtr(c), new IntPtr(1));
    }
}
