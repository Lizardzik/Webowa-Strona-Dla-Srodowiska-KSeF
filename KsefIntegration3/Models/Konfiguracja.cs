using System.ComponentModel.DataAnnotations;

namespace KsefIntegration3.Models
{
    public class Konfiguracja
    {
        [Key]
        public int Id { get; set; }
        public string Parametr_Nazwa { get; set; } = string.Empty;
        public string Parametr_Wartosc { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; } = null;
    }
}
