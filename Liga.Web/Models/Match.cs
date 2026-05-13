using System.ComponentModel.DataAnnotations;

namespace Liga.Web.Models;

public class Match
{
    [Key]
    public int MatchId { get; set; }
    
    public DateTime Date { get; set; }
    
    public int HomeTeamId { get; set; }
    public virtual Team? HomeTeam { get; set; }
    
    public int AwayTeamId { get; set; }
    public virtual Team? AwayTeam { get; set; }

    public int HomeGoals { get; set; }
    public int AwayGoals { get; set; }
}