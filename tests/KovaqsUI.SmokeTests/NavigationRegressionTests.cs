using KovaqsUI.Controls;

internal static class NavigationRegressionTests
{
    internal static void Run(Action<bool, string> check)
    {
        using var form = new NavigationForm();
        using var before = new Button { Text = "Before", TabIndex = 0 };
        using var field = new KTextBox { TabIndex = 1, Top = 50 };
        using var after = new Button { Text = "After", TabIndex = 2, Top = 100 };
        form.Controls.AddRange([before, field, after]);
        form.Show(); Application.DoEvents();
        before.Focus();
        form.Tab();
        check(field.Editor.Focused, "Tab enters the native text editor");
        form.Tab();
        check(after.Focused, "text wrapper adds no duplicate Tab stop");
        form.Tab(backwards: true);
        check(field.Editor.Focused, "Shift+Tab returns to the editor");
        field.TabStop = false;
        before.Focus();
        form.Tab();
        check(after.Focused, "TabStop=false skips the entire text control");
        field.TabStop = true;
        before.Focus();
        form.Tab();
        check(field.Editor.Focused, "TabStop=true restores editor navigation");
        form.Close();
    }
    private sealed class NavigationForm : Form
    {
        internal bool Tab(bool backwards = false) => ProcessDialogKey(Keys.Tab | (backwards ? Keys.Shift : Keys.None));
    }
}

