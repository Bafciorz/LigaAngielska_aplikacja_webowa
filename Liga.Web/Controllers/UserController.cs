namespace Liga.Web.Controllers;
using System.Security.Cryptography;
using System.Text;
using Liga.Web.Data;
using Liga.Web.Models;
using Microsoft.AspNetCore.Mvc;
public class UsersController:Controller
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    private bool IsAdmin()
    {
        var username = HttpContext.Session.GetString("Username");
        return username == "admin";
    }


    public IActionResult Index()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Index", "Home");
        }
        var users = _context.Users.ToList();
        return View(users);
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    public IActionResult Create(string username, string password)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Index", "Home");
        }

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "wypelnij wszystkie pola";
            return View();
        }
        if (_context.Users.Any(u => u.Username == username))
        {
            ViewBag.Error = "Taki użytkownik już istnieje w bazie.";
            return View();
        }

        var new_user = new User
        {
            Username = username,
            PasswordHash = HashPassword(password)
        };
        _context.Users.Add(new_user);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");
        var userToDelete = _context.Users.Find(id);
        if (userToDelete != null)
        {
            var currentUsername = HttpContext.Session.GetString("Username");
            if (userToDelete.Username == currentUsername)
            {
                TempData["ErrorMessage"] = "Błąd: Nie możesz usunąć własnego konta administratora!";
                return RedirectToAction("Index");
            }
            _context.Users.Remove(userToDelete);
            _context.SaveChanges();
            TempData["SuccessMessage"] = $"Użytkownik {userToDelete.Username} został usunięty.";
        }
        return RedirectToAction("Index");
    }
    
    public string HashPassword(string haslo)
    {
        HashAlgorithm algorithm = SHA256.Create();
        byte[] tekts = algorithm.ComputeHash(Encoding.UTF8.GetBytes(haslo));
        return Convert.ToHexString(tekts).ToLower();
    }
    
}