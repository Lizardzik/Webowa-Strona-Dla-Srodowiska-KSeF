using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkiControl.Models;

// Podmioty na fakturze (Faktury_Podmioty)
[Table("Faktury_Podmioty")]
public class FakturaPodmiot
{
    [Key]
    public Guid Guid { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public Guid Faktura_Guid { get; set; }

    [Required]
    [MaxLength(50)]
    public string Faktura_Numer { get; set; } = string.Empty;

    public string? Nazwa { get; set; }

    [MaxLength(30)]
    public string? NIP { get; set; }

    [MaxLength(100)]
    public string? Miejscowosc { get; set; }

    [MaxLength(10)]
    public string? Kod_Pocztowy { get; set; }

    [MaxLength(100)]
    public string? Ulica { get; set; }

    [MaxLength(50)]
    public string? Numer_Domu { get; set; }

    [MaxLength(10)]
    public string? Kod_Kraju { get; set; }

    public int Typ_Na_Fa { get; set; }

    public bool is_Deleted { get; set; }

    public DateTime Timestamp { get; set; }
}
