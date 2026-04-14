namespace Library.UI.Helpers;

/// <summary>
/// Design system and styling utilities for a modern WinForms look.
/// All methods are static — call them in your Form_Load or constructor.
/// </summary>
public static class UIHelper
{
    // ── Color Palette ──────────────────────────────────────────────
    public static readonly Color SidebarBg       = ColorTranslator.FromHtml("#1B2838");
    public static readonly Color SidebarHover     = ColorTranslator.FromHtml("#2A475E");
    public static readonly Color SidebarActive    = ColorTranslator.FromHtml("#3D6B8E");
    public static readonly Color SidebarText      = ColorTranslator.FromHtml("#C7D5E0");

    public static readonly Color ContentBg        = ColorTranslator.FromHtml("#F0F2F5");
    public static readonly Color CardBg           = Color.White;
    public static readonly Color HeaderBg         = Color.White;

    public static readonly Color Primary          = ColorTranslator.FromHtml("#4A90D9");
    public static readonly Color PrimaryDark      = ColorTranslator.FromHtml("#357ABD");
    public static readonly Color Success          = ColorTranslator.FromHtml("#27AE60");
    public static readonly Color SuccessDark      = ColorTranslator.FromHtml("#1E8449");
    public static readonly Color Danger           = ColorTranslator.FromHtml("#E74C3C");
    public static readonly Color DangerDark       = ColorTranslator.FromHtml("#C0392B");
    public static readonly Color Warning          = ColorTranslator.FromHtml("#F39C12");
    public static readonly Color WarningDark      = ColorTranslator.FromHtml("#D68910");

    public static readonly Color TextPrimary      = ColorTranslator.FromHtml("#2C3E50");
    public static readonly Color TextSecondary    = ColorTranslator.FromHtml("#7F8C8D");
    public static readonly Color TextOnDark       = ColorTranslator.FromHtml("#ECF0F1");

    public static readonly Color Border           = ColorTranslator.FromHtml("#DCE1E8");
    public static readonly Color InputBg          = Color.White;

    public static readonly Color GridHeader        = ColorTranslator.FromHtml("#34495E");
    public static readonly Color GridAltRow        = ColorTranslator.FromHtml("#F8F9FA");
    public static readonly Color GridSelection     = ColorTranslator.FromHtml("#D4E6F9");
    public static readonly Color GridSelectionText = ColorTranslator.FromHtml("#1B2838");

    public static readonly Color OverdueColor      = ColorTranslator.FromHtml("#FDEDEC");

    // ── Fonts ──────────────────────────────────────────────────────
    public static readonly Font FontRegular    = new("Segoe UI", 9.5f, FontStyle.Regular);
    public static readonly Font FontBold       = new("Segoe UI", 9.5f, FontStyle.Bold);
    public static readonly Font FontSmall      = new("Segoe UI", 8.5f, FontStyle.Regular);
    public static readonly Font FontTitle      = new("Segoe UI", 14f, FontStyle.Bold);
    public static readonly Font FontSubtitle   = new("Segoe UI", 11f, FontStyle.Regular);
    public static readonly Font FontSidebar    = new("Segoe UI", 10.5f, FontStyle.Regular);
    public static readonly Font FontHeader     = new("Segoe UI", 10f, FontStyle.Bold);

    // ── DataGridView Styling ───────────────────────────────────────
    public static void StyleDataGridView(DataGridView grid)
    {
        EnableDoubleBuffered(grid);
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.EnableHeadersVisualStyles = false;
        grid.GridColor = Border;
        grid.BackgroundColor = CardBg;

        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = GridHeader,
            ForeColor = Color.White,
            Font = FontBold,
            Padding = new Padding(8, 6, 8, 6),
            SelectionBackColor = GridHeader,
            SelectionForeColor = Color.White
        };
        grid.ColumnHeadersHeight = 40;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = CardBg,
            ForeColor = TextPrimary,
            Font = FontRegular,
            Padding = new Padding(8, 4, 8, 4),
            SelectionBackColor = GridSelection,
            SelectionForeColor = GridSelectionText
        };

        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = GridAltRow,
            ForeColor = TextPrimary,
            Font = FontRegular,
            Padding = new Padding(8, 4, 8, 4),
            SelectionBackColor = GridSelection,
            SelectionForeColor = GridSelectionText
        };

        grid.RowHeadersVisible = false;
        grid.RowTemplate.Height = 36;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
    }

    // ── Button Styling ─────────────────────────────────────────────
    public static void StyleButton(Button btn, Color bgColor, Color textColor)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.BackColor = bgColor;
        btn.ForeColor = textColor;
        btn.Font = FontRegular;
        btn.Cursor = Cursors.Hand;
        btn.Padding = new Padding(12, 6, 12, 6);
        btn.Height = 36;

        Color hoverColor = ControlPaint.Dark(bgColor, 0.1f);
        btn.FlatAppearance.MouseOverBackColor = hoverColor;
        btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(bgColor, 0.2f);
    }

    public static void StylePrimaryButton(Button btn)
        => StyleButton(btn, Primary, Color.White);

    public static void StyleSuccessButton(Button btn)
        => StyleButton(btn, Success, Color.White);

    public static void StyleDangerButton(Button btn)
        => StyleButton(btn, Danger, Color.White);

    public static void StyleWarningButton(Button btn)
        => StyleButton(btn, Warning, Color.White);

    public static void StyleSecondaryButton(Button btn)
        => StyleButton(btn, Border, TextPrimary);

    // ── Sidebar Button ─────────────────────────────────────────────
    public static void StyleSidebarButton(Button btn, bool isActive = false)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.BackColor = isActive ? SidebarActive : SidebarBg;
        btn.ForeColor = SidebarText;
        btn.Font = FontSidebar;
        btn.Cursor = Cursors.Hand;
        btn.TextAlign = ContentAlignment.MiddleLeft;
        btn.Padding = new Padding(20, 0, 0, 0);
        btn.Dock = DockStyle.Top;
        btn.Height = 48;

        btn.FlatAppearance.MouseOverBackColor = SidebarHover;
        btn.FlatAppearance.MouseDownBackColor = SidebarActive;
    }

    // ── TextBox Styling ────────────────────────────────────────────
    public static void StyleTextBox(TextBox txt)
    {
        txt.BorderStyle = BorderStyle.FixedSingle;
        txt.BackColor = InputBg;
        txt.ForeColor = TextPrimary;
        txt.Font = FontRegular;
        txt.Height = 32;
    }

    // ── Panel / Card Styling ───────────────────────────────────────
    public static void StyleCard(Panel panel)
    {
        panel.BackColor = CardBg;
        panel.Padding = new Padding(16);
    }

    // ── Label Styling ──────────────────────────────────────────────
    public static void StyleLabel(Label lbl, Font? font = null, Color? color = null)
    {
        lbl.Font = font ?? FontRegular;
        lbl.ForeColor = color ?? TextPrimary;
        lbl.AutoSize = true;
    }

    public static void StyleTitleLabel(Label lbl)
        => StyleLabel(lbl, FontTitle, TextPrimary);

    public static void StyleSubtitleLabel(Label lbl)
        => StyleLabel(lbl, FontSubtitle, TextSecondary);

    // ── Form Base Styling ──────────────────────────────────────────
    public static void StyleForm(Form form)
    {
        form.BackColor = ContentBg;
        form.Font = FontRegular;
        form.StartPosition = FormStartPosition.CenterScreen;
        EnableDoubleBuffered(form);
    }

    /// <summary>
    /// Enables DoubleBuffered on a control via reflection (needed for Wine flicker prevention).
    /// Also works on Panels/DataGridViews that don't expose the property publicly.
    /// </summary>
    public static void EnableDoubleBuffered(Control control)
    {
        var prop = typeof(Control).GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        prop?.SetValue(control, true);
    }

    // ── Helper: Create a styled labeled field ──────────────────────
    public static (Label label, TextBox textBox) CreateField(string labelText, int y, int labelX = 20, int fieldX = 20, int fieldWidth = 300)
    {
        var label = new Label
        {
            Text = labelText,
            Location = new Point(labelX, y),
            AutoSize = true,
            Font = FontSmall,
            ForeColor = TextSecondary
        };

        var textBox = new TextBox
        {
            Location = new Point(fieldX, y + 18),
            Width = fieldWidth,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = InputBg,
            ForeColor = TextPrimary,
            Font = FontRegular
        };

        return (label, textBox);
    }
}
