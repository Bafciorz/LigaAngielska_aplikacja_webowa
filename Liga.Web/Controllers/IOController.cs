using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Liga.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Liga.Web.Models;

namespace Liga.Web.Controllers;

public class IOController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public IOController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Logowanie()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Logowanie(IFormCollection form)
    {
        
        string login = form["login"].ToString();
        string haslo = form["haslo"].ToString();
        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(haslo))
        {
            ViewBag.Error = "Proszę podać login i hasło.";
            return View(); 
        }
        var hash = HashPassword(haslo);
        
        var user = _context.Users.FirstOrDefault(u => u.Username == login && u.PasswordHash == hash );
        if (user != null)
        {
            
            HttpContext.Session.SetString("IsLoggedIn", "true");
            HttpContext.Session.SetString("Username", user.Username);
            
            return RedirectToAction("Index", "Home");
            
        }
        ViewBag.Error = "Nieprawidłowy login lub hasło.";
        return View();
        
        
    }
    public IActionResult Wyloguj()
    {
        HttpContext.Session.Clear(); 
        return RedirectToAction("Logowanie");
    }



    public string HashPassword(string haslo)
    {
        HashAlgorithm algorithm = SHA256.Create();
        byte[] tekts = algorithm.ComputeHash(Encoding.UTF8.GetBytes(haslo));
        return Convert.ToHexString(tekts).ToLower();
    }
}