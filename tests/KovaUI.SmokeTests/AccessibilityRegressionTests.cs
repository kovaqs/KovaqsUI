using KovaUI.Controls;
using System.Runtime.InteropServices;

internal static class AccessibilityRegressionTests
{
    [DllImport("oleacc.dll")]
    private static extern int AccessibleObjectFromWindow(IntPtr hwnd, uint objectId, ref Guid interfaceId,
        [MarshalAs(UnmanagedType.Interface)] out object accessible);
    internal static void Run(Action<bool, string> check)
    {
        using var form = new Form();
        using var button = new KButton { Text = "&Save", AccessibleName = "Save record" };
        using var field = new KTextBox { Top = 60, AccessibleName = "Initial name", AccessibleDescription = "Initial description" };
        form.Controls.AddRange([button, field]); form.Show(); Application.DoEvents();
        check(button.AccessibilityObject.Role == AccessibleRole.PushButton, "button exposes native accessible button role");
        int clicks = 0; button.Click += (_, _) => clicks++;
        button.AccessibilityObject.DoDefaultAction();
        check(clicks == 1, "accessible default button action invokes Click");
        check(field.Editor.AccessibilityObject.Role == AccessibleRole.Text, "editor exposes native accessible text role");
        field.AccessibleName = "Immediate update";
        check(field.Editor.AccessibilityObject.Name == "Immediate update", "typed accessibility metadata update immediately reaches existing native provider");
        ((Control)field).AccessibleName = "Updated name";
        ((Control)field).AccessibleDescription = "Updated description";
        var iid = new Guid("618736e0-3c3d-11cf-810c-00aa00389b71");
        int hr = AccessibleObjectFromWindow(field.Editor.Handle, 0xFFFFFFFC, ref iid, out var accessible);
        check(hr == 0 && accessible is not null, "Windows retrieves the native editor accessibility provider");
        check(field.Editor.AccessibilityObject.Name == "Updated name" && field.Editor.AccessibilityObject.Description == "Updated description",
            "native accessibility observes wrapper metadata changes after creation");
        if (accessible is not null && Marshal.IsComObject(accessible)) Marshal.ReleaseComObject(accessible);
        field.Enabled = false;
        check((field.Editor.AccessibilityObject.State & AccessibleStates.Unavailable) != 0, "disabled editor advertises unavailable accessibility state");
        form.Close();
    }
}
