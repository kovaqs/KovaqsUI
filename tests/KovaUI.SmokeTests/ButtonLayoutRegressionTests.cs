using KovaUI.Controls;
using KovaUI.Styling;

internal static class ButtonLayoutRegressionTests
{
    internal static void Run(Action<bool, string> check)
    {
        using var icon = new Bitmap(8, 8);
        using (var graphics = Graphics.FromImage(icon)) graphics.Clear(Color.Lime);
        using var button = new LayoutButton
        {
            Text = "", Image = icon, ImageSize = new Size(12, 12), Size = new Size(120, 60),
            FillColor = Color.Black, BorderThickness = 0, Shape = KShape.Rectangle,
            TextImageRelation = TextImageRelation.ImageBeforeText, ImageAlign = ContentAlignment.TopRight
        };
        using (var bitmap = button.Render())
            check(bitmap.GetPixel(102, 8).G > 200 && bitmap.GetPixel(17, 30).G < 100, "image-only button respects ImageAlign with non-overlay relation");
        button.Text = "Text";
        button.ImageAlign = ContentAlignment.TopLeft;
        using (var bitmap = button.Render())
            check(bitmap.GetPixel(17, 8).G > 200, "horizontal image relation respects vertical image alignment");
        button.Text = "";
        button.TextImageRelation = TextImageRelation.Overlay;
        button.RightToLeft = RightToLeft.Yes;
        using (var bitmap = button.Render())
            check(bitmap.GetPixel(102, 8).G > 200, "RTL mirrors image alignment");

        using var host = new FlowLayoutPanel { Size = new Size(800, 300) };
        using var auto = new KButton
        {
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Text = "Automatic", Image = icon, TextImageRelation = TextImageRelation.ImageBeforeText
        };
        host.Controls.Add(auto); host.PerformLayout();
        int before = auto.Width;
        auto.ImageSpacing += 40;
        check(auto.Width >= before + 40, "AutoSize updates after ImageSpacing changes");
        before = auto.Width;
        auto.ImageSize = new Size(auto.ImageSize.Width + 30, auto.ImageSize.Height);
        check(auto.Width >= before + 30, "AutoSize updates after ImageSize changes");
        auto.Text = "A longer text that must wrap when the proposed width is constrained";
        var wide = auto.GetPreferredSize(new Size(800, 0));
        var narrow = auto.GetPreferredSize(new Size(160, 0));
        check(narrow.Height > wide.Height, "preferred size accounts for text wrapping at constrained width");
    }

    private sealed class LayoutButton : KButton
    {
        internal Bitmap Render()
        {
            var bitmap = new Bitmap(Width, Height);
            using var graphics = Graphics.FromImage(bitmap);
            OnPaint(new PaintEventArgs(graphics, ClientRectangle));
            return bitmap;
        }
    }
}
