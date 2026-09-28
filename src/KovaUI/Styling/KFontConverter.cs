using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace KovaUI.Styling;

/// <summary>Convertidor tipográfico de tiempo de diseño que preserva las fuentes privadas de KovaUI en el PropertyGrid y la serialización.</summary>
public class KFontConverter : FontConverter
{
    public override object CreateInstance(ITypeDescriptorContext? context, IDictionary propertyValues)
    {
        ArgumentNullException.ThrowIfNull(propertyValues);

        string? name = propertyValues["Name"] as string;
        float size = 10f;
        if (propertyValues["Size"] is float f) size = f;
        else if (propertyValues["Size"] is double d) size = (float)d;
        else if (propertyValues["Size"] is int i) size = i;
        else if (propertyValues["Size"] is not null && float.TryParse(propertyValues["Size"]!.ToString(), out float parsed)) size = parsed;

        GraphicsUnit unit = propertyValues["Unit"] is GraphicsUnit u ? u : GraphicsUnit.Point;

        bool bold = propertyValues["Bold"] is true;
        bool italic = propertyValues["Italic"] is true;
        bool strikeout = propertyValues["Strikeout"] is true;
        bool underline = propertyValues["Underline"] is true;

        FontStyle style = FontStyle.Regular;
        if (bold) style |= FontStyle.Bold;
        if (italic) style |= FontStyle.Italic;
        if (strikeout) style |= FontStyle.Strikeout;
        if (underline) style |= FontStyle.Underline;

        if (!string.IsNullOrWhiteSpace(name) && KFontManager.IsPrivateFamily(name))
        {
            return KFontManager.CreateFont(name, size, style, unit);
        }

        // Si el contexto tiene un control con fuente privada y el nombre entrante se degradó
        Control? control = context?.Instance switch
        {
            Control c => c,
            object[] arr when arr.Length > 0 && arr[0] is Control c => c,
            _ => null
        };

        if (control is not null && control.Font is { } current && KFontManager.IsPrivateFamily(current.Name))
        {
            if (string.IsNullOrWhiteSpace(name) || string.Equals(name, "Microsoft Sans Serif", StringComparison.OrdinalIgnoreCase))
            {
                return KFontManager.CreateFont(current.Name, size, style, unit);
            }
        }

        var fallback = (Font)base.CreateInstance(context, propertyValues);
        return KFontManager.ResolveFont(fallback);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
    {
        if (value is string text && !string.IsNullOrWhiteSpace(text))
        {
            if (KFontManager.TryMatchPrivateFamily(text, out string matched, out _))
            {
                var parts = text.Split(',');
                float size = 10f;
                GraphicsUnit unit = GraphicsUnit.Point;
                FontStyle style = FontStyle.Regular;

                if (parts.Length > 1)
                {
                    var sizeToken = parts[1].Trim();
                    if (sizeToken.EndsWith("pt", StringComparison.OrdinalIgnoreCase))
                    {
                        unit = GraphicsUnit.Point;
                        sizeToken = sizeToken[..^2].Trim();
                    }
                    else if (sizeToken.EndsWith("px", StringComparison.OrdinalIgnoreCase))
                    {
                        unit = GraphicsUnit.Pixel;
                        sizeToken = sizeToken[..^2].Trim();
                    }

                    if (float.TryParse(sizeToken, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parsedSize) ||
                        float.TryParse(sizeToken, System.Globalization.NumberStyles.Float, culture ?? System.Globalization.CultureInfo.CurrentCulture, out parsedSize))
                    {
                        size = parsedSize;
                    }
                }

                var styleTokens = parts.Length > 2 ? string.Join(",", parts[2..]) : (parts.Length > 1 && parts[1].Contains("style", StringComparison.OrdinalIgnoreCase) ? parts[1] : string.Empty);
                if (styleTokens.Contains("Bold", StringComparison.OrdinalIgnoreCase)) style |= FontStyle.Bold;
                if (styleTokens.Contains("Italic", StringComparison.OrdinalIgnoreCase)) style |= FontStyle.Italic;
                if (styleTokens.Contains("Underline", StringComparison.OrdinalIgnoreCase)) style |= FontStyle.Underline;
                if (styleTokens.Contains("Strikeout", StringComparison.OrdinalIgnoreCase)) style |= FontStyle.Strikeout;

                return KFontManager.CreateFont(parts[0].Trim(), size, style, unit);
            }
        }
        var raw = base.ConvertFrom(context, culture, value);
        return raw is Font font ? KFontManager.ResolveFont(font) : raw;
    }
}
