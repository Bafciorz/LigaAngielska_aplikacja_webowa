using Liga.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Liga.Web.Controllers;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Liga.Web.Models;
public class TeamsController:Controller
{
    private readonly ApplicationDbContext _context;

    public TeamsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }
        var teams = _context.Teams.
            Include(p => p.Stadium).
            ToList();
        return View(teams);
    }
    
    public IActionResult Details(int id)
    {
  
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        var team = _context.Teams
            .Include(t => t.Stadium)
            .FirstOrDefault(t => t.TeamId == id);

        if (team == null)
        {
            return RedirectToAction("Index"); 
        }

        var players = _context.Players
            .Where(p => p.TeamId == id)
            .ToList();

        ViewBag.Team = team;
        return View(players);
    }

    public IActionResult Table()
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }
        
        var teams = _context.Teams.ToList();
        var playedMatches = _context.Matches
            .Where(m => m.Date <= DateTime.Now)
            .ToList();
        
        var leagueTable = teams.Select(
            t => new LeagueTableRow()
            {
                TeamName = t.Name,
                MatchesPlayed = 0,
                Wins = 0,
                Losses = 0,
                Draws = 0,
                GoalsScored = 0,
                GoalsConceded = 0,
                Points = 0,
                
            }
            ).ToList();

        foreach (var match in playedMatches)
        {
            var homeTeamRow = leagueTable.FirstOrDefault(r =>
                r.TeamName == teams.FirstOrDefault(t => t.TeamId == match.HomeTeamId)?.Name);
            var awayTeamRow = leagueTable.FirstOrDefault(r =>
                r.TeamName == teams.FirstOrDefault(t => t.TeamId == match.AwayTeamId)?.Name);
            if (homeTeamRow != null && awayTeamRow != null)
            {
                homeTeamRow.MatchesPlayed++;
                awayTeamRow.MatchesPlayed++;

                homeTeamRow.GoalsScored += match.HomeGoals;
                homeTeamRow.GoalsConceded += match.AwayGoals;
                awayTeamRow.GoalsScored += match.AwayGoals;
                awayTeamRow.GoalsConceded += match.HomeGoals;

                if (match.HomeGoals > match.AwayGoals)
                {
                    homeTeamRow.Wins++;
                    homeTeamRow.Points += 3;
                    awayTeamRow.Losses++;
                }
                else if (match.HomeGoals < match.AwayGoals)
                {
                    awayTeamRow.Wins++;
                    awayTeamRow.Points += 3;
                    homeTeamRow.Losses++;
                }
                else
                {
                    homeTeamRow.Draws++;
                    homeTeamRow.Points += 1;
                    awayTeamRow.Draws++;
                    awayTeamRow.Points += 1;
                }
            }
        }

        var sortedTable = leagueTable
                .OrderByDescending(r => r.Points)
                .ThenByDescending(r => r.GoalDifference)
                .ThenByDescending(r => r.GoalsScored)
                .ToList();
            for (int i = 0; i < sortedTable.Count; i++)
            {
                sortedTable[i].Position = i + 1;
            }
            
            
            return View(sortedTable);
        }
    
}