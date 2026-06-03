namespace TournamentManager.Models;

public class Game
{
    public int Id { get; set; }
    public DateTime PlayedAt { get; set; } = DateTime.Now;
    public List<GameResult> Results { get; set; } = new();
}
