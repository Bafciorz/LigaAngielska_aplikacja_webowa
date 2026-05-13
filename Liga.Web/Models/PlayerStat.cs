namespace Liga.Web.Models;

public class PlayerStat
{
    public int PlayerId { get; set; }
    public virtual Player? Player { get; set; }

    public int MatchId { get; set; }
    public virtual Match? Match { get; set; }

    public int Goals { get; set; }
    public int Assists { get; set; }
    public int Minutes { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
}