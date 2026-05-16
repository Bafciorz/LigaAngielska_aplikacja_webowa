using System.Security.Cryptography;
using System.Text;
using Liga.Web.Models;

namespace Liga.Web.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();
        
        if (context.Teams.Any())
        {
            return; 
        }


        var oldTrafford = new Stadium { Name = "Old Trafford", City = "Manchester" };
        var anfield = new Stadium { Name = "Anfield", City = "Liverpool" };
        
        context.Stadiums.AddRange(oldTrafford, anfield);
        context.SaveChanges(); 

        var manUtd = new Team { Name = "Manchester United", StadiumId = oldTrafford.StadiumId };
        var liverpool = new Team { Name = "Liverpool FC", StadiumId = anfield.StadiumId };

        context.Teams.AddRange(manUtd, liverpool);
        context.SaveChanges(); 
        
        var player1 = new Player { Name = "Bruno", Surname = "Fernandes",pozycja = "Pomocnik", TeamId = manUtd.TeamId };
        var player2 = new Player { Name = "Mohamed", Surname = "Salah",pozycja="Napastnik",  TeamId = liverpool.TeamId };

        context.Players.AddRange(player1, player2);
        
        var testMatch = new Match
            
        {
            Date = DateTime.Now.AddDays(7),
            HomeTeamId = manUtd.TeamId,
            AwayTeamId = liverpool.TeamId,
            HomeGoals = 1,
            AwayGoals = 1
        };

        context.Matches.Add(testMatch);
        context.Matches.Add(testMatch);
        context.SaveChanges(); 

   
        var statBruno = new PlayerStat
        {
            MatchId = testMatch.MatchId,
            PlayerId = player1.PlayerId,
            Minutes = 90,
            Goals = 1,     
            Assists = 0,
            YellowCards = 1, 
            RedCards = 0
        };

        var statSalah = new PlayerStat
        {
            MatchId = testMatch.MatchId,
            PlayerId = player2.PlayerId,
            Minutes = 90,
            Goals = 1,    
            Assists = 1,
            YellowCards = 0,
            RedCards = 0
        };

        context.PlayerStats.AddRange(statBruno, statSalah);
        

    
        var admin = new User 
        { 
            Username = "admin", 
            PasswordHash = HashPassword("Admin123!") 
        };
        
        context.Users.Add(admin);
        
        context.SaveChanges();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}