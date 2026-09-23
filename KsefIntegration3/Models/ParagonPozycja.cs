using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkiControl.Models;

// Pozycje paragonu (Paragony_Pozycje)
[Table("Paragony_Pozycje")]
public class ParagonPozycja
{
    [Key]
    public Guid Guid { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Paragon")]
    public Guid Paragon_Guid { get; set; }
    public Paragon Paragon { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Paragon_Numer { get; set; } = string.Empty;

    [Column(TypeName = "date")]
    public DateTime Data { get; set; }

    [Required]
    [MaxLength(100)]
    public string Pozycja_Nazwa { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Pozycja_Cena_Netto { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Pozycja_Cena_Brutto { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Pozycja_Podatek { get; set; }

    public int Pozycja_Stawka_VAT { get; set; }

    [MaxLength(100)]
    public string? Pozycja_Opis { get; set; }

    [MaxLength(50)]
    public string? Towar_Id { get; set; }

    public bool? db_Transfer { get; set; }

    public DateTime? db_Transfer_Date { get; set; }

    public int Pozycja_Ilosc { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Pozycja_Wartosc_N { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Pozycja_Wartosc_B { get; set; }

    [MaxLength(50)]
    public string? Transaction_Id { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Pozycja_Wartosc_VAT { get; set; }

    public bool is_Deleted { get; set; }

    public DateTime? TimeStamp { get; set; }

    [Required]
    [MaxLength(100)]
    public string Urzadzenie_Id { get; set; } = string.Empty;

    public int Paragon_Id { get; set; }
}
