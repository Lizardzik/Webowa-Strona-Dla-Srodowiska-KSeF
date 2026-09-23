using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkiControl.Models;

// Reprezentacja paragonu (Paragony)
[Table("Paragony")]
public class Paragon
{
    [Key]
    public Guid Guid { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int Numer { get; set; }

    [Required]
    [MaxLength(50)]
    public string Paragon_Numer { get; set; } = string.Empty;

    [Column(TypeName = "date")]
    public DateTime Data { get; set; }

    [MaxLength(50)]
    public string? Uzytkownik { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Wartosc { get; set; }

    public bool Flaga_Fiskalna { get; set; }

    public bool? Flaga_Faktura { get; set; }

    public Guid? Faktura_Guid { get; set; }

    [MaxLength(50)]
    public string? Faktura_Numer { get; set; }

    public DateTime? Data_Fiskalna { get; set; }

    [MaxLength(30)]
    public string? Klient_NIP { get; set; }

    public string? Klient_Nazwa { get; set; }

    [MaxLength(100)]
    public string? Klient_Adres { get; set; }

    [MaxLength(100)]
    public string? Paragon_Uwagi { get; set; }

    public bool? db_Transfer { get; set; }

    public DateTime? db_Transfer_Date { get; set; }

    [MaxLength(50)]
    public string? Transaction_Id { get; set; }

    public DateTime? TimeStamp { get; set; }

    public bool? is_Deleted { get; set; }

    [Required]
    [MaxLength(100)]
    public string Urzadzenie_Id { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? fd_pnr { get; set; }

    [MaxLength(50)]
    public string? fd_unr { get; set; }
}
