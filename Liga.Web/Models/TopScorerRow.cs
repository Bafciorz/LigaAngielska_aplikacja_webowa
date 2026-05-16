namespace Liga.Web.Models;

public class TopScorerRow
{
    public int Position { get; set; }
    public string PlayerName { get; set; } = "";
    public string TeamName { get; set; } = "";
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int MatchesPlayed { get; set; }
}