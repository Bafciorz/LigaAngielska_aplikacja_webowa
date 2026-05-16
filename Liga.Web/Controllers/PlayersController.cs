using Liga.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Liga.Web.Controllers;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Liga.Web.Models;

public class PlayersController:Controller
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
            return RedirectToAction("Logowanie", "Home");
        }
        
        var players = _context.Players
            .Include(p => p.Team) 
            .ToList();
        return View(players);
    }
}