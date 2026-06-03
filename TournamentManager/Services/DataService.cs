using System.Text.Json;
using TournamentManager.Models;

namespace TournamentManager.Services;

public class DataService
{
    private readonly string _path;
    private static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

    public DataService()
    {
        string dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TournamentManager");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "tournaments.json");
    }

    public List<Tournament> Load()
    {
        if (!File.Exists(_path)) return new();
        try { return JsonSerializer.Deserialize<List<Tournament>>(File.ReadAllText(_path)) ?? new(); }
        catch { return new(); }
    }

    public void Save(List<Tournament> tournaments) =>
        File.WriteAllText(_path, JsonSerializer.Serialize(tournaments, _opts));
}
