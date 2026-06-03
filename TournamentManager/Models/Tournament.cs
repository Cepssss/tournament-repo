namespace TournamentManager.Models;

public class Tournament
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<string> Teams { get; set; } = new();
    public List<Game> Games { get; set; } = new();
}
