using System.ComponentModel;
using KovaqsUI.Controls;

internal static class TextBehaviorRegressionTests
{
    internal static void Run(Action<bool, string> check)
    {
        using var form = new ValidationForm { AutoValidate = AutoValidate.EnablePreventFocusChange };
        using var field = new KTextBox { TabIndex = 0 };
        using var next = new Button { Text = "Next", TabIndex = 1, Top = 60 };
        form.Controls.AddRange([field, next]);
        var model = new TextModel();
        field.DataBindings.Add(nameof(KTextBox.Text), model, nameof(TextModel.Value), false, DataSourceUpdateMode.OnPropertyChanged);
        form.Show(); Application.DoEvents();
        check(field.Text == "Initial", "Text binding loads initial model value");
        field.Editor.Text = "Native change";
        check(model.Value == "Native change", "native editor changes update bound model");
        model.Value = "Model change";
        check(field.Text == "Model change", "model notification updates native editor");
        int validations = 0;
        bool reject = true;
        field.Validating += (_, e) => { validations++; e.Cancel = reject; };
        field.Focus(); form.Tab();
        check(field.Editor.Focused && validations == 1, "cancelled wrapper validation prevents leaving native editor");
        reject = false; form.Tab();
        check(next.Focused && validations == 2, "successful wrapper validation allows navigation once");
        next.Validating += (_, e) => e.Cancel = true;
        field.CausesValidation = false;
        field.Focus();
        check(field.Editor.Focused, "CausesValidation=false permits focus without validating preceding control");
        check(!field.Editor.CausesValidation, "CausesValidation propagates to native editor");
        int foregroundEvents = 0;
        field.ForeColorChanged += (_, _) => foregroundEvents++;
        field.Appearance.Normal.ForeColor = Color.Magenta;
        check(field.ForeColor == Color.Magenta && field.Editor.ForeColor == Color.Magenta && foregroundEvents == 1, "nested ForeColor synchronizes native property, editor and event");
        field.ForeColor = Color.Cyan;
        check(field.Appearance.Normal.ForeColor == Color.Cyan && foregroundEvents == 2, "direct ForeColor updates the same style with one event");
        field.Appearance.Hover.ForeColor = Color.Yellow;
        check(foregroundEvents == 2 && field.ForeColor == Color.Cyan, "state overrides do not mutate normal ForeColor");
        form.Close();
    }
    private sealed class ValidationForm : Form { internal bool Tab() => ProcessDialogKey(Keys.Tab); }
    private sealed class TextModel : INotifyPropertyChanged
    {
        private string value = "Initial";
        public string Value
        {
            get => value;
            set { if (this.value == value) return; this.value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))); }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
