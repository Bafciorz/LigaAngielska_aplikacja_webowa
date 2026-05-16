using Liga.Web.Data;
using Liga.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Liga.Web.Controllers;

public class PlayersController : Controller
{
    private readonly ApplicationDbContext _context;

    public PlayersController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        var players = _context.Players.Include(p => p.Team).ToList();
        return View(players);
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
    public IActionResult Create(Player player)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        _context.Players.Add(player);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

   
    public IActionResult Edit(int id)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        var player = _context.Players.FirstOrDefault(p => p.PlayerId == id);
        if (player == null)
        {
            return RedirectToAction("Index");
        }

        ViewBag.Teams = _context.Teams.ToList();
        return View(player);
    }

    
    [HttpPost]
    public IActionResult Edit(int id, Player player)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        if (id != player.PlayerId)
        {
            return RedirectToAction("Index");
        }

        _context.Players.Update(player);
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

        var player = _context.Players.FirstOrDefault(p => p.PlayerId == id);
        if (player != null)
        {
            _context.Players.Remove(player);
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }

 
    public IActionResult TopScorers()
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        var scorers = _context.PlayerStats
            .Where(ps => ps.Match != null && ps.Match.Date <= DateTime.Now)
            .GroupBy(ps => new { 
                ps.PlayerId, 
                ps.Player!.Name, 
                ps.Player.Surname, 
                TeamName = ps.Player.Team!.Name 
            })
            .Select(g => new TopScorerRow
            {
                PlayerName = g.Key.Name + " " + g.Key.Surname,
                TeamName = g.Key.TeamName ?? "Wolny agent",
                Goals = g.Sum(ps => ps.Goals),
                Assists = g.Sum(ps => ps.Assists),
                MatchesPlayed = g.Count(ps => ps.Minutes > 0)
            })
            .OrderByDescending(s => s.Goals)
            .ThenByDescending(s => s.Assists)
            .ToList();

        for (int i = 0; i < scorers.Count; i++)
        {
            scorers[i].Position = i + 1;
        }

        return View(scorers);
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

  
    [HttpGet("api/players")]
    public IActionResult ApiGetAll()
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        
        var players = _context.Players
            .Include(p => p.Team)
            .Select(p => new {
                p.PlayerId,
                p.Name,
                p.Surname,
                p.pozycja,
                p.TeamId,
                TeamName = p.Team != null ? p.Team.Name : "Brak"
            }).ToList();
            
        return Ok(players);
    }

    
    [HttpGet("api/players/{id}")]
    public IActionResult ApiGetById(int id)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        
        var p = _context.Players.Include(p => p.Team).FirstOrDefault(x => x.PlayerId == id);
        if (p == null) return NotFound(new { message = "Nie znaleziono zawodnika." });
        
        return Ok(new {
            p.PlayerId, p.Name, p.Surname, p.pozycja, p.TeamId,
            TeamName = p.Team != null ? p.Team.Name : "Brak"
        });
    }

    [HttpPost("api/players")]
    public IActionResult ApiCreate([FromBody] Player player)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        
        _context.Players.Add(player);
        _context.SaveChanges();
        return Created($"/api/players/{player.PlayerId}", player);
    }


    [HttpPut("api/players/{id}")]
    public IActionResult ApiUpdate(int id, [FromBody] Player updated)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        
        var p = _context.Players.FirstOrDefault(x => x.PlayerId == id);
        if (p == null) return NotFound();

        p.Name = updated.Name;
        p.Surname = updated.Surname;
        p.pozycja = updated.pozycja;
        p.TeamId = updated.TeamId;
        
        _context.SaveChanges();
        return Ok(new { message = "Zawodnik zaktualizowany przez API." });
    }

  
    [HttpDelete("api/players/{id}")]
    public IActionResult ApiDelete(int id)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        
        var p = _context.Players.FirstOrDefault(x => x.PlayerId == id);
        if (p == null) return NotFound();

        _context.Players.Remove(p);
        _context.SaveChanges();
        return Ok(new { message = "Zawodnik usunięty przez API." });
    }
}