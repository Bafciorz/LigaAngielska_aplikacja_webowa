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
}