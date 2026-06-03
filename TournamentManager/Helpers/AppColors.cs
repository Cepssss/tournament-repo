namespace TournamentManager.Helpers;

public static class AppColors
{
    public static readonly Color Background = Color.FromArgb(8,  16, 30);
    public static readonly Color Surface    = Color.FromArgb(15, 26, 45);
    public static readonly Color Surface2   = Color.FromArgb(22, 34, 54);
    public static readonly Color Border     = Color.FromArgb(30, 48, 80);
    public static readonly Color Accent     = Color.FromArgb(245, 166, 35);
    public static readonly Color Text       = Color.FromArgb(232, 240, 254);
    public static readonly Color Muted      = Color.FromArgb(122, 144, 176);
    public static readonly Color Danger     = Color.FromArgb(231, 76,  60);
    public static readonly Color Success    = Color.FromArgb(46,  204, 113);
    public static readonly Color DarkRow    = Color.FromArgb(12,  22, 38);

    public static Button Btn(string text, Color back, int width = 110)
    {
        var b = new Button
        {
            Text = text,
            BackColor = back,
            ForeColor = back == Accent ? Color.FromArgb(26, 15, 0) : Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Height = 32,
            Width = width,
            UseVisualStyleBackColor = false,
            Cursor = Cursors.Hand,
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    public static void StyleDgv(DataGridView g, bool editable = false)
    {
        g.BackgroundColor = DarkRow;
        g.DefaultCellStyle.BackColor = Surface2;
        g.DefaultCellStyle.ForeColor = Text;
        g.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 70, 120);
        g.DefaultCellStyle.SelectionForeColor = Text;
        g.AlternatingRowsDefaultCellStyle.BackColor = DarkRow;
        g.ColumnHeadersDefaultCellStyle.BackColor = Surface;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Muted;
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        g.EnableHeadersVisualStyles = false;
        g.GridColor = Border;
        g.BorderStyle = BorderStyle.None;
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        g.RowHeadersVisible = false;
        g.AllowUserToResizeRows = false;
        g.AllowUserToAddRows = false;
        g.ReadOnly = !editable;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.MultiSelect = false;
    }

    public static Label Lbl(string text, float size = 9.5f,
        FontStyle style = FontStyle.Regular, Color? color = null)
        => new()
        {
            Text = text,
            Font = new Font("Segoe UI", size, style),
            ForeColor = color ?? Text,
            BackColor = Color.Transparent,
            AutoSize = true,
        };

    public static Panel HSep() => new()
    {
        Dock = DockStyle.Top,
        Height = 1,
        BackColor = Border
    };
}
