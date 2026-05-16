using Liga.Web.Data;
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
        
        var matches = _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .ToList();

        return View(matches);
    }

    public IActionResult Details(int id)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }
        
        var match = _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .FirstOrDefault(m => m.MatchId == id);

        if (match == null)
        {
            return RedirectToAction("Index");
        }
        var playerStats = _context.PlayerStats
            .Include(ps => ps.Player)
            .ThenInclude(ps =>  ps.Team)
            .Where(ps => ps.MatchId == id)
            .ToList();
        ViewBag.Match =  match;
        return View(playerStats);
    }
}