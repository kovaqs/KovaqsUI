using System.ComponentModel;

namespace KovaqsUI.Styling;

public enum KGradientMode { Inherit, Solid, Linear }

/// <summary>Empty colors inherit from the normal style or theme. Dimensions use logical pixels.</summary>
[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed class KStateStyle
{
    private Color fill, end, text, border;
    private float? angle;
    private KGradientMode gradientMode;
    internal event EventHandler? Changed;
    private void Set(ref Color field, Color value) { if (field == value) return; field = value; Changed?.Invoke(this, EventArgs.Empty); }
    [DefaultValue(typeof(Color), "")]
    public Color FillColor { get => fill; set => Set(ref fill, value); }
    [DefaultValue(typeof(Color), "")]
    public Color GradientColor { get => end; set => Set(ref end, value); }
    [DefaultValue(typeof(Color), "")]
    public Color ForeColor { get => text; set => Set(ref text, value); }
    [DefaultValue(typeof(Color), "")]
    public Color BorderColor { get => border; set => Set(ref border, value); }
    [DefaultValue(null), Description("Gradient angle in degrees. An empty value inherits the preceding state (normal defaults to 90°).")]
    public float? GradientAngle
    {
        get => angle;
        set
        {
            if (value.HasValue && !float.IsFinite(value.Value)) throw new ArgumentOutOfRangeException(nameof(value));
            if (angle == value) return;
            angle = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
    [DefaultValue(KGradientMode.Inherit), Description("Inherit the preceding state, force a solid fill, or enable a linear gradient using the resolved colors.")]
    public KGradientMode GradientMode
    {
        get => gradientMode;
        set
        {
            if (!Enum.IsDefined(value)) throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(KGradientMode));
            if (gradientMode == value) return;
            gradientMode = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
    public override string ToString() => "Style overrides";
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed class KAppearance
{
    public KAppearance() { foreach (var state in new[] { Normal, Hover, Pressed, Focused, Disabled }) state.Changed += (_, _) => Changed?.Invoke(this, EventArgs.Empty); }
    internal event EventHandler? Changed;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] public KStateStyle Normal { get; } = new();
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] public KStateStyle Hover { get; } = new();
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] public KStateStyle Pressed { get; } = new();
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] public KStateStyle Focused { get; } = new();
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] public KStateStyle Disabled { get; } = new();
    public override string ToString() => "Normal / Hover / Pressed / Focused / Disabled";
}

