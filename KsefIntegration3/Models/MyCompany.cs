using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkiControl.Models;

[Table("MojaFirma")]
public class MyCompany
{
    [Key]
    public int Id { get; set; }
    public string Nazwa { get; set; } = string.Empty;
    public string NIP { get; set; } = string.Empty;
    public string Regon { get; set; } = string.Empty;
    public string Ulica { get; set; } = string.Empty;
    public string NumerDomu { get; set; } = string.Empty;
    public string KodPocztowy { get; set; } = string.Empty;
    public string Miejscowosc { get; set; } = string.Empty;
    public string NazwaBanku { get; set; } = string.Empty;
    public string NumerKonta { get; set; } = string.Empty;
    public string PrefiksFaktur { get; set; } = "FV";
    public int DomyslnyTerminPlatnosci { get; set; } = 14;
    public string MiejsceWystawienia { get; set; } = string.Empty;
    public string DomyslnaFormaPlatnosci { get; set; } = "Przelew";
}