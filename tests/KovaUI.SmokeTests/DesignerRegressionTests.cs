using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;

using KovaUI.Controls;

internal static class DesignerRegressionTests
{
    internal static void Run(Action<bool, string> check)
    {
        using var surface = new DesignSurface(typeof(Form));
        var host = (IDesignerHost)surface.GetService(typeof(IDesignerHost))!;
        var root = (Form)host.RootComponent!;
        var button = (KButton)host.CreateComponent(typeof(KButton), "actionButton");
        var field = (KTextBox)host.CreateComponent(typeof(KTextBox), "nameField");
        root.Controls.AddRange([button, field]);
        var tabStop = TypeDescriptor.GetProperties(field)[nameof(KTextBox.TabStop)]!;
        field.TabStop = false;
        tabStop.ResetValue(field);
        check(field.TabStop && field.Editor.TabStop && !tabStop.ShouldSerializeValue(field),
            "designer reset restores the text field's actual TabStop default");
        check(button.Site?.DesignMode == true && field.Site?.DesignMode == true, "controls instantiate on a real DesignSurface");
        check(host.GetDesigner(button) is not null && host.GetDesigner(field) is not null, "designer host resolves control designers");
        TypeDescriptor.GetProperties(button)[nameof(KButton.CornerRadius)]!.SetValue(button, 17);
        TypeDescriptor.GetProperties(field)[nameof(KTextBox.PlaceholderText)]!.SetValue(field, "Name");
        check(button.CornerRadius == 17 && field.PlaceholderText == "Name", "designer property descriptors edit live control instances");
        button.Appearance.Hover.FillColor = Color.Coral;
        button.Appearance.Pressed.GradientAngle = 35;
        button.FocusAppearance.Thickness = 2.5f;
        button.Animation.Duration = 240;
        button.Animation.Easing = KovaUI.Styling.KEasing.SmoothStep;
        field.Animation.Enabled = false;
        field.Text = "Persisted text";
        var serializer = new CodeDomComponentSerializationService(surface);
        check(serializer is not null, "CodeDOM serialization service initializes against designer host");
        using var store = serializer!.CreateStore();
        serializer.Serialize(store, button);
        serializer.Serialize(store, field);
        store.Close();
        check(store.Errors.Count == 0, "designer serializes both controls without errors");
        using var restored = new Container();
        serializer.DeserializeTo(store, restored);
        var restoredButton = restored.Components.OfType<KButton>().Single();
        var restoredField = restored.Components.OfType<KTextBox>().Single();
        check(restoredButton.CornerRadius == 17 && restoredButton.Appearance.Hover.FillColor == Color.Coral
            && restoredButton.Appearance.Pressed.GradientAngle == 35 && restoredButton.FocusAppearance.Thickness == 2.5f,
            "designer round trip preserves nested appearance and focus properties");
        check(restoredButton.Animation.Duration == 240 && restoredButton.Animation.Easing == KovaUI.Styling.KEasing.SmoothStep && !restoredField.Animation.Enabled,
            "designer round trip preserves animation options");
        check(restoredField.Text == "Persisted text" && restoredField.PlaceholderText == "Name",
            "designer round trip preserves native editor content");
        check(restoredField.Controls.Count == 1 && ReferenceEquals(restoredField.Controls[0], restoredField.Editor),
            "designer round trip does not duplicate the private native editor");
        using var grid = new PropertyGrid { SelectedObject = button };
        grid.CreateControl();
        check(grid.SelectedObject == button && TypeDescriptor.GetProperties(button)[nameof(KButton.Appearance)]!.Converter.GetPropertiesSupported(),
            "PropertyGrid can inspect expandable control appearance");
    }
}


