using System.ComponentModel;
using KovaqsUI.Controls;
using KovaqsUI.Styling;

internal static class RenderingRegressionTests
{
    internal static void Run(Action<bool, string> check)
    {
        using var button = new RenderButton { Text = string.Empty, FillColor = Color.Red, GradientColor = Color.Blue };
        foreach (var shape in Enum.GetValues<KShape>())
        foreach (int edge in new[] { 1, 2, 3, 4, 8, 16 })
        foreach (int border in new[] { 0, 1, 8, 100 })
        {
            button.Shape = shape;
            button.Size = new Size(edge, edge);
            button.BorderThickness = border;
            using var bitmap = button.Render();
        }
        check(true, "gradient rendering at 1–16 px with all shapes and 0–100 px borders");
        button.Size = new Size(120, 60);
        button.Shape = KShape.Rectangle;
        button.BorderThickness = 0;
        using (var normal = button.Render())
            check(normal.GetPixel(60, 10).R > normal.GetPixel(60, 50).R, "normal gradient defaults to vertical");
        button.Appearance.Hover.GradientAngle = 0;
        button.Hover();
        using (var hover = button.Render())
        {
            check(hover.GetPixel(10, 30).R > hover.GetPixel(110, 30).R, "hover angle overrides without duplicating gradient colors");
            check(Math.Abs(hover.GetPixel(60, 10).R - hover.GetPixel(60, 50).R) <= 1, "hover angle removes inherited vertical direction");
        }
        button.Appearance.Hover.GradientMode = KGradientMode.Solid;
        using (var solid = button.Render())
            check(solid.GetPixel(10, 30).ToArgb() == Color.Red.ToArgb() && solid.GetPixel(110, 30).ToArgb() == Color.Red.ToArgb(), "solid hover overrides a gradient normal state");
        button.Appearance.Pressed.GradientMode = KGradientMode.Linear;
        button.Press();
        using (var pressed = button.Render())
            check(pressed.GetPixel(10, 30).R > pressed.GetPixel(110, 30).R, "pressed can restore gradient endpoints after solid hover");
        var angleProperty = TypeDescriptor.GetProperties(button.Appearance.Hover)[nameof(KStateStyle.GradientAngle)]!;
        check(angleProperty.ShouldSerializeValue(button.Appearance.Hover), "designer preserves an angle-only override");
        angleProperty.ResetValue(button.Appearance.Hover);
        check(button.Appearance.Hover.GradientAngle is null && !angleProperty.ShouldSerializeValue(button.Appearance.Hover), "designer resets gradient angle to inheritance");
        int concreteEvents = 0, controlEvents = 0;
        button.Paint += (_, _) => concreteEvents++;
        ((Control)button).Paint += (_, e) =>
        {
            controlEvents++;
            e.Graphics.FillRectangle(Brushes.Lime, 40, 20, 10, 10);
        };
        using (var painted = button.Render())
        {
            check(concreteEvents == 1 && controlEvents == 1, "Paint event fires once through KButton and Control references");
            check(painted.GetPixel(45, 25).ToArgb() == Color.Lime.ToArgb(), "consumer Paint renders over the Kova face");
        }
    }

    private sealed class RenderButton : KButton
    {
        internal void Hover() => OnMouseEnter(EventArgs.Empty);
        internal void Press() => OnKeyDown(new KeyEventArgs(Keys.Space));
        internal Bitmap Render()
        {
            var bitmap = new Bitmap(Width, Height);
            try
            {
                using var graphics = Graphics.FromImage(bitmap);
                OnPaint(new PaintEventArgs(graphics, ClientRectangle));
                return bitmap;
            }
            catch { bitmap.Dispose(); throw; }
        }
    }
}
