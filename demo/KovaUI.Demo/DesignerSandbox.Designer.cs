namespace KovaUI.Demo;

partial class DesignerSandbox
{
    private System.ComponentModel.IContainer components = new System.ComponentModel.Container();
    private Controls.KButton actionButton = null!;
    private Controls.KTextBox nameField = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        actionButton = new KovaUI.Controls.KButton();
        nameField = new KovaUI.Controls.KTextBox();
        kButton1 = new KovaUI.Controls.KButton();
        kTextBox1 = new KovaUI.Controls.KTextBox();
        SuspendLayout();
        // 
        // actionButton
        // 
        actionButton.BackColor = Color.Transparent;
        actionButton.FlatStyle = FlatStyle.Flat;
        actionButton.Location = new Point(27, 32);
        actionButton.Margin = new Padding(3, 4, 3, 4);
        actionButton.Name = "actionButton";
        actionButton.Padding = new Padding(14, 5, 14, 5);
        actionButton.Size = new Size(160, 53);
        actionButton.TabIndex = 0;
        actionButton.Text = "Continuar";
        actionButton.UseVisualStyleBackColor = false;
        // 
        // nameField
        // 
        nameField.BackColor = Color.Transparent;
        nameField.Location = new Point(27, 126);
        nameField.Margin = new Padding(3, 4, 3, 4);
        nameField.Name = "nameField";
        nameField.Padding = new Padding(14, 11, 14, 11);
        nameField.PlaceholderText = "Escribe aquí";
        nameField.Size = new Size(297, 44);
        nameField.TabIndex = 1;
        nameField.TabStop = false;
        // 
        // kButton1
        // 
        kButton1.Animation.Duration = 180;
        kButton1.Animation.Easing = Styling.KEasing.Linear;
        kButton1.Appearance.Normal.BorderColor = Color.Black;
        kButton1.BackColor = Color.Transparent;
        kButton1.BorderColor = Color.Black;
        kButton1.FlatStyle = FlatStyle.Flat;
        kButton1.Location = new Point(117, 301);
        kButton1.Name = "kButton1";
        kButton1.Padding = new Padding(12, 4, 12, 4);
        kButton1.Size = new Size(140, 50);
        kButton1.TabIndex = 2;
        kButton1.Text = "kButton1";
        kButton1.UseVisualStyleBackColor = false;
        // 
        // kTextBox1
        // 
        kTextBox1.BackColor = Color.Transparent;
        kTextBox1.Lines = new string[]
{
    "kTextBox1"
};
        kTextBox1.Location = new Point(183, 202);
        kTextBox1.Name = "kTextBox1";
        kTextBox1.Padding = new Padding(12, 8, 12, 8);
        kTextBox1.Size = new Size(325, 50);
        kTextBox1.TabIndex = 3;
        kTextBox1.TabStop = false;
        kTextBox1.Text = "kTextBox1";
        // 
        // DesignerSandbox
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(595, 496);
        Controls.Add(kTextBox1);
        Controls.Add(kButton1);
        Controls.Add(actionButton);
        Controls.Add(nameField);
        Margin = new Padding(3, 4, 3, 4);
        Name = "DesignerSandbox";
        Text = "KovaUI · Designer sandbox";
        ResumeLayout(false);
    }

    private Controls.KButton kButton1;
    private Controls.KTextBox kTextBox1;
}

