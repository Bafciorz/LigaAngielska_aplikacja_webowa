using Liga.Web.Data;
using Liga.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Liga.Web.Controllers;

public class StadiumsController : Controller
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
            return RedirectToAction("Logowanie", "IO");
        }

        var stadiums = _context.Stadiums.ToList();
        return View(stadiums);
    }

  
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }
        return View();
    }

   
    [HttpPost]
    public IActionResult Create(Stadium stadium)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        _context.Stadiums.Add(stadium);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    
    public IActionResult Edit(int id)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        var stadium = _context.Stadiums.FirstOrDefault(s => s.StadiumId == id);
        if (stadium == null) return RedirectToAction("Index");

        return View(stadium);
    }

    
    [HttpPost]
    public IActionResult Edit(int id, Stadium stadium)
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Logowanie", "IO");
        }

        if (id != stadium.StadiumId) return RedirectToAction("Index");

        _context.Stadiums.Update(stadium);
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

        var stadium = _context.Stadiums.FirstOrDefault(s => s.StadiumId == id);
        if (stadium != null)
        {
            _context.Stadiums.Remove(stadium);
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
            checkResult = StatusCode(403, new { message = "Odmowa dostępu!" });
            return false;
        }
        checkResult = null;
        return true;
    }
    
    [HttpGet("api/stadiums")]
    public IActionResult ApiGetAll()
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        return Ok(_context.Stadiums.ToList());
    }


    [HttpGet("api/stadiums/{id}")]
    public IActionResult ApiGetById(int id)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        var stadium = _context.Stadiums.FirstOrDefault(s => s.StadiumId == id);
        if (stadium == null) return NotFound();
        return Ok(stadium);
    }


    [HttpPost("api/stadiums")]
    public IActionResult ApiCreate([FromBody] Stadium stadium)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        _context.Stadiums.Add(stadium);
        _context.SaveChanges();
        return Created($"/api/stadiums/{stadium.StadiumId}", stadium);
    }
    
    [HttpPut("api/stadiums/{id}")]
    public IActionResult ApiUpdate(int id, [FromBody] Stadium updated)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        var stadium = _context.Stadiums.FirstOrDefault(s => s.StadiumId == id);
        if (stadium == null) return NotFound();

        stadium.Name = updated.Name;
        stadium.City = updated.City;
        _context.SaveChanges();
        return Ok(new { message = "Stadion zaktualizowany przez API." });
    }
    
    [HttpDelete("api/stadiums/{id}")]
    public IActionResult ApiDelete(int id)
    {
        if (!CheckApiAuth(out var authError)) return authError!;
        var stadium = _context.Stadiums.FirstOrDefault(s => s.StadiumId == id);
        if (stadium == null) return NotFound();

        _context.Stadiums.Remove(stadium);
        _context.SaveChanges();
        return Ok(new { message = "Stadion usunięty przez API." });
    }
}