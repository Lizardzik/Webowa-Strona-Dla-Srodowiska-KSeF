using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkiControl.Models;

// Pojedyncza linijka na fakturze (konkretny towar lub usługa) - Faktury_Pozycje
[Table("Faktury_Pozycje")]
public class FakturaPozycja
{
    [Key]
    public Guid Guid { get; set; }


    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Faktura")]
    public Guid Faktura_Guid { get; set; }
    public Faktura Faktura { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Faktura_Numer { get; set; } = string.Empty;

    [Column(TypeName = "date")]
    public DateTime Data { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Pozycja_Nazwa { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Pozycja_Cena_Netto { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Pozycja_Cena_Brutto { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Pozycja_Kwota_VAT { get; set; }

    [Required]
    [MaxLength(10)]
    public string Pozycja_Stawka_VAT { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Pozycja_Opis { get; set; }

    [MaxLength(50)]
    public string? Towar_Id { get; set; }

    public Guid? Towar_Guid { get; set; }

    public bool db_Transfer { get; set; }

    public DateTime? db_Transfer_Date { get; set; }

    public int Pozycja_Ilosc { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Pozycja_Wartosc_N { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Pozycja_Wartosc_B { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Pozycja_Wartosc_VAT { get; set; }

    [Required]
    [MaxLength(100)]
    public string Urzadzenie_Id { get; set; } = string.Empty;

    public DateTime? TimeStamp { get; set; }

    public bool is_Deleted { get; set; }

    public int? Faktura_Id { get; set; }
}
