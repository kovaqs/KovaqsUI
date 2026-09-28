using System.ComponentModel;

namespace KovaUI.Styling;

public enum KEasing { Linear, EaseOutCubic, SmoothStep }

/// <summary>Application-wide motion switch; disable for reduced-motion preferences.</summary>
public static class KAnimationSettings
{
    public static bool Enabled { get; set; } = true;
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed class KAnimationStyle
{
    private int duration = 150;
    [DefaultValue(true)] public bool Enabled { get; set; } = true;
    [DefaultValue(150), Description("Transition duration in milliseconds. Zero changes immediately.")]
    public int Duration { get => duration; set { ArgumentOutOfRangeException.ThrowIfNegative(value); duration = value; } }
    [DefaultValue(KEasing.EaseOutCubic)] public KEasing Easing { get; set; } = KEasing.EaseOutCubic;
    public override string ToString() => "State transitions";
}
