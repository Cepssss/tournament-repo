namespace TournamentManager.Models;

public class Tournament
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<string> Teams { get; set; } = new();
    public List<Game> Games { get; set; } = new();

    // One multiplier per position (index 0 = 1st place). Defaults match original ruleset.
    public List<double> Multipliers { get; set; } = new()
        { 1.6, 1.4, 1.4, 1.4, 1.4, 1.2, 1.2, 1.2, 1.2, 1.2, 1.0, 1.0, 1.0, 1.0, 1.0 };
}
