using TournamentManager.Helpers;
using TournamentManager.Models;
using TournamentManager.Services;

namespace TournamentManager.Forms;

public class TournamentForm : Form
{
    private readonly Tournament _t;
    private readonly DataService _ds;

    private Label _lblProgress = null!;
    private Button _btnAdd = null!;
    private Panel _banner = null!;
    private Label _bannerLbl = null!;
    private DataGridView _standingsDgv = null!;
    private ListView _gamesLv = null!;

    public TournamentForm(Tournament tournament, DataService ds)
    {
        _t  = tournament;
        _ds = ds;
        Build();
        RefreshView();
    }

    // ── Layout ──────────────────────────────────────────────────────
    private void Build()
    {
        Text = _t.Name;
        Size = new Size(1100, 660);
        MinimumSize = new Size(800, 500);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppColors.Background;
        ForeColor = AppColors.Text;

        // Header
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 54,
            BackColor = AppColors.Surface
        };

        var btnBack = AppColors.Btn("← Back", AppColors.Surface2, 90);
        btnBack.Dock = DockStyle.Left;
        btnBack.ForeColor = AppColors.Text;
        btnBack.Margin = new Padding(16, 11, 0, 11);
        btnBack.Click += (_, _) => Close();

        var titleLbl = new Label
        {
            Text = _t.Name,
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = AppColors.Text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0)
        };

        var btnDelT = AppColors.Btn("Delete", AppColors.Danger, 85);
        btnDelT.Dock = DockStyle.Right;
        btnDelT.Margin = new Padding(0, 11, 16, 11);
        btnDelT.Click += (_, _) => DeleteTournament();

        var btnMult = AppColors.Btn("Multipliers", AppColors.Surface2, 105);
        btnMult.Dock = DockStyle.Right;
        btnMult.ForeColor = AppColors.Text;
        btnMult.Margin = new Padding(0, 11, 8, 11);
        btnMult.Click += (_, _) => EditMultipliers();

        _btnAdd = AppColors.Btn("+ Add Game", AppColors.Accent, 115);
        _btnAdd.Dock = DockStyle.Right;
        _btnAdd.Margin = new Padding(0, 11, 8, 11);
        _btnAdd.Click += (_, _) => AddGame();

        _lblProgress = new Label
        {
            Text = "0 / 5 games",
            Font = new Font("Segoe UI", 9),
            ForeColor = AppColors.Muted,
            Dock = DockStyle.Right,
            TextAlign = ContentAlignment.MiddleRight,
            AutoSize = false,
            Width = 100,
            Padding = new Padding(0, 0, 4, 0)
        };

        header.Controls.Add(titleLbl);
        header.Controls.Add(btnBack);
        header.Controls.Add(btnDelT);
        header.Controls.Add(btnMult);
        header.Controls.Add(_btnAdd);
        header.Controls.Add(_lblProgress);

        // Complete banner (hidden until 5 games done)
        _banner = new Panel
        {
            Dock = DockStyle.Top,
            Height = 38,
            BackColor = Color.FromArgb(13, 32, 0),
            Visible = false
        };
        _bannerLbl = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(130, 240, 60),
            BackColor = Color.Transparent
        };
        _banner.Controls.Add(_bannerLbl);

        // Split
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            BackColor = AppColors.Background
        };
        // Panel min sizes and distance must be set after the control has a real width
        Load += (_, _) =>
        {
            split.Panel1MinSize = 350;
            split.Panel2MinSize = 250;
            int available = split.Width - split.SplitterWidth;
            split.SplitterDistance = Math.Max(350,
                Math.Min(available - 250, (int)(available * 0.62)));
        };

        BuildStandingsPanel(split.Panel1);
        BuildGamesPanel(split.Panel2);

        Controls.Add(split);
        Controls.Add(_banner);
        Controls.Add(AppColors.HSep());
        Controls.Add(header);
    }

    private void BuildStandingsPanel(SplitterPanel panel)
    {
        panel.BackColor = AppColors.Background;
        panel.Padding = new Padding(16, 12, 8, 16);

        var hdr = new Label
        {
            Text = "STANDINGS",
            Dock = DockStyle.Top,
            Height = 24,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = AppColors.Muted,
            BackColor = Color.Transparent
        };

        _standingsDgv = new DataGridView { Dock = DockStyle.Fill };
        AppColors.StyleDgv(_standingsDgv);

        panel.Controls.Add(_standingsDgv);
        panel.Controls.Add(hdr);
    }

    private void BuildGamesPanel(SplitterPanel panel)
    {
        panel.BackColor = AppColors.Background;
        panel.Padding = new Padding(8, 12, 16, 16);

        var hdr = new Label
        {
            Text = "GAMES",
            Dock = DockStyle.Top,
            Height = 24,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = AppColors.Muted,
            BackColor = Color.Transparent
        };

        _gamesLv = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = false,
            BorderStyle = BorderStyle.None,
            BackColor = AppColors.Background,
            ForeColor = AppColors.Text,
            Font = new Font("Segoe UI", 9.5f),
            MultiSelect = false,
            HideSelection = false,
            HeaderStyle = ColumnHeaderStyle.Nonclickable
        };
        _gamesLv.Columns.Add("Game",  68, HorizontalAlignment.Center);
        _gamesLv.Columns.Add("Leader", -2);
        _gamesLv.Columns.Add("Points", 72, HorizontalAlignment.Right);
        _gamesLv.DoubleClick += (_, _) => EditSelectedGame();

        // Buttons row
        var btns = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            BackColor = AppColors.Background
        };
        var btnEdit = AppColors.Btn("Edit", AppColors.Surface2, 90);
        btnEdit.Location = new Point(0, 6);
        btnEdit.ForeColor = AppColors.Text;
        btnEdit.Click += (_, _) => EditSelectedGame();

        var btnDel = AppColors.Btn("Delete", AppColors.Danger, 90);
        btnDel.Location = new Point(98, 6);
        btnDel.Click += (_, _) => DeleteSelectedGame();

        btns.Controls.Add(btnEdit);
        btns.Controls.Add(btnDel);

        panel.Controls.Add(_gamesLv);
        panel.Controls.Add(hdr);
        panel.Controls.Add(btns);
    }

    // ── Refresh ─────────────────────────────────────────────────────
    private void RefreshView()
    {
        int played = _t.Games.Count;
        bool done  = played >= 5;

        _lblProgress.Text   = $"{played} / 5 games";
        _btnAdd.Enabled     = !done;
        _banner.Visible     = done;

        if (done)
        {
            var top = ScoreCalculator.CalcStandings(_t).FirstOrDefault()?.Team ?? "";
            _bannerLbl.Text = $"🏆  Tournament Complete — Winner: {top}";
        }

        RebuildStandings(played);
        RebuildGames();
    }

    private void RebuildStandings(int played)
    {
        _standingsDgv.Columns.Clear();
        _standingsDgv.Rows.Clear();

        _standingsDgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "#",     Width = 42 });
        _standingsDgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Team",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        for (int i = 1; i <= played; i++)
            _standingsDgv.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = $"G{i}", Width = 60, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
        _standingsDgv.Columns.Add(new DataGridViewTextBoxColumn
            { HeaderText = "Total", Width = 72,
              DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });

        var rows = ScoreCalculator.CalcStandings(_t);
        for (int ri = 0; ri < rows.Count; ri++)
        {
            var s    = rows[ri];
            int rank = ri + 1;
            string medal = rank == 1 ? "🥇" : rank == 2 ? "🥈" : rank == 3 ? "🥉" : rank.ToString();

            var cells = new object[2 + played + 1];
            cells[0] = medal;
            cells[1] = s.Team;
            for (int g = 0; g < played; g++)
                cells[2 + g] = s.GameScores.Count > g && s.GameScores[g].HasValue
                    ? ScoreCalculator.Fmt(s.GameScores[g]!.Value) : "—";
            cells[2 + played] = ScoreCalculator.Fmt(s.Total);

            _standingsDgv.Rows.Add(cells);
            var totalCell = _standingsDgv.Rows[ri].Cells[2 + played];
            totalCell.Style.ForeColor = AppColors.Accent;
            totalCell.Style.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        }
    }

    private void RebuildGames()
    {
        _gamesLv.Items.Clear();
        if (_t.Games.Count == 0)
        {
            _gamesLv.Items.Add(new ListViewItem("No games yet")
                { ForeColor = AppColors.Muted });
            return;
        }
        foreach (var g in _t.Games)
        {
            var best = g.Results
                .OrderByDescending(r => ScoreCalculator.CalcScore(r.Kills, r.Position))
                .FirstOrDefault();
            string leader = best != null ? $"#{best.Position}  {best.Team}" : "";
            double bPts   = best != null ? ScoreCalculator.CalcScore(best.Kills, best.Position) : 0;

            _gamesLv.Items.Add(new ListViewItem(new[]
            {
                $"Game {g.Id}", leader, ScoreCalculator.Fmt(bPts)
            })
            { Tag = g, ForeColor = AppColors.Text, BackColor = AppColors.Background });
        }
    }

    // ── Actions ─────────────────────────────────────────────────────
    private void EditMultipliers()
    {
        using var form = new MultipliersForm(_t.Multipliers);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        _t.Multipliers = form.Result;
        Persist();
    }

    private void AddGame()
    {
        if (_t.Games.Count >= 5) return;
        using var form = new GameEntryForm(_t);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        _t.Games.Add(new Game { Id = _t.Games.Count + 1, Results = form.Results! });
        Persist();
    }

    private void EditSelectedGame()
    {
        if (_gamesLv.SelectedItems.Count == 0 || _gamesLv.SelectedItems[0].Tag is not Game game) return;
        using var form = new GameEntryForm(_t, game);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        game.Results = form.Results!;
        Persist();
    }

    private void DeleteSelectedGame()
    {
        if (_gamesLv.SelectedItems.Count == 0 || _gamesLv.SelectedItems[0].Tag is not Game game) return;
        if (MessageBox.Show($"Delete Game {game.Id}? This cannot be undone.", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        _t.Games.Remove(game);
        for (int i = 0; i < _t.Games.Count; i++) _t.Games[i].Id = i + 1;
        Persist();
    }

    private void DeleteTournament()
    {
        if (MessageBox.Show($"Delete \"{_t.Name}\"? This cannot be undone.", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var all = _ds.Load();
        all.RemoveAll(x => x.Id == _t.Id);
        _ds.Save(all);
        Close();
    }

    private void Persist()
    {
        var all = _ds.Load();
        int idx = all.FindIndex(x => x.Id == _t.Id);
        if (idx >= 0) all[idx] = _t; else all.Add(_t);
        _ds.Save(all);
        RefreshView();
    }
}
