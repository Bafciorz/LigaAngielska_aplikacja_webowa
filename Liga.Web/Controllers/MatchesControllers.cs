using Liga.Web.Data;
using Liga.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Liga.Web.Controllers;

public class MatchesController : Controller
{
    private readonly ApplicationDbContext _context;

    public MatchesController(ApplicationDbContext context)
    {
        _context = context;
    }


    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        var matches = _context.Matches.Include(m => m.HomeTeam).Include(m => m.AwayTeam).ToList();
        return View(matches);
    }

  
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        ViewBag.Teams = _context.Teams.ToList();
        return View();
    }

    
    [HttpPost]
    public IActionResult Create(Match match)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        _context.Matches.Add(match);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    
    public IActionResult Edit(int id)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        var match = _context.Matches.FirstOrDefault(m => m.MatchId == id);
        if (match == null) return RedirectToAction("Index");

        ViewBag.Teams = _context.Teams.ToList();
        return View(match);
    }

  
    [HttpPost]
    public IActionResult Edit(int id, Match match)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        if (id != match.MatchId) return RedirectToAction("Index");

        _context.Matches.Update(match);
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

        var match = _context.Matches.FirstOrDefault(m => m.MatchId == id);
        if (match != null)
        {
            _context.Matches.Remove(match);
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    
    public IActionResult Details(int id)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        var match = _context.Matches.Include(m => m.HomeTeam).Include(m => m.AwayTeam).FirstOrDefault(m => m.MatchId == id);
        if (match == null) return RedirectToAction("Index");

        var playerStats = _context.PlayerStats.Include(ps => ps.Player).ThenInclude(p => p.Team).Where(ps => ps.MatchId == id).ToList();
        ViewBag.Match = match;
        return View(playerStats);
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
    
    [HttpGet("api/matches")]
    public IActionResult ApiGetAll()
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        
        var matches = _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Select(m => new {
                m.MatchId,
                m.Date,
                m.HomeTeamId,
                HomeTeamName = m.HomeTeam != null ? m.HomeTeam.Name : "Nieznany",
                m.AwayTeamId,
                AwayTeamName = m.AwayTeam != null ? m.AwayTeam.Name : "Nieznany",
                m.HomeGoals,
                m.AwayGoals
            }).ToList();
            
        return Ok(matches);
    }


    [HttpGet("api/matches/{id}")]
    public IActionResult ApiGetById(int id)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        
        var m = _context.Matches.Include(x => x.HomeTeam).Include(x => x.AwayTeam).FirstOrDefault(x => x.MatchId == id);
        if (m == null) return NotFound(new { message = "Nie znaleziono meczu." });
        
        return Ok(new {
            m.MatchId, m.Date, m.HomeTeamId,
            HomeTeamName = m.HomeTeam != null ? m.HomeTeam.Name : "Nieznany",
            m.AwayTeamId,
            AwayTeamName = m.AwayTeam != null ? m.AwayTeam.Name : "Nieznany",
            m.HomeGoals, m.AwayGoals
        });
    }

    [HttpPost("api/matches")]
    public IActionResult ApiCreate([FromBody] Match match)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        
        _context.Matches.Add(match);
        _context.SaveChanges();
        return Created($"/api/matches/{match.MatchId}", match);
    }


    [HttpPut("api/matches/{id}")]
    public IActionResult ApiUpdate(int id, [FromBody] Match updated)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        
        var m = _context.Matches.FirstOrDefault(x => x.MatchId == id);
        if (m == null) return NotFound();

        m.Date = updated.Date;
        m.HomeTeamId = updated.HomeTeamId;
        m.AwayTeamId = updated.AwayTeamId;
        m.HomeGoals = updated.HomeGoals;
        m.AwayGoals = updated.AwayGoals;
        
        _context.SaveChanges();
        return Ok(new { message = "Mecz zaktualizowany przez API." });
    }


    [HttpDelete("api/matches/{id}")]
    public IActionResult ApiDelete(int id)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        
        var m = _context.Matches.FirstOrDefault(x => x.MatchId == id);
        if (m == null) return NotFound();

        _context.Matches.Remove(m);
        _context.SaveChanges();
        return Ok(new { message = "Mecz usunięty przez API." });
    }
}