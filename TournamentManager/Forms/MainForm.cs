using TournamentManager.Helpers;
using TournamentManager.Models;
using TournamentManager.Services;

namespace TournamentManager.Forms;

public class MainForm : Form
{
    private readonly DataService _ds = new();
    private List<Tournament> _list = new();
    private ListView _lv = null!;
    private Button _btnOpen = null!;
    private Button _btnDel = null!;

    public MainForm()
    {
        Build();
        Reload();
    }

    private void Build()
    {
        Text = "Tournament Manager";
        Size = new Size(860, 540);
        MinimumSize = new Size(640, 400);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppColors.Background;
        ForeColor = AppColors.Text;

        // ── Header ──────────────────────────────────────────────────
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 54,
            BackColor = AppColors.Surface,
            Padding = new Padding(16, 0, 16, 0)
        };

        var title = new Label
        {
            Text = "🏆  Tournament Manager",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = AppColors.Text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var btnNew = AppColors.Btn("+ New Tournament", AppColors.Accent, 165);
        btnNew.Dock = DockStyle.Right;
        btnNew.Margin = new Padding(0, 11, 0, 11);
        btnNew.Click += (_, _) => NewTournament();

        header.Controls.Add(title);
        header.Controls.Add(btnNew);

        // ── ListView ────────────────────────────────────────────────
        _lv = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = false,
            BorderStyle = BorderStyle.None,
            BackColor = AppColors.Background,
            ForeColor = AppColors.Text,
            Font = new Font("Segoe UI", 10),
            MultiSelect = false,
            HideSelection = false,
            HeaderStyle = ColumnHeaderStyle.Nonclickable
        };
        _lv.Columns.Add("Tournament",  280);
        _lv.Columns.Add("Teams",        70, HorizontalAlignment.Center);
        _lv.Columns.Add("Games",       100, HorizontalAlignment.Center);
        _lv.Columns.Add("Status",       -2);
        _lv.DoubleClick += (_, _) => OpenSelected();

        // ── Footer ──────────────────────────────────────────────────
        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            BackColor = AppColors.Surface,
            Padding = new Padding(16, 9, 16, 9)
        };

        _btnOpen = AppColors.Btn("Open", AppColors.Accent, 100);
        _btnOpen.Location = new Point(16, 9);
        _btnOpen.Click += (_, _) => OpenSelected();

        _btnDel = AppColors.Btn("Delete", AppColors.Danger, 100);
        _btnDel.Location = new Point(124, 9);
        _btnDel.Click += (_, _) => DeleteSelected();

        footer.Controls.Add(_btnOpen);
        footer.Controls.Add(_btnDel);
        footer.Controls.Add(AppColors.HSep());

        Controls.Add(_lv);
        Controls.Add(AppColors.HSep());
        Controls.Add(header);
        Controls.Add(footer);
    }

    private void Reload()
    {
        _list = _ds.Load();
        _lv.Items.Clear();

        foreach (var t in _list)
        {
            int played = t.Games.Count;
            bool done = played >= 5;
            string status = played == 0 ? "Not started" : $"{played} / 5 played";

            if (played > 0)
            {
                var top = ScoreCalculator.CalcStandings(t).FirstOrDefault()?.Team ?? "";
                status = done ? $"🏆 Winner: {top}" : $"🔝 Leading: {top}";
            }

            var item = new ListViewItem(new[]
                { t.Name, t.Teams.Count.ToString(), $"{played} / 5", status })
            {
                Tag = t,
                ForeColor = AppColors.Text,
                BackColor = AppColors.Background
            };
            _lv.Items.Add(item);
        }
    }

    private void OpenSelected()
    {
        if (_lv.SelectedItems.Count == 0) return;
        var t = (Tournament)_lv.SelectedItems[0].Tag!;
        using var form = new TournamentForm(t, _ds);
        form.ShowDialog(this);
        Reload();
    }

    private void NewTournament()
    {
        using var form = new NewTournamentForm();
        if (form.ShowDialog(this) != DialogResult.OK) return;
        _list.Add(form.Result!);
        _ds.Save(_list);
        Reload();
    }

    private void DeleteSelected()
    {
        if (_lv.SelectedItems.Count == 0) return;
        var t = (Tournament)_lv.SelectedItems[0].Tag!;
        if (MessageBox.Show($"Delete \"{t.Name}\"? This cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
            != DialogResult.Yes) return;
        _list.Remove(t);
        _ds.Save(_list);
        Reload();
    }
}
