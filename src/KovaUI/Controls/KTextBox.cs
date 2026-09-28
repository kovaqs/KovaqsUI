using System.ComponentModel;
using KovaUI.Core;
using KovaUI.Drawing;
using KovaUI.Styling;

namespace KovaUI.Controls;

/// <summary>Configurable frame around an actual WinForms TextBox. Editor exposes the complete native API.</summary>
[ToolboxItem(true), DefaultEvent(nameof(TextChanged)), DefaultProperty(nameof(Text)), DefaultBindingProperty(nameof(Text))]
public class KTextBox : UserControl, IKThemeControl
{
    private KStateTransition? transition;
    private readonly TextBox editor;
    private KTheme theme = KTheme.Light;
    private int radius = KMetrics.RadiusMedium, border = 1, iconSize = 18, iconSpacing = KMetrics.SpaceS;
    private KShape shape;
    private Image? leftIcon, rightIcon;
    private bool hover, pressed;
    private Rectangle leftBounds, rightBounds;
    private Color normalForeColor;
    private Font? defaultFont;
    [TypeConverter(typeof(KFontConverter))]
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override Font Font { get => base.Font; set => base.Font = KFontManager.ResolveFont(value, defaultFont); }
    public override void ResetFont() => Font = null;
    private bool ShouldSerializeFont() => !Equals(Font, defaultFont);
    public KTextBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        base.BackColor = Color.Transparent;
        TabStop = true;
        Size = new Size(260, KMetrics.HeightMedium);
        Padding = new Padding(KMetrics.SpaceM, KMetrics.SpaceS, KMetrics.SpaceM, KMetrics.SpaceS);
        editor = new KNativeTextBox { BorderStyle = BorderStyle.None, AutoSize = true, TabStop = true };
        Controls.Add(editor);
        editor.TextChanged += (_, e) => OnTextChanged(e);
        editor.GotFocus += (_, _) => RefreshStyle();
        editor.LostFocus += (_, _) => SetPressed(false);
        editor.MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) SetPressed(true); };
        editor.MouseUp += (_, e) => { if (e.Button == MouseButtons.Left) SetPressed(false); };
        editor.MouseCaptureChanged += (_, _) => { if (!editor.Capture) SetPressed(false); };
        editor.MouseEnter += (_, _) => { hover = true; RefreshStyle(); };
        editor.MouseLeave += (_, _) => { hover = ClientRectangle.Contains(PointToClient(MousePosition)); RefreshStyle(); };
        editor.KeyDown += (_, e) => OnKeyDown(e);
        editor.KeyUp += (_, e) => OnKeyUp(e);
        editor.KeyPress += (_, e) => OnKeyPress(e);
        editor.ReadOnlyChanged += (_, e) => ReadOnlyChanged?.Invoke(this, e);
        editor.ModifiedChanged += (_, e) => ModifiedChanged?.Invoke(this, e);
        editor.TextAlignChanged += (_, e) => TextAlignChanged?.Invoke(this, e);
        editor.Click += (_, e) => OnClick(e);
        Appearance.Changed += AppearanceChanged;
        FocusAppearance.Changed += AppearanceChanged;
        LayoutEditor(); RefreshStyle();
        base.Font = defaultFont = KTypography.Body;
    }
    private void AppearanceChanged(object? sender, EventArgs e)
    {
        if (normalForeColor != Appearance.Normal.ForeColor)
        {
            normalForeColor = Appearance.Normal.ForeColor;
            base.ForeColor = normalForeColor;
        }
        RefreshStyle();
    }
    public override Color ForeColor
    {
        get => base.ForeColor;
        set { if (Appearance is null) base.ForeColor = value; else Appearance.Normal.ForeColor = value; }
    }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TextBox Editor => editor;
    [Category("Behavior"), DefaultValue(true)]
    public new bool TabStop { get => base.TabStop; set => base.TabStop = value; }
    [Category("Accessibility"), DefaultValue(null)]
    public new string? AccessibleName
    {
        get => base.AccessibleName;
        set { base.AccessibleName = value; if (editor is not null) editor.AccessibleName = value; }
    }
    [Category("Accessibility"), DefaultValue(null)]
    public new string? AccessibleDescription
    {
        get => base.AccessibleDescription;
        set { base.AccessibleDescription = value; if (editor is not null) editor.AccessibleDescription = value; }
    }
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public KAppearance Appearance { get; } = new();
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public KAnimationStyle Animation { get; } = new();
    [Category("Kova Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public KFocusStyle FocusAppearance { get; } = new();
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public KTheme Theme { get => theme; set { theme = value ?? throw new ArgumentNullException(nameof(value)); RefreshStyle(); } }
    [Category("Kova Appearance"), DefaultValue(12)]
    public int CornerRadius { get => radius; set { ArgumentOutOfRangeException.ThrowIfNegative(value); radius = value; Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(1)]
    public int BorderThickness { get => border; set { ArgumentOutOfRangeException.ThrowIfNegative(value); border = value; LayoutEditor(); Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(KShape.Rounded)]
    public KShape Shape { get => shape; set { shape = value; Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(typeof(Color), "")]
    public Color FillColor { get => Appearance.Normal.FillColor; set => Appearance.Normal.FillColor = value; }
    [Category("Kova Appearance"), DefaultValue(typeof(Color), "")]
    public Color BorderColor { get => Appearance.Normal.BorderColor; set => Appearance.Normal.BorderColor = value; }
    [Category("Kova Appearance"), DefaultValue(typeof(Color), "")]
    public Color GradientColor { get => Appearance.Normal.GradientColor; set => Appearance.Normal.GradientColor = value; }
    [Category("Kova Appearance"), DefaultValue(null)]
    public Image? IconLeft { get => leftIcon; set { leftIcon = value; LayoutEditor(); Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(null)]
    public Image? IconRight { get => rightIcon; set { rightIcon = value; LayoutEditor(); Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(18)]
    public int IconSize { get => iconSize; set { ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value); iconSize = value; LayoutEditor(); Invalidate(); } }
    [Category("Kova Appearance"), DefaultValue(8)]
    public int IconSpacing { get => iconSpacing; set { ArgumentOutOfRangeException.ThrowIfNegative(value); iconSpacing = value; LayoutEditor(); Invalidate(); } }
    [Browsable(true), Bindable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue("")]
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string Text
    {
        get => editor is null ? base.Text : editor.Text;
        set { value ??= string.Empty; if (editor is null) { base.Text = value; return; } editor.Text = value; }
    }
    [Category("Behavior"), DefaultValue("")] public string PlaceholderText { get => editor.PlaceholderText; set => editor.PlaceholderText = value; }
    [Category("Behavior"), DefaultValue(false)] public bool UseSystemPasswordChar { get => editor.UseSystemPasswordChar; set => editor.UseSystemPasswordChar = value; }
    [Category("Behavior"), DefaultValue('\0')] public char PasswordChar { get => editor.PasswordChar; set => editor.PasswordChar = value; }
    [Category("Behavior"), DefaultValue(false)] public bool Multiline { get => editor.Multiline; set { editor.Multiline = value; LayoutEditor(); } }
    [Category("Behavior"), DefaultValue(false)] public bool ReadOnly { get => editor.ReadOnly; set { editor.ReadOnly = value; RefreshStyle(); } }
    [Category("Behavior"), DefaultValue(32767)] public int MaxLength { get => editor.MaxLength; set => editor.MaxLength = value; }
    [Category("Behavior"), DefaultValue(true)] public bool ShortcutsEnabled { get => editor.ShortcutsEnabled; set => editor.ShortcutsEnabled = value; }
    [Category("Behavior"), DefaultValue(true)] public bool HideSelection { get => editor.HideSelection; set => editor.HideSelection = value; }
    [Category("Behavior"), DefaultValue(false)] public bool AcceptsReturn { get => editor.AcceptsReturn; set => editor.AcceptsReturn = value; }
    [Category("Behavior"), DefaultValue(false)] public bool AcceptsTab { get => editor.AcceptsTab; set => editor.AcceptsTab = value; }
    [Category("Behavior"), DefaultValue(true)] public bool WordWrap { get => editor.WordWrap; set => editor.WordWrap = value; }
    [Category("Behavior"), DefaultValue(ScrollBars.None)] public ScrollBars ScrollBars { get => editor.ScrollBars; set => editor.ScrollBars = value; }
    [Category("Behavior"), DefaultValue(CharacterCasing.Normal)] public CharacterCasing CharacterCasing { get => editor.CharacterCasing; set => editor.CharacterCasing = value; }
    [Category("Appearance"), DefaultValue(HorizontalAlignment.Left)] public HorizontalAlignment TextAlign { get => editor.TextAlign; set => editor.TextAlign = value; }
    [Category("Behavior"), DefaultValue(AutoCompleteMode.None)] public AutoCompleteMode AutoCompleteMode { get => editor.AutoCompleteMode; set => editor.AutoCompleteMode = value; }
    [Category("Behavior"), DefaultValue(AutoCompleteSource.None)] public AutoCompleteSource AutoCompleteSource { get => editor.AutoCompleteSource; set => editor.AutoCompleteSource = value; }
    [Category("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] public AutoCompleteStringCollection AutoCompleteCustomSource { get => editor.AutoCompleteCustomSource; set => editor.AutoCompleteCustomSource = value; }
    [Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] public string[] Lines { get => editor.Lines; set => editor.Lines = value; }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public int SelectionStart { get => editor.SelectionStart; set => editor.SelectionStart = value; }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public int SelectionLength { get => editor.SelectionLength; set => editor.SelectionLength = value; }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public string SelectedText { get => editor.SelectedText; set => editor.SelectedText = value; }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public bool Modified { get => editor.Modified; set => editor.Modified = value; }
    [Browsable(false)] public bool CanUndo => editor.CanUndo;
    [Browsable(false)] public int TextLength => editor.TextLength;
    [Browsable(false)] public int PreferredHeight => editor.PreferredHeight + Padding.Vertical;
    [Browsable(false)] public override bool Focused => editor?.Focused ?? false;
    public event EventHandler? IconLeftClick;
    public event EventHandler? IconRightClick;
    public event EventHandler? ReadOnlyChanged;
    public event EventHandler? ModifiedChanged;
    public event EventHandler? TextAlignChanged;
    public new bool Focus() => editor.Focus();
    public void Select(int start, int length) => editor.Select(start, length);
    public void SelectAll() => editor.SelectAll();
    public void DeselectAll() => editor.DeselectAll();
    public void AppendText(string? value) => editor.AppendText(value);
    public void Clear() => editor.Clear();
    public void Copy() => editor.Copy();
    public void Cut() => editor.Cut();
    public void Paste() => editor.Paste();
    public void Undo() => editor.Undo();
    public void ClearUndo() => editor.ClearUndo();
    public void ScrollToCaret() => editor.ScrollToCaret();
    private PaintStyle CurrentStyle()
    {
        var style = KRenderer.Resolve(theme, Appearance, KButtonVariant.Outline, true, Enabled, hover, pressed, ContainsFocus);
        return (transition ??= new KStateTransition(this)).Sample(style, Animation);
    }
    private void RefreshStyle()
    {
        if (editor is null) return;
        var style = CurrentStyle();
        // Native EDIT windows require an opaque, uniform background. The frame can still use gradients.
        editor.BackColor = Color.FromArgb(255, style.Fill.R, style.Fill.G, style.Fill.B);
        editor.ForeColor = style.Text;
        Invalidate();
    }
    private void LayoutEditor()
    {
        if (editor is null) return;
        float scale = DeviceDpi / 96f;
        int edge = Math.Max(1, (int)Math.Ceiling(border * scale)), size = (int)(iconSize * scale), gap = (int)(iconSpacing * scale);
        int left = Math.Max(Padding.Left, edge + 2), right = Math.Max(Padding.Right, edge + 2);
        int top = Math.Max(Padding.Top, edge + 2), bottom = Math.Max(Padding.Bottom, edge + 2);
        leftBounds = leftIcon is null ? Rectangle.Empty : new Rectangle(left, (Height - size) / 2, size, size);
        rightBounds = rightIcon is null ? Rectangle.Empty : new Rectangle(Width - right - size, (Height - size) / 2, size, size);
        if (leftIcon is not null) left += size + gap;
        if (rightIcon is not null) right += size + gap;
        int height = editor.Multiline ? Math.Max(1, Height - top - bottom) : editor.PreferredHeight;
        editor.SetBounds(left, editor.Multiline ? top : Math.Max(edge, (Height - height) / 2), Math.Max(1, Width - left - right), height);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        float scale = DeviceDpi / 96f;
        var style = CurrentStyle();
        editor.BackColor = Color.FromArgb(255, style.Fill.R, style.Fill.G, style.Fill.B);
        editor.ForeColor = style.Text;
        KRenderer.Surface(e.Graphics, ClientRectangle, style, radius * scale, border * scale, shape, scale,
            Enabled && ContainsFocus ? FocusAppearance.Resolve(Appearance.Focused.BorderColor, theme.Focus) : Color.Empty,
            FocusAppearance.Thickness, FocusAppearance.Inset);
        if (leftIcon is not null) e.Graphics.DrawImage(leftIcon, leftBounds);
        if (rightIcon is not null) e.Graphics.DrawImage(rightIcon, rightBounds);
        base.OnPaint(e);
    }
    protected override void WndProc(ref Message message)
    {
        if (editor is KNativeTextBox native && native.ApplyMaskColors(ref message)) return;
        base.WndProc(ref message);
    }
    private void SetPressed(bool value) { pressed = value && Enabled; RefreshStyle(); }
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left) { editor.Focus(); SetPressed(true); }
    }
    protected override void OnMouseUp(MouseEventArgs e) { if (e.Button == MouseButtons.Left) SetPressed(false); base.OnMouseUp(e); }
    protected override void OnMouseCaptureChanged(EventArgs e) { if (!Capture) SetPressed(false); base.OnMouseCaptureChanged(e); }
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button != MouseButtons.Left) return;
        if (leftBounds.Contains(e.Location)) IconLeftClick?.Invoke(this, EventArgs.Empty);
        if (rightBounds.Contains(e.Location)) IconRightClick?.Invoke(this, EventArgs.Empty);
    }
    protected override void OnMouseEnter(EventArgs e) { hover = true; base.OnMouseEnter(e); RefreshStyle(); }
    protected override void OnMouseLeave(EventArgs e) { hover = ClientRectangle.Contains(PointToClient(MousePosition)); base.OnMouseLeave(e); RefreshStyle(); }
    protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); LayoutEditor(); }
    protected override void OnPaddingChanged(EventArgs e) { base.OnPaddingChanged(e); LayoutEditor(); }
    protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); if (editor is not null) editor.Font = Font; LayoutEditor(); }
    protected override void OnForeColorChanged(EventArgs e) { base.OnForeColorChanged(e); RefreshStyle(); }
    protected override void OnEnabledChanged(EventArgs e) { pressed = false; base.OnEnabledChanged(e); RefreshStyle(); }
    protected override void OnTabStopChanged(EventArgs e)
    {
        base.OnTabStopChanged(e);
        if (editor is not null) editor.TabStop = TabStop;
    }
    protected override void OnCausesValidationChanged(EventArgs e)
    {
        base.OnCausesValidationChanged(e);
        if (editor is not null) editor.CausesValidation = CausesValidation;
    }
    protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); LayoutEditor(); }
    protected override void OnContextMenuStripChanged(EventArgs e) { base.OnContextMenuStripChanged(e); if (editor is not null) editor.ContextMenuStrip = ContextMenuStrip; }
    protected override void OnEnter(EventArgs e) { base.OnEnter(e); if (editor is not null && !editor.Focused) editor.Focus(); }
    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        editor.AccessibleName = AccessibleName;
        editor.AccessibleDescription = AccessibleDescription;
    }
    public override Size GetPreferredSize(Size proposedSize) => new(Math.Max(120, proposedSize.Width), Math.Max(PreferredHeight, (int)(KMetrics.HeightMedium * DeviceDpi / 96f)));
    protected override void Dispose(bool disposing)
    {
        if (disposing) { transition?.Dispose(); Appearance.Changed -= AppearanceChanged; FocusAppearance.Changed -= AppearanceChanged; }
        base.Dispose(disposing); if (disposing) { defaultFont?.Dispose(); defaultFont = null; }
    }
}
