using KovaUI.Controls;

namespace KovaUI.Demo;

/// <summary>Modeless editor over the actual sample controls, not cloned previews.</summary>
internal sealed class AppearanceInspector : Form
{
    private readonly PropertyGrid properties = new() { Dock = DockStyle.Fill, HelpVisible = true, ToolbarVisible = true };
    private readonly ComboBox selector = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Control para personalizar" };

    internal AppearanceInspector(IReadOnlyList<Control> samples)
    {
        Text = "KovaUI · Personalización en vivo";
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(450, 650);
        MinimumSize = new Size(360, 420);
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        var help = new Label
        {
            Dock = DockStyle.Bottom, Height = 72, Padding = new Padding(10),
            Text = "Expande Appearance y FocusAppearance. Los cambios se aplican a la Demo.\r\nClic derecho → Restablecer recupera el valor predeterminado."
        };
        Controls.Add(properties); Controls.Add(selector); Controls.Add(help);
        int index = 0;
        foreach (var sample in samples)
        {
            string label = sample is KTextBox field ? field.PlaceholderText : sample.Text.Replace("&", "");
            if (string.IsNullOrWhiteSpace(label)) label = "Campo de texto";
            selector.Items.Add(new Sample(++index, label, sample));
        }
        selector.SelectedIndexChanged += (_, _) =>
        {
            properties.SelectedObject = (selector.SelectedItem as Sample)?.Control;
            ShowAppearance();
        };
        if (selector.Items.Count > 0) selector.SelectedIndex = 0;
        Shown += (_, _) => ShowAppearance();
    }
    private void ShowAppearance()
    {
        properties.CollapseAllGridItems();
        var root = properties.SelectedGridItem;
        while (root?.Parent is not null) root = root.Parent;
        if (root is null) return;
        foreach (GridItem category in root.GridItems)
        {
            if (category.Label != "Kova Appearance") continue;
            category.Expanded = true;
            foreach (GridItem property in category.GridItems)
            {
                if (property.PropertyDescriptor?.Name != "Appearance") continue;
                property.Expanded = true;
                properties.SelectedGridItem = property;
                return;
            }
        }
    }
    private sealed record Sample(int Number, string Label, Control Control)
    {
        public override string ToString() => $"{Number:00} · {Control.GetType().Name} · {Label}";
    }
}
