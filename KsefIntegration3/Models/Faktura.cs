using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkiControl.Models;

// Reprezentacja pojedynczej faktury w bazie danych (Faktury)
[Table("Faktury")]
public class Faktura
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public List<FakturaPozycja> FakturaPozycje { get; set; } = new List<FakturaPozycja>();

    public int Numer { get; set; }

    [Required]
    [MaxLength(50)]
    public string Faktura_Numer { get; set; } = string.Empty;

    [Column(TypeName = "date")]
    public DateTime Data { get; set; }

    [MaxLength(100)]
    public string? Uzytkownik { get; set; }

    public bool Flaga_Fiskalna { get; set; }

    [MaxLength(30)]
    public string? Nabywca_NIP { get; set; }

    public string? Nabywca_Nazwa { get; set; }

    [MaxLength(100)]
    public string? Nabywca_Miejscowosc { get; set; }

    [MaxLength(10)]
    public string? Nabywca_KodPocztowy { get; set; }

    [MaxLength(100)]
    public string? Nabywca_Ulica { get; set; }

    [MaxLength(50)]
    public string? Nabywca_UlicaNumer { get; set; }

    [MaxLength(100)]
    public string? Nabywca_Imie { get; set; }

    [MaxLength(100)]
    public string? Nabywca_Nazwisko { get; set; }

    public int Nabywca_Rodzaj { get; set; }

    [MaxLength(30)]
    public string? Odbiorca_NIP { get; set; }

    public string? Odbiorca_Nazwa { get; set; }

    [MaxLength(100)]
    public string? Odbiorca_Miejscowosc { get; set; }

    [MaxLength(10)]
    public string? Odbiorca_KodPocztowy { get; set; }

    [MaxLength(100)]
    public string? Odbiorca_Ulica { get; set; }

    [MaxLength(50)]
    public string? Odbiorca_UlicaNumer { get; set; }

    [MaxLength(100)]
    public string? Odbiorca_Imie { get; set; }

    [MaxLength(100)]
    public string? Odbiorca_Nazwisko { get; set; }

    [MaxLength(100)]
    public string? Faktura_Uwagi { get; set; }

    public bool db_Transfer { get; set; }

    public DateTime? db_Transfer_Date { get; set; }

    [Column(TypeName = "date")]
    public DateTime Faktura_Data { get; set; }

    [MaxLength(50)]
    public string? Platnosc_Nazwa { get; set; }

    public int? Platnosc_Termin { get; set; }

    [MaxLength(100)]
    public string? Paragon_Numer { get; set; }

    [MaxLength(100)]
    public string? Transaction_Id { get; set; }

    [Column(TypeName = "date")]
    public DateTime Usluga_Data { get; set; }

    public Guid Guid { get; set; }

    public Guid? Paragon_Guid { get; set; }

    [Required]
    [MaxLength(100)]
    public string Urzadzenie_Id { get; set; } = string.Empty;

    public bool is_Deleted { get; set; }

    public DateTime? TimeStamp { get; set; }

    [MaxLength(100)]
    public string? Sprzedawca_Nazwa { get; set; }

    [MaxLength(100)]
    public string? Sprzedawca_Miejscowosc { get; set; }

    [MaxLength(10)]
    public string? Sprzedawca_KodPocztowy { get; set; }

    [MaxLength(100)]
    public string? Sprzedawca_Ulica { get; set; }

    [MaxLength(50)]
    public string? Sprzedawca_Numer { get; set; }

    [MaxLength(30)]
    public string? Sprzedawca_NIP { get; set; }

    [MaxLength(100)]
    public string? Faktura_MiejsceWystawienia { get; set; }

    public int? Faktura_Typ { get; set; }

    public Guid? oGuid { get; set; }

    [NotMapped]
    public decimal Kwota { get; set; }

    [NotMapped]
    public string NabywcaDisplayName =>
        !string.IsNullOrWhiteSpace(Nabywca_Nazwa)
            ? Nabywca_Nazwa
            : $"{Nabywca_Imie} {Nabywca_Nazwisko}".Trim();
}
