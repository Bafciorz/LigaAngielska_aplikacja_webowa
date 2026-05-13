using System.ComponentModel.DataAnnotations;

namespace Liga.Web.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Username { get; set; } = "";
    [Required]
    public string PasswordHash { get; set; } = ""; // Przechowywane jako skrót
    public string Role { get; set; } = "User"; // Np. Admin lub User
    public string ApiKey { get; set; } = Guid.NewGuid().ToString(); // Do REST API
}