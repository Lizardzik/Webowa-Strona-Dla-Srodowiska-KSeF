using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkiControl.Models;

// Model bazy danych dla kontrahenta (Kontrahenci)
[Table("Kontrahenci")]
public class Kontrahent
{
    [Key]
    public int Id { get; set; }

    public string? Nazwa { get; set; }
    public string? Imie { get; set; }
    public string? Nazwisko { get; set; }
    public string? NIP { get; set; }
    public string? Regon { get; set; }
    public string? Miejscowosc { get; set; }
    public string? Kod_Pocztowy { get; set; }
    public string? Ulica { get; set; }
    public string? NumerDomu { get; set; }
    public bool Status { get; set; } = true;
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string? Typ { get; set; }
    public int Kontrahent_Rodzaj { get; set; } = 1;
    public string? Kod_Kraju { get; set; } = "PL";
    //public DateTime? DataDodania { get; set; }
    //public string? Pesel { get; set; }
    //public string? Email { get; set; }
    //public string? Telefon { get; set; }

    // Pełna nazwa do wyświetlania na listach i w tabelach
    [NotMapped]
    public string DisplayName =>
        !string.IsNullOrWhiteSpace(Nazwa)
            ? Nazwa
            : $"{Imie} {Nazwisko}".Trim();

    // Adres sformatowany specjalnie do wydruku na dokumencie
    [NotMapped]
    public string FullAddress => $"{Ulica} {NumerDomu}, {Kod_Pocztowy} {Miejscowosc}".Trim(' ', ',');
}
