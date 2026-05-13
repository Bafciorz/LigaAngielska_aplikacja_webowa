namespace Liga.Web.Models;
using System.ComponentModel.DataAnnotations;

public class Stadium
{
    [Key]
    public int StadiumId { get; set; }

    [Required] public string Name { get; set; } = "";
    public string City { get; set; } = "";
}