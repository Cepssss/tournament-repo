using TournamentManager.Helpers;
using TournamentManager.Models;

namespace TournamentManager.Forms;

public class GameEntryForm : Form
{
    private readonly Tournament _t;
    private readonly Game? _existing;
    private DataGridView _dgv = null!;
    private Label _lblError = null!;

    public List<GameResult>? Results { get; private set; }

    public GameEntryForm(Tournament tournament, Game? existing = null)
    {
        _t        = tournament;
        _existing = existing;
        Build();
        Populate();
    }

    private void Build()
    {
        int gameNum = _existing?.Id ?? _t.Games.Count + 1;
        Text = _existing != null ? $"Edit Game {_existing.Id}" : $"Add Game {gameNum}";
        Size = new Size(660, 620);
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

        var btnSave = AppColors.Btn("Save Game", AppColors.Accent, 110);
        btnSave.Location = new Point(footer.Width - 120, 11);
        btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSave.Click += (_, _) => TrySave();

        footer.Controls.Add(btnCancel);
        footer.Controls.Add(btnSave);
        footer.Controls.Add(AppColors.HSep());

        // ── Error label ─────────────────────────────────────────────
        _lblError = new Label
        {
            Dock = DockStyle.Top,
            Height = 0,
            BackColor = Color.FromArgb(42, 10, 10),
            ForeColor = Color.FromArgb(255, 144, 144),
            Font = new Font("Segoe UI", 9.5f),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            Visible = false
        };

        // ── DataGridView ─────────────────────────────────────────────
        _dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
        };
        AppColors.StyleDgv(_dgv, editable: true);

        _dgv.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "Team", HeaderText = "Team", ReadOnly = true,
              AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _dgv.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "Position", HeaderText = "Position (1–15)", Width = 130,
              DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
        _dgv.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "Kills", HeaderText = "Kills", Width = 90,
              DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
        _dgv.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "Multiplier", HeaderText = "Multiplier", Width = 100, ReadOnly = true,
              DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter,
                                   ForeColor = AppColors.Muted } });
        _dgv.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "Points", HeaderText = "Points", Width = 90, ReadOnly = true,
              DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight,
                                   Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                                   ForeColor = AppColors.Accent } });

        // Live update while typing
        _dgv.EditingControlShowing += OnEditingControlShowing;

        Controls.Add(_dgv);
        Controls.Add(_lblError);
        Controls.Add(footer);
    }

    private void Populate()
    {
        foreach (var team in _t.Teams)
        {
            var prev = _existing?.Results.FirstOrDefault(r => r.Team == team);
            string pos   = prev != null ? prev.Position.ToString() : "";
            string kills = prev != null ? prev.Kills.ToString() : "";
            string mult  = prev != null ? $"×{ScoreCalculator.GetMultiplier(prev.Position, _t.Multipliers)}" : "—";
            string pts   = prev != null ? ScoreCalculator.Fmt(ScoreCalculator.CalcScore(prev.Kills, prev.Position, _t.Multipliers)) : "—";

            _dgv.Rows.Add(team, pos, kills, mult, pts);
        }
    }

    // ── Live update ─────────────────────────────────────────────────
    private void OnEditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
    {
        if (e.Control is not TextBox tb) return;
        tb.TextChanged -= OnCellTextChanged;
        tb.TextChanged += OnCellTextChanged;
    }

    private void OnCellTextChanged(object? sender, EventArgs e)
    {
        if (_dgv.CurrentCell == null) return;
        int row = _dgv.CurrentCell.RowIndex;
        int col = _dgv.CurrentCell.ColumnIndex;
        if (col != 1 && col != 2) return;

        string posText   = col == 1 ? ((TextBox)sender!).Text
                                    : _dgv.Rows[row].Cells[1].Value?.ToString() ?? "";
        string killsText = col == 2 ? ((TextBox)sender!).Text
                                    : _dgv.Rows[row].Cells[2].Value?.ToString() ?? "";

        if (int.TryParse(posText, out int pos) && pos >= 1 && pos <= 15)
        {
            double mult = ScoreCalculator.GetMultiplier(pos, _t.Multipliers);
            _dgv.Rows[row].Cells[3].Value = $"×{mult}";
            if (int.TryParse(killsText, out int kills) && kills >= 0)
                _dgv.Rows[row].Cells[4].Value = ScoreCalculator.Fmt(ScoreCalculator.CalcScore(kills, pos, _t.Multipliers));
            else
                _dgv.Rows[row].Cells[4].Value = "—";
        }
        else
        {
            _dgv.Rows[row].Cells[3].Value = "—";
            _dgv.Rows[row].Cells[4].Value = "—";
        }
    }

    // ── Validation & Save ────────────────────────────────────────────
    private void TrySave()
    {
        // Commit any pending edit
        _dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
        _dgv.EndEdit();

        HideError();
        ResetCellHighlights();

        var results  = new List<GameResult>();
        bool hasErr  = false;

        foreach (DataGridViewRow row in _dgv.Rows)
        {
            string team      = row.Cells[0].Value?.ToString() ?? "";
            string posText   = row.Cells[1].Value?.ToString() ?? "";
            string killsText = row.Cells[2].Value?.ToString() ?? "";

            bool posOk   = int.TryParse(posText,   out int pos)   && pos   >= 1 && pos   <= 15;
            bool killsOk = int.TryParse(killsText, out int kills) && kills >= 0;

            if (!posOk)   { HighlightCell(row, 1); hasErr = true; }
            if (!killsOk) { HighlightCell(row, 2); hasErr = true; }
            if (posOk && killsOk) results.Add(new GameResult { Team = team, Position = pos, Kills = kills });
        }

        if (hasErr)
        {
            ShowError($"All rows require a valid position (1–15) and kills (≥ 0).");
            return;
        }

        // Duplicate position check
        var dupes = results.GroupBy(r => r.Position).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (dupes.Count > 0)
        {
            foreach (DataGridViewRow row in _dgv.Rows)
                if (int.TryParse(row.Cells[1].Value?.ToString(), out int p) && dupes.Contains(p))
                    HighlightCell(row, 1);
            ShowError($"Position {dupes[0]} is assigned to more than one team.");
            return;
        }

        Results = results;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void ShowError(string msg)
    {
        _lblError.Text    = "  ⚠  " + msg;
        _lblError.Height  = 34;
        _lblError.Visible = true;
    }

    private void HideError()
    {
        _lblError.Visible = false;
        _lblError.Height  = 0;
    }

    private static void HighlightCell(DataGridViewRow row, int col)
    {
        row.Cells[col].Style.BackColor = Color.FromArgb(60, 20, 20);
        row.Cells[col].Style.ForeColor = Color.FromArgb(255, 130, 130);
    }

    private void ResetCellHighlights()
    {
        foreach (DataGridViewRow row in _dgv.Rows)
            foreach (DataGridViewCell cell in row.Cells)
                cell.Style.BackColor = Color.Empty;
    }
}
