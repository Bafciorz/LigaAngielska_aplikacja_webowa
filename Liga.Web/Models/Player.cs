using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Liga.Web.Models;

public class Player
{
    [Key]
    public int PlayerId { get; set; }
    [Required]
    public string Name { get; set; } = "";
    [Required]
    public string Surname { get; set; } = "";
    [Required]
    public string pozycja { get; set; } = "";
    
    public int TeamId { get; set; }
    [ForeignKey("TeamId")]
    public virtual Team? Team { get; set; }
    
}