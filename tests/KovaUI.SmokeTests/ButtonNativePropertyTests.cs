using KovaUI.Controls;
using KovaUI.Styling;

internal static class ButtonNativePropertyTests
{
    internal static void Run(Action<bool, string> check)
    {
        using var form = new Form();
        using var button = new KButton { Text = "", FillColor = Color.Magenta, Shape = KShape.Rectangle };
        form.Controls.Add(button); form.Show(); Application.DoEvents();
        int clicks = 0;
        button.Click += (_, _) => clicks++;
        foreach (var style in Enum.GetValues<FlatStyle>())
        {
            ((Button)button).FlatStyle = style;
            using var bitmap = new Bitmap(button.Width, button.Height);
            button.DrawToBitmap(bitmap, button.ClientRectangle);
            check(bitmap.GetPixel(button.Width / 2, button.Height / 2).ToArgb() == Color.Magenta.ToArgb(),
                $"Kova rendering survives inherited FlatStyle={style}");
            int before = clicks;
            button.PerformClick();
            check(clicks == before + 1, $"native button activation survives FlatStyle={style}");
        }
        using (var beforeHandle = new KButton { FlatStyle = FlatStyle.System, FillColor = Color.Magenta, Text = "" })
        {
            form.Controls.Add(beforeHandle);
            using var bitmap = new Bitmap(beforeHandle.Width, beforeHandle.Height);
            beforeHandle.DrawToBitmap(bitmap, beforeHandle.ClientRectangle);
            check(bitmap.GetPixel(beforeHandle.Width / 2, beforeHandle.Height / 2).ToArgb() == Color.Magenta.ToArgb(),
                "System style assigned before HWND creation retains Kova rendering");
        }
        using var image = new Bitmap(8, 8);
        using (var graphics = Graphics.FromImage(image)) graphics.Clear(Color.Lime);
        using var list = new ImageList();
        list.Images.Add("sample", image);
        button.ImageList = list; button.ImageKey = "sample";
        button.ImageAlign = ContentAlignment.MiddleCenter;
        using (var bitmap = new Bitmap(button.Width, button.Height))
        {
            button.DrawToBitmap(bitmap, button.ClientRectangle);
            check(bitmap.GetPixel(button.Width / 2, button.Height / 2).G > 200, "ImageList/ImageKey renders native image selection");
        }
        form.Close();
    }
}
