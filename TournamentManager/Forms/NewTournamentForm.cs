using TournamentManager.Helpers;
using TournamentManager.Models;

namespace TournamentManager.Forms;

public class NewTournamentForm : Form
{
    private TextBox _tbName = null!;
    private readonly TextBox[] _tbTeams = new TextBox[15];

    public Tournament? Result { get; private set; }

    public NewTournamentForm()
    {
        Build();
    }

    private void Build()
    {
        Text = "New Tournament";
        Size = new Size(520, 600);
        MinimumSize = Size;
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppColors.Surface;
        ForeColor = AppColors.Text;

        // ── Footer ──────────────────────────────────────────────────
        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 54,
            BackColor = AppColors.Surface
        };

        var btnCancel = AppColors.Btn("Cancel", AppColors.Surface2, 90);
        btnCancel.ForeColor = AppColors.Muted;
        btnCancel.Location = new Point(footer.Width - 218, 11);
        btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        var btnCreate = AppColors.Btn("Create", AppColors.Accent, 110);
        btnCreate.Location = new Point(footer.Width - 120, 11);
        btnCreate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCreate.Click += (_, _) => TrySave();

        footer.Controls.Add(btnCancel);
        footer.Controls.Add(btnCreate);
        footer.Controls.Add(AppColors.HSep());

        // ── Scrollable body ─────────────────────────────────────────
        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = AppColors.Surface
        };

        int y = 20;
        const int lx = 20;
        const int fw = 460;

        // Name field
        AddSectionLabel(scroll, "TOURNAMENT NAME", lx, ref y);

        _tbName = new TextBox
        {
            Location = new Point(lx, y),
            Width = fw,
            BackColor = AppColors.Surface2,
            ForeColor = AppColors.Text,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 11),
            MaxLength = 60
        };
        _tbName.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) TrySave(); };
        scroll.Controls.Add(_tbName);
        y += 44;

        // Teams
        AddSectionLabel(scroll, "TEAMS  (leave blank for default names)", lx, ref y);

        for (int i = 0; i < 15; i++)
        {
            int col = i % 2;
            int row = i / 2;
            var tb = new TextBox
            {
                Location = new Point(lx + col * 238, y + row * 38),
                Width = 222,
                BackColor = AppColors.Surface2,
                ForeColor = AppColors.Text,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                MaxLength = 40
            };
            // Placeholder text via hint label
            var hint = new Label
            {
                Text = $"Team {i + 1}",
                Location = new Point(lx + col * 238 + 3, y + row * 38 + 3),
                ForeColor = AppColors.Muted,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            int idx = i;
            tb.Enter += (_, _) => hint.Visible = false;
            tb.Leave += (_, _) => hint.Visible = string.IsNullOrEmpty(tb.Text);
            _tbTeams[idx] = tb;
            scroll.Controls.Add(hint);
            scroll.Controls.Add(tb);
        }

        Controls.Add(scroll);
        Controls.Add(footer);
        ActiveControl = _tbName;
    }

    private static void AddSectionLabel(Panel parent, string text, int x, ref int y)
    {
        var lbl = new Label
        {
            Text = text,
            Location = new Point(x, y),
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = AppColors.Muted,
            AutoSize = true,
            BackColor = Color.Transparent
        };
        parent.Controls.Add(lbl);
        y += 24;
    }

    private void TrySave()
    {
        string name = _tbName.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show("Please enter a tournament name.", "Required",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _tbName.Focus();
            return;
        }

        var teams = _tbTeams
            .Select((tb, i) => string.IsNullOrWhiteSpace(tb.Text) ? $"Team {i + 1}" : tb.Text.Trim())
            .ToList();

        Result = new Tournament { Name = name, Teams = teams };
        DialogResult = DialogResult.OK;
        Close();
    }
}
