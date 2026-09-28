using KovaqsUI.Controls;

internal static class FocusStateRegressionTests
{
    internal static void Run(Action<bool, string> check)
    {
        using var form = new Form();
        using var field = new KTextBox { Text = "MMMM", BorderThickness = 0 };
        field.Appearance.Normal.FillColor = Color.White;
        field.FocusAppearance.Visible = true;
        field.FocusAppearance.Thickness = 1.5f;
        field.FocusAppearance.Inset = 3;
        field.Appearance.Focused.BorderColor = Color.Magenta;
        form.Controls.Add(field); form.Show(); Application.DoEvents(); field.Focus();
        using (var bitmap = Capture(field))
            check(HasColor(bitmap, Color.Magenta), "borderless text field retains configurable visible focus");
        field.FocusAppearance.Color = Color.Lime;
        using (var bitmap = Capture(field))
            check(HasColor(bitmap, Color.Lime) && !HasColor(bitmap, Color.Magenta), "focus color can override the state border color independently");
        field.FocusAppearance.Visible = false;
        using (var bitmap = Capture(field))
            check(!HasColor(bitmap, Color.Lime), "focus indicator can be hidden explicitly");
        field.Appearance.Disabled.ForeColor = Color.Magenta;
        field.Enabled = false;
        using (var bitmap = Capture(field))
            check(HasColor(bitmap, Color.Magenta), "native disabled editor uses configured foreground");
        field.UseSystemPasswordChar = true;
        field.Text = "secret";
        using (var password = Capture(field))
        {
            field.Text = "xxxxxx";
            using var masked = Capture(field);
            check(EqualPixels(password, masked), "disabled password rendering never exposes characters");
        }
        field.UseSystemPasswordChar = false;
        field.Text = "Before";
        field.Select(1, 2);
        field.Enabled = true;
        check(field.Text == "Before" && field.SelectionStart == 1 && field.SelectionLength == 2, "reenabling preserves native text and selection");
        field.Appearance.Normal.ForeColor = Color.Magenta;
        field.Appearance.Disabled.FillColor = Color.White;
        field.Select(0, 0);
        foreach (var direction in new[] { RightToLeft.No, RightToLeft.Yes })
        foreach (var alignment in Enum.GetValues<HorizontalAlignment>())
        foreach (var sample in new[] { "MMMM", "ש", "שלום", "abc שלום 123", "שלום abc 123" })
        {
            field.Enabled = true; field.RightToLeft = direction; field.TextAlign = alignment;
            field.Text = sample;
            using var native = Capture(field);
            field.Enabled = false;
            using var disabled = Capture(field);
            var expected = TextBounds(native);
            var actual = TextBounds(disabled);
            check(!actual.IsEmpty && Math.Abs(actual.Left - expected.Left) <= 2 && Math.Abs(actual.Right - expected.Right) <= 2,
                $"disabled text preserves native alignment ({direction}, {alignment}, {sample})");
        }
        field.RightToLeft = RightToLeft.No;
        field.TextAlign = HorizontalAlignment.Left;
        field.Size = new Size(170, 90);
        foreach (bool multiline in new[] { false, true })
        {
            field.Enabled = true;
            field.Multiline = multiline;
            field.WordWrap = multiline;
            field.Text = string.Join(multiline ? "\r\n" : " ", Enumerable.Repeat("Mixed שלום abc\t123", 12));
            field.Select(field.TextLength, 0); field.ScrollToCaret();
            using var native = Capture(field);
            field.Enabled = false;
            using var disabled = Capture(field);
            var expected = TextBounds(native);
            var actual = TextBounds(disabled);
            check(!actual.IsEmpty && Math.Abs(expected.Left - actual.Left) <= 2 && Math.Abs(expected.Right - actual.Right) <= 2
                && Math.Abs(expected.Top - actual.Top) <= 2 && Math.Abs(expected.Bottom - actual.Bottom) <= 2,
                $"native disabled mask preserves scrolled content (multiline={multiline})");
        }
        field.Multiline = false; field.Text = "MMMM"; field.Select(0, 0); field.ScrollToCaret();
        field.Appearance.Disabled.FillColor = SystemColors.GrayText;
        using (var bitmap = Capture(field))
            check(HasColor(bitmap, Color.Magenta), "custom text remains visible when background equals native disabled ink");
        form.Close();
    }
    private static Bitmap Capture(Control control)
    {
        var bitmap = new Bitmap(control.Width, control.Height);
        control.DrawToBitmap(bitmap, control.ClientRectangle);
        return bitmap;
    }
    private static bool HasColor(Bitmap bitmap, Color target)
    {
        for (int y = 0; y < bitmap.Height; y++)
        for (int x = 0; x < bitmap.Width; x++)
        {
            var color = bitmap.GetPixel(x, y);
            if (Math.Abs(color.R - target.R) < 30 && Math.Abs(color.G - target.G) < 30 && Math.Abs(color.B - target.B) < 30) return true;
        }
        return false;
    }
    private static bool EqualPixels(Bitmap first, Bitmap second)
    {
        for (int y = 0; y < first.Height; y++)
        for (int x = 0; x < first.Width; x++)
            if (first.GetPixel(x, y) != second.GetPixel(x, y)) return false;
        return true;
    }
    private static Rectangle TextBounds(Bitmap bitmap)
    {
        int left = bitmap.Width, top = bitmap.Height, right = -1, bottom = -1;
        for (int y = 0; y < bitmap.Height; y++)
        for (int x = 0; x < bitmap.Width; x++)
        {
            var c = bitmap.GetPixel(x, y);
            if (c.R < 190 || c.G > 60 || c.B < 190) continue;
            left = Math.Min(left, x); right = Math.Max(right, x); top = Math.Min(top, y); bottom = Math.Max(bottom, y);
        }
        return right < 0 ? Rectangle.Empty : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
    }
}




