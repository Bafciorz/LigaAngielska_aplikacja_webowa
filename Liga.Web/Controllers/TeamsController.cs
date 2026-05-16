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
}