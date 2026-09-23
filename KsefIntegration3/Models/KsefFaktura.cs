using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkiControl.Models;

// Integracja z systemem KSEF - faktury (Ksef_Faktury)
[Table("Ksef_Faktury")]
public class KsefFaktura
{
    [Key]
    public Guid Guid { get; set; }

    public Guid Faktura_Guid { get; set; }

    /// <summary>
    /// Zawartość XML faktury. Mapowane na typ SQL 'xml'.
    /// </summary>
    [Column(TypeName = "xml")]
    public string? Faktura_Zawartosc { get; set; }

    [Required]
    [MaxLength(30)]
    public string Faktura_NIP { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Faktura_Numer { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Ksef_SessionReferenceNr { get; set; }

    [MaxLength(50)]
    public string? Ksef_InvoiceReferenceNr { get; set; }

    [Column(TypeName = "date")]
    public DateTime Faktura_Data { get; set; }

    [Column(TypeName = "date")]
    public DateTime? Ksef_Data { get; set; }

    public int Ksef_Status { get; set; }

    public int? Resp_ordinalNumber { get; set; }

    [MaxLength(256)]
    public string? Resp_invoiceNumber { get; set; }

    [MaxLength(50)]
    public string? Resp_ksefNumber { get; set; }

    [MaxLength(50)]
    public string? Resp_referenceNumber { get; set; }

    [MaxLength(50)]
    public string? Resp_invoiceHash { get; set; }

    [MaxLength(256)]
    public string? Resp_invoiceFileName { get; set; }

    [MaxLength(50)]
    public string? Resp_acquisitionDate { get; set; }

    [MaxLength(50)]
    public string? Resp_invoicingDate { get; set; }

    [MaxLength(50)]
    public string? Resp_permanentStorageDate { get; set; }

    public string? Resp_upoDownloadUrl { get; set; }

    [MaxLength(50)]
    public string? Resp_upoDownloadUrlExpirationDate { get; set; }

    [MaxLength(50)]
    public string? Resp_invoicingMode { get; set; }

    public int? Resp_statusCode { get; set; }

    public string? Resp_statusDescription { get; set; }

    public string? Resp_statusDetails { get; set; }

    public string? Resp_statusExtensions { get; set; }

    public DateTime CreateTimeStamp { get; set; }

    public DateTime? UpdateTimeStamp { get; set; }

    public string? Resp_qrUrl { get; set; }

    public string? Offline_Code { get; set; }

    public string? Offline_Cert { get; set; }
}
