using Liga.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Liga.Web.Controllers;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Liga.Web.Models;

public class StadiumsController:Controller
{
    private readonly ApplicationDbContext _context;

    public StadiumsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "Home");
        }
        
        var Stadiums = _context.Stadiums.ToList();
        return View(Stadiums);
    }
}