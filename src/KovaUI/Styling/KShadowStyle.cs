using System.ComponentModel;

namespace KovaUI.Styling;

[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed class KShadowStyle
{
    private bool enabled;
    private Color color = Color.Black;
    private int blur = 8, opacity = 24;
    private Point offset = new(0, 2);
    internal event EventHandler? Changed;
    [DefaultValue(false)] public bool Enabled { get => enabled; set { enabled = value; Changed?.Invoke(this, EventArgs.Empty); } }
    [DefaultValue(typeof(Color), "Black")] public Color Color { get => color; set { color = value; Changed?.Invoke(this, EventArgs.Empty); } }
    [DefaultValue(8)] public int Blur { get => blur; set { if (value < 0 || value > 64) throw new ArgumentOutOfRangeException(nameof(value)); blur = value; Changed?.Invoke(this, EventArgs.Empty); } }
    [DefaultValue(24)] public int Opacity { get => opacity; set { if (value < 0 || value > 255) throw new ArgumentOutOfRangeException(nameof(value)); opacity = value; Changed?.Invoke(this, EventArgs.Empty); } }
    [DefaultValue(typeof(Point), "0, 2")] public Point Offset { get => offset; set { if (Math.Abs((long)value.X) > 64 || Math.Abs((long)value.Y) > 64) throw new ArgumentOutOfRangeException(nameof(value)); offset = value; Changed?.Invoke(this, EventArgs.Empty); } }
    public override string ToString() => "Soft shadow";
}
