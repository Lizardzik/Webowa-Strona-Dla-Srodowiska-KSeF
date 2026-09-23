using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkiControl.Models;

// System numeracji dokumentów (Numeracja)
[Table("Numeracja")]
public class Numeracja
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Urzadzenie_Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Document_Type { get; set; } = string.Empty;

    public int Number { get; set; }

    [MaxLength(10)]
    public string? Sufix { get; set; }

    [MaxLength(10)]
    public string? Prefix { get; set; }

    public int? Year { get; set; }

    public DateTime? TimeStamp { get; set; }

    public int? Month { get; set; }
}
