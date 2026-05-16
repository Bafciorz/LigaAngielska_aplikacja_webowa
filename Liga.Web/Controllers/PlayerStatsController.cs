using Liga.Web.Data;
using Liga.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Liga.Web.Controllers;

public class PlayerStatsController : Controller
{
    private readonly ApplicationDbContext _context;

    public PlayerStatsController(ApplicationDbContext context)
    {
        _context = context;
    }

  
    public IActionResult Create(int matchId)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        var match = _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .FirstOrDefault(m => m.MatchId == matchId);

        if (match == null) return RedirectToAction("Index", "Matches");

        ViewBag.Match = match;
        ViewBag.Players = _context.Players.ToList();
        return View();
    }


    [HttpPost]
    public IActionResult Create(PlayerStat stat)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }
        
        var exists = _context.PlayerStats.Any(ps => ps.MatchId == stat.MatchId && ps.PlayerId == stat.PlayerId);
        if (!exists)
        {
            _context.PlayerStats.Add(stat);
            _context.SaveChanges();
        }

 
        return RedirectToAction("Details", "Matches", new { id = stat.MatchId });
    }

   
    public IActionResult Edit(int matchId, int playerId)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        var stat = _context.PlayerStats
            .Include(ps => ps.Player)
            .Include(ps => ps.Match)
            .FirstOrDefault(ps => ps.MatchId == matchId && ps.PlayerId == playerId);

        if (stat == null) return RedirectToAction("Details", "Matches", new { id = matchId });

        return View(stat);
    }

     [HttpPost]
    public IActionResult Edit(PlayerStat stat)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        _context.PlayerStats.Update(stat);
        _context.SaveChanges();

        return RedirectToAction("Details", "Matches", new { id = stat.MatchId });
    }
    
    [HttpPost]
    public IActionResult Delete(int matchId, int playerId)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        var stat = _context.PlayerStats.FirstOrDefault(ps => ps.MatchId == matchId && ps.PlayerId == playerId);
        if (stat != null)
        {
            _context.PlayerStats.Remove(stat);
            _context.SaveChanges();
        }

        return RedirectToAction("Details", "Matches", new { id = matchId });
    }
    
}