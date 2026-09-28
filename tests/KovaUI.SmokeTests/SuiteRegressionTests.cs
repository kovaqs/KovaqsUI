using KovaUI.Controls;
using KovaUI.Styling;

internal static class SuiteRegressionTests
{
    private sealed class CheckProbe : KCheckBox { internal void ClickNative() => OnClick(EventArgs.Empty); }
    internal static void Run(Action<bool, string> check)
    {
        using (var label = new KLabel { Text = "Título", TextRole = KTextRole.Title })
        {
            var properties = System.ComponentModel.TypeDescriptor.GetProperties(label);
            check(label.AutoSize && label.Font.Bold && !properties["Font"]!.ShouldSerializeValue(label)
                && !properties["ForeColor"]!.ShouldSerializeValue(label), "label role defaults do not serialize as custom overrides");
            using var customFont = new Font("Arial", 14);
            label.Font = customFont; label.ForeColor = Color.Coral;
            label.Theme = KTheme.Dark; label.TextRole = KTextRole.Caption;
            check(label.Font == customFont && label.ForeColor == Color.Coral, "label preserves explicit typography and color across theme and role changes");
            properties["Font"]!.ResetValue(label); properties["ForeColor"]!.ResetValue(label);
            check(label.Font.Size == 9 && label.ForeColor == KTheme.Dark.TextSecondary,
                "label designer resets recover role typography and theme color");
            using var labelHost = new Form(); labelHost.Controls.Add(label); labelHost.Show(); Application.DoEvents();
            int width = label.Width; label.Text = "Texto bastante más largo para comprobar AutoSize";
            check(label.Width > width, "label retains native automatic text sizing");
        }
        using var form = new Form { Size = new Size(700, 420) };
        using var panel = new KPanel { Size = new Size(640, 360), Dock = DockStyle.Fill };
        form.Controls.Add(panel);
        using var checkbox = new CheckProbe { ThreeState = true, Text = "Unicode ✓", Location = new Point(25, 25) };
        using var radio1 = new KRadioButton { Text = "A", Location = new Point(25, 70) };
        using var radio2 = new KRadioButton { Text = "B", Location = new Point(230, 70) };
        using var combo = new KComboBox { Location = new Point(25, 120) };
        combo.ForeColor = Color.Purple; combo.BackColor = Color.Beige;
        combo.ItemAppearance.BorderColor = Color.Red; combo.Theme = KTheme.Dark;
        check(combo.ForeColor == Color.Purple && combo.BackColor == Color.Beige
            && combo.Appearance.Normal.ForeColor == Color.Purple && combo.FillColor == Color.Beige,
            "combo preserves native color overrides when styles and theme change");
        combo.Appearance.Normal.ForeColor = Color.Navy; combo.FillColor = Color.Ivory;
        check(combo.ForeColor == Color.Navy && combo.BackColor == Color.Ivory,
            "combo synchronizes appearance colors with native properties");
        var comboProperties = System.ComponentModel.TypeDescriptor.GetProperties(combo);
        comboProperties["ForeColor"]!.ResetValue(combo); comboProperties["BackColor"]!.ResetValue(combo);
        check(combo.ForeColor == KTheme.Dark.TextPrimary && combo.BackColor == KTheme.Dark.Surface
            && !comboProperties["ForeColor"]!.ShouldSerializeValue(combo) && !comboProperties["BackColor"]!.ShouldSerializeValue(combo),
            "combo designer color reset restores theme inheritance");
        using var progress = new KProgressBar { Location = new Point(25, 180), Size = new Size(250, 18) };
        panel.Controls.AddRange([checkbox, radio1, radio2, combo, progress]);
        form.Show(); Application.DoEvents();
        int changes = 0; checkbox.CheckStateChanged += (_, _) => changes++;
        checkbox.ClickNative(); checkbox.ClickNative();
        check(checkbox.CheckState == CheckState.Indeterminate && changes == 2, "checkbox keeps native three-state cycle and events");
        checkbox.ClickNative(); check(checkbox.CheckState == CheckState.Unchecked, "checkbox completes native state cycle");
        radio1.Checked = true; radio2.Checked = true;
        check(!radio1.Checked && radio2.Checked, "radio buttons retain exclusive native grouping");
        combo.DisplayMember = "Name"; combo.ValueMember = "Id";
        combo.DataSource = new[] { new { Name = "Uno", Id = 1 }, new { Name = "Dos", Id = 2 } };
        combo.SelectedValue = 2;
        check(combo.Text == "Dos" && combo.SelectedIndex == 1, "combo preserves native data binding and selected value");
        combo.DataSource = null; combo.Items.Clear(); combo.DropDownStyle = ComboBoxStyle.DropDown; combo.Text = "Editable";
        check(combo.Text == "Editable", "combo retains native editable mode");
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        ProgressBar native = progress; native.Minimum = 10; native.Maximum = 80; native.Value = 20; native.Step = 15; native.PerformStep();
        check(progress.Value == 35, "progress preserves native range and step through base reference");
        native.Increment(100); check(progress.Value == 80, "progress clamps native increment to maximum");
        panel.Shadow.Enabled = true; panel.Shadow.Blur = 12;
        using var docked = new Label { Dock = DockStyle.Bottom, Height = 20 }; panel.Controls.Add(docked); panel.PerformLayout();
        check(docked.Left >= panel.Padding.Left + 12 && docked.Right < panel.Width - 12, "panel docking reserves shadow and padding");
        var custom = KTheme.Dark with { Primary = Color.Teal };
        KThemeManager.Apply(form, custom);
        check(radio1.Theme == custom && combo.Theme == custom && progress.Theme == custom, "themes propagate through custom panels");
        checkbox.CheckedState.GradientColor = Color.Coral; checkbox.Checked = true;
        progress.Fill.GradientColor = Color.Coral; panel.GradientColor = Color.FromArgb(50,60,70);
        using var bitmap = new Bitmap(form.Width, form.Height); form.DrawToBitmap(bitmap, form.ClientRectangle);
        check(true, "complete suite renders with custom gradients and shadow");
        using (var backdrop = new KPanel { Size = new Size(280, 100), FillColor = Color.Red, GradientColor = Color.Blue })
        using (var transparent = new KCheckBox { Location = new Point(20, 15), Size = new Size(230, 70), Text = "" })
        using (var composite = new Bitmap(280, 100))
        {
            backdrop.Surface.GradientAngle = 90;
            backdrop.Controls.Add(transparent);
            backdrop.DrawToBitmap(composite, backdrop.ClientRectangle);
            var inside = composite.GetPixel(180, 45);
            var outside = composite.GetPixel(265, 45);
            check(Math.Abs(inside.R - outside.R) < 5 && Math.Abs(inside.B - outside.B) < 5,
                "transparent choice preserves its parent's gradient surface");
        }
        using (var aligned = new KCheckBox { Size = new Size(100, 100), Checked = true, Text = "", Padding = new Padding(4) })
        {
            aligned.CheckedState.FillColor = Color.Magenta;
            aligned.CheckedState.BorderColor = Color.Magenta;
            panel.Controls.Add(aligned);
            using var positionImage = new Bitmap(100, 100);
            aligned.CheckAlign = ContentAlignment.BottomRight;
            aligned.DrawToBitmap(positionImage, aligned.ClientRectangle);
            check(positionImage.GetPixel(80, 80).R > 200 && positionImage.GetPixel(80, 80).B > 200,
                "checkbox respects bottom-right glyph alignment");
            aligned.CheckAlign = ContentAlignment.TopCenter;
            aligned.DrawToBitmap(positionImage, aligned.ClientRectangle);
            check(positionImage.GetPixel(45, 10).R > 200 && positionImage.GetPixel(45, 10).B > 200,
                "checkbox respects top-center glyph alignment");
            aligned.Appearance = Appearance.Button;
            aligned.DrawToBitmap(positionImage, aligned.ClientRectangle);
            check(positionImage.GetPixel(60, 60).ToArgb() == Color.Magenta.ToArgb(),
                "checkbox button appearance fills the full selected surface");
            using var icon = new Bitmap(8, 8);
            using (var graphics = Graphics.FromImage(icon)) graphics.Clear(Color.Lime);
            aligned.Image = icon;
            aligned.ImageAlign = ContentAlignment.BottomRight;
            aligned.DrawToBitmap(positionImage, aligned.ClientRectangle);
            check(positionImage.GetPixel(92, 92).ToArgb() == Color.Lime.ToArgb(),
                "choice control preserves native image and alignment properties");
            aligned.Image = null;
            foreach (var nativeStyle in Enum.GetValues<FlatStyle>())
            {
                ((CheckBox)aligned).FlatStyle = nativeStyle;
                aligned.DrawToBitmap(positionImage, aligned.ClientRectangle);
                check(positionImage.GetPixel(60, 60).ToArgb() == Color.Magenta.ToArgb(),
                    $"choice styling survives inherited FlatStyle={nativeStyle}");
            }
        }
        checkbox.AppearanceStates.Disabled.ForeColor = Color.Magenta;
        checkbox.Enabled = false;
        using (var choiceImage = new Bitmap(checkbox.Width, checkbox.Height))
        {
            checkbox.DrawToBitmap(choiceImage, checkbox.ClientRectangle);
            int tinted = 0;
            for (int y = 0; y < choiceImage.Height; y++)
                for (int x = 45; x < choiceImage.Width; x++)
                {
                    var pixel = choiceImage.GetPixel(x, y);
                    if (pixel.R > 170 && pixel.B > 170 && pixel.G < 100) tinted++;
                }
            check(tinted > 5, "choice label honors custom disabled text color");
        }
        foreach (Control choice in new Control[] { checkbox, radio1 })
        {
            PaintEventHandler overlay = (_, e) => e.Graphics.FillRectangle(Brushes.Lime, 0, 0, 8, 8);
            choice.Paint += overlay;
            using var painted = new Bitmap(choice.Width, choice.Height);
            choice.DrawToBitmap(painted, choice.ClientRectangle);
            check(painted.GetPixel(3, 3).ToArgb() == Color.Lime.ToArgb(),
                $"{choice.GetType().Name} preserves consumer Paint overlays through Control reference");
            choice.Paint -= overlay;
        }
        foreach (Control control in new Control[] { checkbox, radio1, combo, progress, panel })
        {
            control.Size = new Size(1, 1); control.DrawToBitmap(bitmap, new Rectangle(0,0,Math.Max(1,control.Width),Math.Max(1,control.Height)));
        }
        check(true, "new controls tolerate small bounds");
        using var button = new KButton();
        check(button.Cursor == Cursors.Hand && checkbox.Cursor == Cursors.Hand && radio1.Cursor == Cursors.Hand, "interactive buttons default to hand cursor");
        form.Close();
    }
}


