using System.Buffers;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace KovaUI.Core;

/// <summary>Native EDIT behavior and layout, including the inactive presentation.</summary>
internal sealed class KNativeTextBox : TextBox
{
    private const int WmPaint = 0x000F, WmPrintClient = 0x0318, ReflectedCtlColorStatic = 0x2138;
    // A chromatic mask distinguishes glyph coverage from native selection colors.
    private static readonly Color MaskBackground = Color.FromArgb(1, 254, 1);
    private IntPtr maskBrush;
    private bool printingMask;
    private Bitmap? disabledFrame;

    [DllImport("gdi32.dll")] private static extern IntPtr CreateSolidBrush(int color);
    [DllImport("gdi32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool DeleteObject(IntPtr value);
    [DllImport("gdi32.dll")] private static extern int SetBkColor(IntPtr dc, int color);

    internal bool ApplyMaskColors(ref Message message)
    {
        if (!printingMask || message.Msg != 0x0138 || message.LParam != Handle) return false;
        SetBkColor(message.WParam, ColorTranslator.ToWin32(MaskBackground));
        message.Result = maskBrush;
        return true;
    }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == 0x003D && Parent is { } owner) // WM_GETOBJECT: retain the native UIA/MSAA provider.
        {
            AccessibleName = owner.AccessibleName;
            AccessibleDescription = owner.AccessibleDescription;
        }
        // Native setters may change scroll/layout without a managed property event.
        if (!printingMask && message.Msg is 0x000C or 0x0005 or 0x0030 or 0x0015 or 0x001A or 0x031A
            or 0x00B1 or 0x00B3 or 0x00B4 or 0x00B5 or 0x00B6 or 0x00B7 or 0x00C2 or 0x00CB
            or 0x00CC or 0x00D3 or 0x0114 or 0x0115)
            ReleaseFrame();
        base.WndProc(ref message);
        if (printingMask)
        {
            if (message.Msg == ReflectedCtlColorStatic)
            {
                SetBkColor(message.WParam, ColorTranslator.ToWin32(MaskBackground));
                message.Result = maskBrush;
            }
            return;
        }
        if (Enabled || !IsHandleCreated || ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
        if (message.Msg != WmPaint && message.Msg != WmPrintClient) return;
        using var target = message.Msg == WmPrintClient && message.WParam != IntPtr.Zero
            ? Graphics.FromHdc(message.WParam) : Graphics.FromHwnd(Handle);
        if (disabledFrame is null)
        {
            var bitmap = RenderNativeMask();
            try { Recolor(bitmap, BackColor, ForeColor, SystemColors.GrayText); disabledFrame = bitmap; }
            catch { bitmap.Dispose(); throw; }
        }
        target.DrawImageUnscaled(disabledFrame, 0, 0);
    }

    private Bitmap RenderNativeMask()
    {
        var bitmap = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppArgb);
        try
        {
            if (maskBrush == IntPtr.Zero) maskBrush = CreateSolidBrush(ColorTranslator.ToWin32(MaskBackground));
            if (maskBrush == IntPtr.Zero) throw new System.ComponentModel.Win32Exception();
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(MaskBackground);
            IntPtr dc = graphics.GetHdc();
            try
            {
                printingMask = true;
                // Call the native base directly, avoiding recursion through this overlay.
                // Client-only printing preserves the control's native scroll position,
                // wrapping, password mask, tabs, and complex-script shaping.
                var print = Message.Create(Handle, WmPrintClient, dc, new IntPtr(0x0C));
                base.WndProc(ref print);
            }
            finally { printingMask = false; graphics.ReleaseHdc(dc); }
            return bitmap;
        }
        catch { bitmap.Dispose(); throw; }
    }

    private static void Recolor(Bitmap bitmap, Color background, Color foreground, Color ink)
    {
        var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        byte[]? pixels = null;
        try
        {
            int length = checked(data.Stride * data.Height);
            pixels = ArrayPool<byte>.Shared.Rent(length);
            Marshal.Copy(data.Scan0, pixels, 0, length);
            for (int y = 0; y < data.Height; y++)
            for (int x = 0; x < data.Width; x++)
            {
                int i = y * data.Stride + x * 4;
                // ClearType has separate coverage for each color channel. A
                // single grayscale alpha would leave colored native fringes.
                if (InRange(pixels[i], MaskBackground.B, ink.B) &&
                    InRange(pixels[i + 1], MaskBackground.G, ink.G) &&
                    InRange(pixels[i + 2], MaskBackground.R, ink.R))
                {
                    pixels[i] = Blend(background.B, foreground.B, Coverage(pixels[i], MaskBackground.B, ink.B));
                    pixels[i + 1] = Blend(background.G, foreground.G, Coverage(pixels[i + 1], MaskBackground.G, ink.G));
                    pixels[i + 2] = Blend(background.R, foreground.R, Coverage(pixels[i + 2], MaskBackground.R, ink.R));
                }
                pixels[i + 3] = 255; // GDI printing does not maintain bitmap alpha.
            }
            Marshal.Copy(pixels, 0, data.Scan0, length);
        }
        finally
        {
            if (pixels is not null) ArrayPool<byte>.Shared.Return(pixels);
            bitmap.UnlockBits(data);
        }
    }
    private static bool InRange(byte value, byte background, byte ink) => value >= Math.Min(background, ink) - 2 && value <= Math.Max(background, ink) + 2;
    private static double Coverage(byte value, byte background, byte ink) => ink == background ? 0 : Math.Clamp((double)(value - background) / (ink - background), 0, 1);
    private static byte Blend(byte background, byte foreground, double alpha) => (byte)Math.Clamp((int)Math.Round(background + (foreground - background) * alpha), 0, 255);
    private void ReleaseBrush() { if (maskBrush != IntPtr.Zero) { DeleteObject(maskBrush); maskBrush = IntPtr.Zero; } }
    private void ReleaseFrame() { disabledFrame?.Dispose(); disabledFrame = null; }
    protected override void OnInvalidated(InvalidateEventArgs e) { if (!printingMask) ReleaseFrame(); base.OnInvalidated(e); }
    protected override void OnForeColorChanged(EventArgs e) { ReleaseFrame(); base.OnForeColorChanged(e); }
    protected override void OnBackColorChanged(EventArgs e) { ReleaseFrame(); base.OnBackColorChanged(e); }
    protected override void OnHandleDestroyed(EventArgs e) { ReleaseFrame(); ReleaseBrush(); base.OnHandleDestroyed(e); }
    protected override void OnEnabledChanged(EventArgs e) { ReleaseFrame(); if (Enabled) ReleaseBrush(); base.OnEnabledChanged(e); }
    protected override void Dispose(bool disposing) { if (disposing) { ReleaseFrame(); ReleaseBrush(); } base.Dispose(disposing); }
}



