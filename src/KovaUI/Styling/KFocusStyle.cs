using System.ComponentModel;

namespace KovaUI.Styling;

/// <summary>Independent focus indicator. Empty color inherits Focused.BorderColor or the theme.</summary>
[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed class KFocusStyle
{
    private Color color;
    private float thickness, inset;
    private bool visible;
    internal event EventHandler? Changed;
    [DefaultValue(typeof(Color), "")]
    public Color Color { get => color; set { if (color == value) return; color = value; Changed?.Invoke(this, EventArgs.Empty); } }
    [DefaultValue(0f), Description("Focus stroke width in logical pixels (96 DPI).")]
    public float Thickness { get => thickness; set => SetDimension(ref thickness, value); }
    [DefaultValue(0f), Description("Distance inside the control border in logical pixels (96 DPI).")]
    public float Inset { get => inset; set => SetDimension(ref inset, value); }
    [DefaultValue(false)]
    public bool Visible { get => visible; set { if (visible == value) return; visible = value; Changed?.Invoke(this, EventArgs.Empty); } }
    private void SetDimension(ref float field, float value)
    {
        if (!float.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        if (field == value) return;
        field = value; Changed?.Invoke(this, EventArgs.Empty);
    }
    internal Color Resolve(Color stateColor, Color themeColor) => !Visible || thickness == 0 ? Color.Empty
        : !color.IsEmpty ? color : !stateColor.IsEmpty ? stateColor : themeColor;
    public override string ToString() => "Focus indicator";
}

