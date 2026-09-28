using System.Diagnostics;
using KovaUI.Controls;
using KovaUI.Styling;

internal static class AnimationRegressionTests
{
    private sealed class AnimatedButton : KButton { public void Hover(bool value) { if (value) OnMouseEnter(EventArgs.Empty); else OnMouseLeave(EventArgs.Empty); } }
    private sealed class AnimatedField : KTextBox { public void Hover() => OnMouseEnter(EventArgs.Empty); }
    internal static void Run(Action<bool, string> check)
    {
        KAnimationSettings.Enabled = true;
        try
        {
            using var form = new Form();
            using var button = new AnimatedButton { Text = "", FillColor = Color.Black, BorderThickness = 0 };
            using var field = new AnimatedField { Top = 80, FillColor = Color.Black };
            button.Animation.Duration = field.Animation.Duration = 400;
            button.Animation.Easing = field.Animation.Easing = KEasing.Linear;
            button.Appearance.Hover.FillColor = field.Appearance.Hover.FillColor = Color.White;
            form.Controls.AddRange([button, field]); form.Show(); Pump(450);
            button.Hover(false); Pump(450);
            var initial = Pixel(button);
            button.Hover(true); Pixel(button); Pump(100);
            var middle = Pixel(button);
            check(initial.R == 0 && middle.R > 0 && middle.R < 255, "animation renders intermediate button colors");
            button.Hover(false); var reverse = Pixel(button);
            check(Math.Abs(reverse.R - middle.R) < 35, "animation reverses from current visual value without jumping");
            Pump(450); check(Pixel(button).R == 0, "animation reaches exact final state");
            field.Hover(); Pump(100);
            check(field.Editor.BackColor.R > 0 && field.Editor.BackColor.R < 255, "native editor background follows animated frame");
            Pump(450); check(field.Editor.BackColor == Color.FromArgb(255,255,255), "native editor reaches animation target");
            button.Animation.Enabled = false; button.Hover(true);
            check(Pixel(button).R == 255, "per-control motion switch applies state immediately");
            check(!button.FocusAppearance.Visible && button.FocusAppearance.Thickness == 0 && button.FocusAppearance.Color.IsEmpty,
                "focus indicator has no initial stroke or color");
            button.Animation.Enabled = true; button.Hover(false); Pixel(button); Pump(450);
            button.ForeColor = Color.White; Pixel(button); Pump(450);
            button.PerformClick(); Pump(60);
            check(Pixel(button).R > 0, "click animation produces a visible pulse on the button face");
            button.Animation.Duration = 0;
            button.PerformClick();
            check(Pixel(button).R == 0, "zero duration cancels an in-flight click pulse immediately");
            button.Animation.Duration = 5000;
            button.PerformClick(); Pump(40);
            button.Animation.Enabled = false; Pump(50);
            var clockType = typeof(KButton).Assembly.GetType("KovaUI.Core.KAnimationClock")!;
            var activeTimer = (System.Windows.Forms.Timer?)clockType.GetField("timer", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!.GetValue(null);
            check(activeTimer is null || !activeTimer.Enabled, "disabling motion stops a long click pulse before its duration expires");
            button.Dispose(); Pump(900);
            var transitionType = typeof(KButton).Assembly.GetType("KovaUI.Core.KAnimationClock")!;
            var timer = (System.Windows.Forms.Timer?)transitionType.GetField("timer", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!.GetValue(null);
            check(timer is null || !timer.Enabled, "shared animation timer stops after disposal and settled states");
        }
        finally { KAnimationSettings.Enabled = false; }
    }
    private static Color Pixel(Control control) { using var bitmap = new Bitmap(control.Width, control.Height); control.DrawToBitmap(bitmap, control.ClientRectangle); return bitmap.GetPixel(control.Width / 2, control.Height / 2); }
    private static void Pump(int milliseconds) { var watch = Stopwatch.StartNew(); while (watch.ElapsedMilliseconds < milliseconds) { Application.DoEvents(); Thread.Sleep(5); } }
}



