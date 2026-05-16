namespace Liga.Web.Data;
using Microsoft.EntityFrameworkCore;
using Liga.Web.Models;

public class ApplicationDbContext:DbContext
{
    public  ApplicationDbContext(DbContextOptions<ApplicationDbContext>options):base(options)
    {
    }
    
    public DbSet<Team>  Teams { get; set; }
    public DbSet<Match>  Matches { get; set; }
    public DbSet<Player>  Players { get; set; }
    public DbSet<Stadium >  Stadiums { get; set; }
    public DbSet<PlayerStat>  PlayerStats { get; set; }
    public DbSet<User>  Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PlayerStat>().
            HasKey(ps => new { ps.PlayerId, ps.MatchId });
        
        modelBuilder.Entity<Match>().
            HasOne(m => m.HomeTeam).
            WithMany(). 
            HasForeignKey(m => m.HomeTeamId).
            OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Match>().
            HasOne(m => m.AwayTeam).
            WithMany().
            HasForeignKey(m => m.AwayTeamId).
            OnDelete(DeleteBehavior.Restrict);
    }
}