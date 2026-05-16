using Liga.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Liga.Web.Models;

namespace Liga.Web.Controllers;

public class TeamsController : Controller
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
        var teams = _context.Teams
            .Include(p => p.Stadium)
            .ToList();
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

    
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }
        ViewBag.Stadiums = _context.Stadiums.ToList();
        return View();
    }


    [HttpPost]
    public IActionResult Create(Team team)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        _context.Teams.Add(team);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

   
    public IActionResult Edit(int id)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        var team = _context.Teams.FirstOrDefault(t => t.TeamId == id);
        if (team == null)
        {
            return RedirectToAction("Index");
        }

        ViewBag.Stadiums = _context.Stadiums.ToList();
        return View(team);
    }

    
    [HttpPost]
    public IActionResult Edit(int id, Team team)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        if (id != team.TeamId)
        {
            return RedirectToAction("Index");
        }

        _context.Teams.Update(team);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    
    [HttpPost]
    public IActionResult Delete(int id)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        var team = _context.Teams.FirstOrDefault(t => t.TeamId == id);
        if (team != null)
        {
            _context.Teams.Remove(team);
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }
    private bool CheckApiAuth(out IActionResult? checkResult)
    {
        if (!Request.Headers.TryGetValue("X-Username", out var username) ||
            !Request.Headers.TryGetValue("X-ApiKey", out var apiKey))
        {
            checkResult = Unauthorized(new { message = "Brak nagłówków autoryzacji!" });
            return false;
        }
        var userExists = _context.Users.Any(u => u.Username == username.ToString() && u.ApiKey == apiKey.ToString());
        if (!userExists)
        {
            checkResult = StatusCode(403, new { message = "Odmowa dostępu! Zły token." });
            return false;
        }
        checkResult = null;
        return true;
    }

  
    [HttpGet("api/teams")]
    public IActionResult ApiGetAll()
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        
        var teams = _context.Teams
            .Include(t => t.Stadium)
            .Select(t => new {
                t.TeamId,
                t.Name,
                t.StadiumId,
                StadiumName = t.Stadium != null ? t.Stadium.Name : "Brak"
            }).ToList();
            
        return Ok(teams);
    }

   
    [HttpGet("api/teams/{id}")]
    public IActionResult ApiGetById(int id)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        
        var t = _context.Teams.Include(x => x.Stadium).FirstOrDefault(x => x.TeamId == id);
        if (t == null) return NotFound(new { message = "Nie znaleziono klubu." });
        
        return Ok(new {
            t.TeamId, t.Name, t.StadiumId,
            StadiumName = t.Stadium != null ? t.Stadium.Name : "Brak"
        });
    }

    
    [HttpPost("api/teams")]
    public IActionResult ApiCreate([FromBody] Team team)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        
        _context.Teams.Add(team);
        _context.SaveChanges();
        return Created($"/api/teams/{team.TeamId}", team);
    }


    [HttpPut("api/teams/{id}")]
    public IActionResult ApiUpdate(int id, [FromBody] Team updated)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        
        var t = _context.Teams.FirstOrDefault(x => x.TeamId == id);
        if (t == null) return NotFound();

        t.Name = updated.Name;
        t.StadiumId = updated.StadiumId;
        
        _context.SaveChanges();
        return Ok(new { message = "Drużyna zaktualizowana przez API." });
    }


    [HttpDelete("api/teams/{id}")]
    public IActionResult ApiDelete(int id)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        
        var t = _context.Teams.FirstOrDefault(x => x.TeamId == id);
        if (t == null) return NotFound();

        _context.Teams.Remove(t);
        _context.SaveChanges();
        return Ok(new { message = "Drużyna usunięta przez API." });
    }
}