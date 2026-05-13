using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Liga.Web.Models;

public class Team
{
    [Key]
    public int TeamId { get; set; }
    [Required]
    public string Name { get; set; } = "";
    public int StadiumId { get; set; }
    [ForeignKey("StadiumId")]
    public virtual Stadium? Stadium { get; set; } 

}