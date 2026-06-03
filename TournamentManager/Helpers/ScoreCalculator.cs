using TournamentManager.Models;

namespace TournamentManager.Helpers;

public static class ScoreCalculator
{
    public static double GetMultiplier(int position) => position switch
    {
        1    => 1.6,
        <= 5 => 1.4,
        <= 10 => 1.2,
        _    => 1.0
    };

    public static double CalcScore(int kills, int position) =>
        Math.Round(kills * GetMultiplier(position), 2);

    public static string Fmt(double pts)
    {
        if (pts == 0) return "0";
        string s = pts.ToString("F2").TrimEnd('0');
        return s.EndsWith('.') ? s.TrimEnd('.') : s;
    }

    public static List<StandingRow> CalcStandings(Tournament t)
    {
        var map = t.Teams.ToDictionary(team => team, _ => new StandingRow());

        foreach (var game in t.Games)
        {
            foreach (var team in t.Teams)
            {
                var r = game.Results.FirstOrDefault(x => x.Team == team);
                if (r != null)
                {
                    double pts = CalcScore(r.Kills, r.Position);
                    map[team].GameScores.Add(pts);
                    map[team].Total = Math.Round(map[team].Total + pts, 2);
                }
                else
                {
                    map[team].GameScores.Add(null);
                }
            }
        }

        return map
            .Select(kv => { kv.Value.Team = kv.Key; return kv.Value; })
            .OrderByDescending(e => e.Total)
            .ToList();
    }
}

public class StandingRow
{
    public string Team { get; set; } = "";
    public List<double?> GameScores { get; set; } = new();
    public double Total { get; set; }
}
