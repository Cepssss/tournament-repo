using TournamentManager.Helpers;

namespace TournamentManager.Forms;

public class MultipliersForm : Form
{
    private readonly NumericUpDown[] _inputs = new NumericUpDown[15];

    public List<double> Result { get; private set; } = new();

    public MultipliersForm(List<double> current)
    {
        Build(current);
    }

    private void Build(List<double> current)
    {
        Text = "Edit Multipliers";
        Size = new Size(340, 610);
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

        var btnReset = AppColors.Btn("Reset Defaults", AppColors.Surface2, 130);
        btnReset.ForeColor = AppColors.Muted;
        btnReset.Location = new Point(12, 11);
        btnReset.Click += (_, _) => ResetToDefaults();

        var btnCancel = AppColors.Btn("Cancel", AppColors.Surface2, 80);
        btnCancel.ForeColor = AppColors.Muted;
        btnCancel.Location = new Point(footer.Width - 192, 11);
        btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        var btnSave = AppColors.Btn("Save", AppColors.Accent, 90);
        btnSave.Location = new Point(footer.Width - 104, 11);
        btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSave.Click += (_, _) => Save();

        footer.Controls.Add(btnReset);
        footer.Controls.Add(btnCancel);
        footer.Controls.Add(btnSave);
        footer.Controls.Add(AppColors.HSep());

        // ── Body ────────────────────────────────────────────────────
        var body = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 14, 16, 8),
            BackColor = AppColors.Surface,
            AutoScroll = false
        };

        var note = new Label
        {
            Text = "Score = kills × multiplier   •   changes recalculate all existing games",
            Location = new Point(16, 14),
            Font = new Font("Segoe UI", 8f),
            ForeColor = AppColors.Muted,
            AutoSize = true
        };
        body.Controls.Add(note);

        // Column headers
        var hPos  = new Label { Text = "Position",   Location = new Point(16, 40),  Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = AppColors.Muted, AutoSize = true };
        var hMult = new Label { Text = "Multiplier", Location = new Point(200, 40), Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = AppColors.Muted, AutoSize = true };
        body.Controls.Add(hPos);
        body.Controls.Add(hMult);

        var sep = new Panel { Location = new Point(12, 60), Width = 290, Height = 1, BackColor = AppColors.Border };
        body.Controls.Add(sep);

        // 15 rows
        string[] ordinals = { "1st","2nd","3rd","4th","5th","6th","7th","8th","9th","10th","11th","12th","13th","14th","15th" };

        for (int i = 0; i < 15; i++)
        {
            int y = 70 + i * 30;

            var lbl = new Label
            {
                Text = ordinals[i],
                Location = new Point(16, y + 4),
                Font = new Font("Segoe UI", 10),
                ForeColor = AppColors.Text,
                Width = 60,
                AutoSize = false
            };

            var nud = new NumericUpDown
            {
                Location = new Point(192, y),
                Width = 90,
                Minimum = 0.0m,
                Maximum = 99.9m,
                DecimalPlaces = 1,
                Increment = 0.1m,
                Value = (decimal)(i < current.Count ? current[i] : 1.0),
                BackColor = AppColors.Surface2,
                ForeColor = AppColors.Text,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Center
            };
            // Remove the ugly white border from the up/down buttons
            nud.Controls[0].BackColor = AppColors.Surface2;

            _inputs[i] = nud;
            body.Controls.Add(lbl);
            body.Controls.Add(nud);
        }

        Controls.Add(body);
        Controls.Add(footer);
    }

    private void ResetToDefaults()
    {
        for (int i = 0; i < 15; i++)
            _inputs[i].Value = (decimal)ScoreCalculator.DefaultMultipliers[i];
    }

    private void Save()
    {
        Result = _inputs.Select(n => (double)n.Value).ToList();
        DialogResult = DialogResult.OK;
        Close();
    }
}
