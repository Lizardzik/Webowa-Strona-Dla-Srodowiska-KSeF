using Microsoft.EntityFrameworkCore;
using SkiControl.Data;
using SkiControl.Models;
using KsefIntegration3.Services;
using KsefIntegration3.XmlConverter;
using KsefIntegration_ModelLibrary.Api;

namespace SkiControl.Services;

public class FakturaService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly ConfigurationService _configurationService;

    public FakturaService(IDbContextFactory<AppDbContext> dbContextFactory, ConfigurationService configurationService)
    {
        _dbContextFactory = dbContextFactory;
        _configurationService = configurationService;
    }

    public async Task<List<Faktura>> GetInvoicesAsync()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var invoicesWithTotals = await context.Faktury
            .Where(f => !f.is_Deleted)
            .OrderByDescending(f => f.TimeStamp)
            .ThenByDescending(f => f.Id)
            .Select(f => new
            {
                Faktura = f,
                KwotaBrutto = context.FakturyPozycje
                    .Where(p => p.Faktura_Guid == f.Guid && !p.is_Deleted)
                    .Sum(p => (decimal?)p.Pozycja_Wartosc_B) ?? 0m
            })
            .ToListAsync();

        foreach (var item in invoicesWithTotals)
        {
            item.Faktura.Kwota = item.KwotaBrutto;
        }

        return invoicesWithTotals.Select(i => i.Faktura).ToList();
    }

    public async Task<int> GetInvoiceCountForContractorAsync(string? nip)
    {
        if (string.IsNullOrEmpty(nip)) return 0;
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Faktury.CountAsync(i => i.Nabywca_NIP == nip && !i.is_Deleted);
    }

    public async Task<List<Faktura>> GetRecentInvoicesAsync(int count = 5)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var invoicesWithTotals = await context.Faktury
            .Where(f => !f.is_Deleted)
            .OrderByDescending(f => f.TimeStamp)
            .ThenByDescending(f => f.Id)
            .Take(count)
            .Select(f => new
            {
                Faktura = f,
                KwotaBrutto = context.FakturyPozycje
                    .Where(p => p.Faktura_Guid == f.Guid && !p.is_Deleted)
                    .Sum(p => (decimal?)p.Pozycja_Wartosc_B) ?? 0m
            })
            .ToListAsync();

        foreach (var item in invoicesWithTotals)
        {
            item.Faktura.Kwota = item.KwotaBrutto;
        }

        return invoicesWithTotals.Select(i => i.Faktura).ToList();
    }

    public async Task<bool> AddInvoiceAsync(Faktura invoice, ApiResponseDetails? apiResponseDetails, string? xml = null)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        if (invoice.Guid == Guid.Empty)
        {
            invoice.Guid = Guid.NewGuid();
        }

        invoice.Data = invoice.Usluga_Data != default ? invoice.Usluga_Data : invoice.Faktura_Data;
        invoice.TimeStamp = DateTime.Now;
        invoice.Urzadzenie_Id = "asdihjadjs231";
        invoice.is_Deleted = false;

        var nabywca = new FakturaPodmiot
        {
            Guid = Guid.NewGuid(),
            Faktura_Guid = invoice.Guid,
            Faktura_Numer = invoice.Faktura_Numer,
            Nazwa = invoice.Nabywca_Nazwa,
            NIP = invoice.Nabywca_NIP,
            Ulica = invoice.Nabywca_Ulica,
            Numer_Domu = invoice.Nabywca_UlicaNumer,
            Kod_Pocztowy = invoice.Nabywca_KodPocztowy,
            Miejscowosc = invoice.Nabywca_Miejscowosc,
            Kod_Kraju = "PL",
            Typ_Na_Fa = 2,
            is_Deleted = false,
            Timestamp = DateTime.Now
        };
        context.FakturyPodmioty.Add(nabywca);

        var odbiorca = new FakturaPodmiot
        {
            Guid = Guid.NewGuid(),
            Faktura_Guid = invoice.Guid,
            Faktura_Numer = invoice.Faktura_Numer,
            Nazwa = invoice.Odbiorca_Nazwa,
            NIP = invoice.Odbiorca_NIP,
            Ulica = invoice.Odbiorca_Ulica,
            Numer_Domu = invoice.Odbiorca_UlicaNumer,
            Kod_Pocztowy = invoice.Odbiorca_KodPocztowy,
            Miejscowosc = invoice.Odbiorca_Miejscowosc,
            Kod_Kraju = "PL",
            Typ_Na_Fa = 3,
            is_Deleted = false,
            Timestamp = DateTime.Now
        };
        context.FakturyPodmioty.Add(odbiorca);

        context.Faktury.Add(invoice);

        if (apiResponseDetails == null || apiResponseDetails.SingleInvoice == null)
        {
            return await context.SaveChangesAsync() > 0;
        }

        string? benjamin = null;
        KsefFaktura ksefFaktura = new KsefFaktura
        {
            UpdateTimeStamp = DateTime.Now,
            CreateTimeStamp = DateTime.Now,
            Resp_acquisitionDate = apiResponseDetails.SingleInvoice.AcquisitionDate.ToString(),
            Resp_invoiceFileName = apiResponseDetails.SingleInvoice.InvoiceFileName,
            Resp_invoiceHash = apiResponseDetails.SingleInvoice.InvoiceHash,
            Resp_invoiceNumber = apiResponseDetails.SingleInvoice.InvoiceNumber,
            Resp_invoicingDate = apiResponseDetails.SingleInvoice.InvoicingDate.ToString(),
            Resp_ksefNumber = apiResponseDetails.SingleInvoice.KsefNumber,
            Resp_invoicingMode = apiResponseDetails.SingleInvoice.InvoicingMode.ToString(),
            Resp_ordinalNumber = apiResponseDetails.SingleInvoice.OrdinalNumber,
            Resp_permanentStorageDate = apiResponseDetails.SingleInvoice.PermanentStorageDate.ToString(),
            Resp_qrUrl = apiResponseDetails.SingleInvoice.qrInvoiceUrl,
            Resp_referenceNumber = apiResponseDetails.SingleInvoice.ReferenceNumber,
            Resp_statusCode = apiResponseDetails.SingleInvoice.Status?.Code,
            Resp_statusDescription = apiResponseDetails.SingleInvoice.Status?.Description,
            Resp_statusDetails = benjamin,
            Offline_Code = apiResponseDetails.SingleInvoice.qrInvoiceUrl,

            Faktura_Data = invoice.Faktura_Data,
            Faktura_Guid = invoice.Guid,
            Faktura_NIP = invoice.Sprzedawca_NIP,
            Faktura_Numer = invoice.Faktura_Numer,
            Faktura_Zawartosc = xml,
            Guid = Guid.NewGuid(),
            Ksef_Data = invoice.Faktura_Data,
            Ksef_InvoiceReferenceNr = apiResponseDetails.SingleInvoice.ReferenceNumber,
            Ksef_SessionReferenceNr = apiResponseDetails.SessionReferenceNumber,
            Ksef_Status = ((int?)apiResponseDetails.KsefCode) ?? 0,
            Offline_Cert = benjamin,
            Resp_statusExtensions = benjamin,
            Resp_upoDownloadUrl = apiResponseDetails.SingleInvoice.UpoDownloadUrl?.ToString(),
            Resp_upoDownloadUrlExpirationDate = apiResponseDetails.SingleInvoice.UpoDownloadUrlExpirationDate?.ToString()
        };

        context.KsefFaktury.Add(ksefFaktura);

        return await context.SaveChangesAsync() > 0;
    }
    public async Task<bool> ResendToKsefAsync(Guid invoiceGuid)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var invoice = await context.Faktury.FirstOrDefaultAsync(f => f.Guid == invoiceGuid && !f.is_Deleted);
        if (invoice == null) return false;

        var positions = await GetInvoicePositionsAsync(invoiceGuid);

        await Task.Delay(500);

        var existingKsef = await context.KsefFaktury.FirstOrDefaultAsync(k => k.Faktura_Guid == invoiceGuid);
        if (existingKsef == null)
        {
            existingKsef = new KsefFaktura
            {
                Guid = Guid.NewGuid(),
                Faktura_Guid = invoice.Guid,
                Faktura_Numer = invoice.Faktura_Numer,
                Faktura_NIP = invoice.Sprzedawca_NIP,
                Faktura_Data = invoice.Faktura_Data,
                CreateTimeStamp = DateTime.Now,
                UpdateTimeStamp = DateTime.Now,
                Ksef_Status = 200
            };
            context.KsefFaktury.Add(existingKsef);
        }

        existingKsef.UpdateTimeStamp = DateTime.Now;
        existingKsef.Resp_ksefNumber = $"{invoice.Sprzedawca_NIP}-{DateTime.Now:yyyyMMdd}-TEST-{Random.Shared.Next(100000, 999999)}";
        existingKsef.Resp_acquisitionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

        return await context.SaveChangesAsync() > 0;
    }
    public async Task<List<FakturaPozycja>> GetInvoicePositionsAsync(Guid invoiceGuid)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.FakturyPozycje
            .Where(p => p.Faktura_Guid == invoiceGuid && !p.is_Deleted)
            .ToListAsync();
    }
}