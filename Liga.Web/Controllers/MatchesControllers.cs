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

        // Tutaj musimy zaimportować dwa razy tabelę Team (dla gospodarzy i gości)
        var matches = _context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .ToList();

        return View(matches);
    }
}