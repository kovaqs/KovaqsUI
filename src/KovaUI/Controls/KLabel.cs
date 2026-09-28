using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using KovaUI.Styling;

namespace KovaUI.Controls;

public enum KTextRole { Body, Title, Subtitle, Caption, Section }

/// <summary>A native Label with reusable typography and optional theme colors.</summary>
[ToolboxItem(true)]
public class KLabel : Label, IKThemeControl
{
    private KTheme theme = KTheme.Light;
    private KTextRole textRole;
    private Color customColor = Color.Empty;
    private Font? roleFont;
    private bool customFont;

    public KLabel()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        AutoSize = true;
        ApplyTypography();
        ApplyColor();
    }

    [Category("Kova Appearance"), DefaultValue(KTextRole.Body)]
    [Description("Estilo tipográfico. Font y ForeColor permiten personalizarlo libremente.")]
    public KTextRole TextRole
    {
        get => textRole;
        set
        {
            if (!Enum.IsDefined(value)) throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(KTextRole));
            if (textRole == value) return;
            textRole = value;
            if (!customFont) ApplyTypography();
            ApplyColor();
        }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public KTheme Theme
    {
        get => theme;
        set { theme = value ?? throw new ArgumentNullException(nameof(value)); ApplyColor(); }
    }

    [DefaultValue(true)]
    public override bool AutoSize { get => base.AutoSize; set => base.AutoSize = value; }

    public override Color ForeColor
    {
        get => base.ForeColor;
        set { customColor = value; ApplyColor(); }
    }
    public override void ResetForeColor() => ForeColor = Color.Empty;
    private bool ShouldSerializeForeColor() => !customColor.IsEmpty;

    [AllowNull]
    public override Font Font
    {
        get => base.Font;
        set
        {
            customFont = value is not null;
            if (customFont) base.Font = value;
            else ApplyTypography();
        }
    }
    public override void ResetFont() => Font = null;
    private bool ShouldSerializeFont() => customFont;

    private void ApplyColor() => base.ForeColor = !customColor.IsEmpty ? customColor
        : textRole is KTextRole.Subtitle or KTextRole.Caption or KTextRole.Section
            ? theme.TextSecondary : theme.TextPrimary;

    private void ApplyTypography()
    {
        var (size, style) = textRole switch
        {
            KTextRole.Title => (23f, FontStyle.Bold),
            KTextRole.Subtitle => (11f, FontStyle.Regular),
            KTextRole.Caption => (9f, FontStyle.Regular),
            KTextRole.Section => (9f, FontStyle.Bold),
            _ => (10f, FontStyle.Regular)
        };
        var previous = roleFont;
        roleFont = new Font("Segoe UI", size, style);
        base.Font = roleFont;
        previous?.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) { roleFont?.Dispose(); roleFont = null; }
    }
}

