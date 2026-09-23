using System.ComponentModel.DataAnnotations;

namespace SkiControl.Models;

public class Towary
{
    [Key]
    public int Id { get; set; }
    public string? Nazwa { get; set; }
    public int? Stawka_VAT { get; set; }
    public decimal? Cena_Netto { get; set; }
    public decimal? Cena_Brutto { get; set; }
    public string? Kod_Towaru { get; set; }
    public string? Kod_PKWIU { get; set; }
    public string? Kod_PKD { get; set; }
    public bool Status { get; set; } = true;
    public Guid Guid { get; set; } = Guid.NewGuid();
    public int Rodzaj { get; set; } = 1;
    public string? Jednostka_Miary { get; set; }
}