using System.Drawing.Drawing2D;

namespace KovaUI.Demo;

internal static class DemoIcons
{
    internal static Bitmap Create(string glyph, Color color)
    {
        var image = new Bitmap(24, 24);
        using var graphics = Graphics.FromImage(image);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(color, 2) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        switch (glyph)
        {
            case "add":
                graphics.DrawLine(pen, 5, 12, 19, 12);
                graphics.DrawLine(pen, 12, 5, 12, 19);
                break;
            case "search":
                graphics.DrawEllipse(pen, 4, 4, 11, 11);
                graphics.DrawLine(pen, 14, 14, 20, 20);
                break;
            case "clear":
                graphics.DrawLine(pen, 6, 6, 18, 18);
                graphics.DrawLine(pen, 18, 6, 6, 18);
                break;
        }
        return image;
    }
}
