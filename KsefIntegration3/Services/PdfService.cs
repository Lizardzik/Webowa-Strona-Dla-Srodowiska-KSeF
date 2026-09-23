using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using SkiControl.Data;
using SkiControl.Models;

namespace SkiControl.Services;

public class FakturaDokumentService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public FakturaDokumentService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
        _dbContextFactory = dbContextFactory;
    }

    public static bool IsCorrection(Faktura f) =>
        (f.Faktura_Numer != null && f.Faktura_Numer.Contains("FK")) || f.Faktura_Typ == 1 || f.Faktura_Typ == 2;

    public async Task<string> BuildInvoiceHtmlAsync(Faktura faktura, List<FakturaPozycja> positions)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var ksefInfo = await context.KsefFaktury
            .FirstOrDefaultAsync(k => k.Faktura_Guid == faktura.Guid);

        Faktura? baseInvoice = null;
        List<FakturaPozycja> basePositions = new();

        if (IsCorrection(faktura))
        {
            if (faktura.oGuid.HasValue)
            {
                baseInvoice = await context.Faktury.FirstOrDefaultAsync(f => f.Guid == faktura.oGuid.Value);
            }

            if (baseInvoice == null && !string.IsNullOrWhiteSpace(faktura.Faktura_Uwagi) && faktura.Faktura_Uwagi.Contains("Korekta do faktury nr "))
            {
                var part = faktura.Faktura_Uwagi.Split("Korekta do faktury nr ")[1].Split(' ')[0].Trim();
                baseInvoice = await context.Faktury.FirstOrDefaultAsync(f => f.Faktura_Numer == part);
            }

            if (baseInvoice != null)
            {
                basePositions = await context.FakturyPozycje
                    .Where(p => p.Faktura_Guid == baseInvoice.Guid && !p.is_Deleted)
                    .ToListAsync();
            }
        }

        await using var htmlRenderer = new HtmlRenderer(_serviceProvider, _loggerFactory);
        return await htmlRenderer.Dispatcher.InvokeAsync(() => BuildHtml(faktura, positions, ksefInfo, baseInvoice, basePositions));
    }

    public async Task<Stream> GeneratePdfAsync(Faktura faktura, List<FakturaPozycja> positions)
    {
        string html = await BuildInvoiceHtmlAsync(faktura, positions);

        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true
        });

        await using var page = await browser.NewPageAsync();
        await page.SetContentAsync(html);

        var pdfStream = await page.PdfStreamAsync(new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top = "10mm",
                Bottom = "10mm",
                Left = "10mm",
                Right = "10mm"
            }
        });

        await page.CloseAsync();
        return pdfStream;
    }

    private static string BuildHtml(Faktura faktura, List<FakturaPozycja> positions, KsefFaktura? ksefInfo, Faktura? baseInvoice, List<FakturaPozycja> basePositions)
    {
        string nabywcaNipLine = string.IsNullOrEmpty(faktura.Nabywca_NIP) ? "" : $"NIP: {faktura.Nabywca_NIP}<br/>";
        string sprzedawcaNipLine = string.IsNullOrEmpty(faktura.Sprzedawca_NIP) ? "" : $"NIP: {faktura.Sprzedawca_NIP}<br/>";
        string platnoscText = string.IsNullOrEmpty(faktura.Platnosc_Nazwa) ? "Przelew" : faktura.Platnosc_Nazwa;

        string terminPlatnosciText = faktura.Platnosc_Termin.HasValue
            ? faktura.Faktura_Data.AddDays(faktura.Platnosc_Termin.Value).ToString("dd.MM.yyyy")
            : faktura.Faktura_Data.ToString("dd.MM.yyyy");

        bool isCorrection = IsCorrection(faktura);
        string docTitle = isCorrection ? "Faktura VAT Korekta" : "Faktura VAT";

        string rootInvoiceNumber = baseInvoice?.Faktura_Numer ?? "";
        string correctionReason = faktura.Faktura_Uwagi ?? "";
        if (string.IsNullOrEmpty(rootInvoiceNumber) && !string.IsNullOrWhiteSpace(faktura.Faktura_Uwagi) && faktura.Faktura_Uwagi.Contains("Korekta do faktury nr "))
        {
            var part = faktura.Faktura_Uwagi.Split("Korekta do faktury nr ")[1];
            rootInvoiceNumber = part.Split(' ')[0].Trim();
        }

        string correctionInfoBlock = "";
        if (isCorrection)
        {
            correctionInfoBlock = $@"
            <div class='correction-box'>
                <div class='correction-title'>Przyczyna i dane dotyczące korekty:</div>
                <div>Dokument korygowany: <strong>{(string.IsNullOrEmpty(rootInvoiceNumber) ? "Faktura pierwotna" : $"Faktura VAT nr {rootInvoiceNumber}")}</strong></div>
                {(!string.IsNullOrWhiteSpace(correctionReason) ? $"<div>Przyczyna korekty: <strong>{correctionReason}</strong></div>" : "")}
            </div>";
        }

        bool hasDiffRecipient = !string.IsNullOrWhiteSpace(faktura.Odbiorca_Nazwa) &&
                                 !string.Equals(faktura.Odbiorca_Nazwa.Trim(), faktura.Nabywca_Nazwa?.Trim(), StringComparison.OrdinalIgnoreCase);

        string odbiorcaBlock = "";
        if (hasDiffRecipient)
        {
            string odbiorcaNipLine = string.IsNullOrWhiteSpace(faktura.Odbiorca_NIP) ? "" : $"NIP: {faktura.Odbiorca_NIP}<br/>";
            odbiorcaBlock = $@"
            <div class='box'>
                <div class='box-title'>Odbiorca / Płatnik:</div>
                <strong>{faktura.Odbiorca_Nazwa}</strong><br/>
                {odbiorcaNipLine}
                ul. {faktura.Odbiorca_Ulica} {faktura.Odbiorca_UlicaNumer}<br/>
                {faktura.Odbiorca_KodPocztowy} {faktura.Odbiorca_Miejscowosc}
            </div>";
        }

        string pozycjeTableHtml = (isCorrection && basePositions.Any())
            ? BuildCorrectionPositionsTableHtml(basePositions, positions)
            : BuildStandardPositionsTableHtml(positions);

        var vatGroups = positions
            .GroupBy(p => p.Pozycja_Stawka_VAT?.Replace("%", "").Trim() ?? "23")
            .Select(g => new
            {
                Stawka = g.Key,
                Netto = g.Sum(x => x.Pozycja_Wartosc_N),
                Vat = g.Sum(x => x.Pozycja_Wartosc_VAT),
                Brutto = g.Sum(x => x.Pozycja_Wartosc_B)
            })
            .OrderBy(g => g.Stawka)
            .ToList();

        decimal razemNetto = vatGroups.Sum(g => g.Netto);
        decimal razemVat = vatGroups.Sum(g => g.Vat);
        decimal razemBrutto = vatGroups.Sum(g => g.Brutto);

        string vatRows = "";
        foreach (var g in vatGroups)
        {
            vatRows += $@"
            <tr>
                <td class='text-center'>{g.Stawka}%</td>
                <td class='text-right'>{g.Netto:N2} zł</td>
                <td class='text-right'>{g.Vat:N2} zł</td>
                <td class='text-right'>{g.Brutto:N2} zł</td>
            </tr>";
        }

        string kwotaSlownieText = LiczbaSlownie(faktura.Kwota);

        // SEKCYJNA BUDOWA 2 KODÓW QR (ONLINE I OFFLINE) PONIŻEJ PODPISÓW
        string qrOnlineContent = ksefInfo?.Resp_qrUrl;
        string qrOfflineContent = ksefInfo?.Offline_Code;

        string qrOnlineBox = !string.IsNullOrEmpty(qrOnlineContent)
            ? $"<div class='qr-box'><img src='https://api.qrserver.com/v1/create-qr-code/?size=90x90&data={Uri.EscapeDataString(qrOnlineContent)}' style='width:75px;height:75px;'/><span class='qr-label'>QR KSeF Online</span></div>"
            : "<div class='qr-box placeholder'><span>QR KSeF<br/>ONLINE</span></div>";

        string qrOfflineBox = !string.IsNullOrEmpty(qrOfflineContent)
            ? $"<div class='qr-box'><img src='https://api.qrserver.com/v1/create-qr-code/?size=90x90&data={Uri.EscapeDataString(qrOfflineContent)}' style='width:75px;height:75px;'/><span class='qr-label'>QR KSeF Offline</span></div>"
            : "<div class='qr-box placeholder'><span>QR KSeF<br/>OFFLINE</span></div>";

        string html = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        * {{ box-sizing: border-box; }}
        body {{ 
            font-family: Arial, Helvetica, sans-serif; 
            color: #1a1a1a; 
            font-size: 11.5px; 
            margin: 0; 
            padding: 25px 30px;
            background: #fff;
        }}
        
        .header-grid {{ 
            display: flex; 
            justify-content: space-between; 
            align-items: flex-start; 
            margin-bottom: 14px; 
        }}
        .header-center {{ text-align: center; flex: 1; }}
        .header-center h1 {{ font-size: 24px; margin: 0; font-weight: bold; color: #0f172a; }}
        .header-center .doc-number {{ font-size: 13.5px; font-weight: bold; margin-top: 4px; color: #2563eb; }}
        .header-right {{ text-align: right; font-size: 11px; white-space: nowrap; line-height: 1.5; }}
        
        .divider {{ border: none; border-top: 1px solid #cbd5e1; margin: 12px 0 14px 0; }}
        
        .correction-box {{
            border: 1px solid #fde047;
            background-color: #fefce8;
            padding: 9px 12px;
            border-radius: 4px;
            margin-bottom: 14px;
            line-height: 1.5;
            font-size: 11px;
            color: #854d0e;
        }}
        .correction-title {{ font-weight: bold; margin-bottom: 2px; }}

        .parties {{ display: flex; justify-content: space-between; gap: 30px; margin-bottom: 12px; }}
        .box {{ width: 50%; line-height: 1.45; font-size: 11.5px; }}
        .box-title {{ font-weight: bold; margin-bottom: 4px; color: #334155; border-bottom: 1px solid #e2e8f0; padding-bottom: 2px; }}
        
        table.items-table {{ 
            width: 100%; 
            border-collapse: collapse; 
            margin-top: 14px; 
        }}
        table.items-table th, table.items-table td {{ 
            border: 1px solid #cbd5e1; 
            padding: 6px 8px; 
            font-size: 10.5px; 
        }}
        table.items-table thead th {{ 
            background-color: #f1f5f9; 
            color: #0f172a; 
            font-weight: 600; 
        }}
        .section-header {{
            background-color: #e2e8f0;
            font-weight: bold;
            font-size: 11px;
            color: #1e293b;
        }}
        .diff-row {{
            background-color: #fef2f2;
            font-weight: bold;
        }}
        .text-right {{ text-align: right; }}
        .text-center {{ text-align: center; }}
        
        .vat-wrapper {{ 
            display: flex; 
            justify-content: flex-end;
            margin-top: 14px; 
        }}
        .vat-container {{ width: 48%; }}
        .vat-container .title {{ 
            font-weight: bold; 
            font-size: 11px; 
            margin-bottom: 4px; 
            text-align: center; 
        }}
        table.vat-table {{ 
            width: 100%; 
            border-collapse: collapse; 
        }}
        table.vat-table th, table.vat-table td {{ 
            border: 1px solid #cbd5e1; 
            padding: 5px 8px; 
            font-size: 10.5px; 
        }}
        table.vat-table thead th {{ 
            background-color: #f1f5f9; 
            font-weight: 600; 
        }}
        
        .summary {{ 
            margin-top: 18px; 
            line-height: 1.55; 
            font-size: 11.5px; 
        }}
        .summary .total {{ 
            font-size: 14px; 
            font-weight: bold; 
            color: #0f172a;
            margin-bottom: 4px; 
        }}
        
        .footer-signatures {{ 
            margin-top: 45px; 
            display: flex; 
            justify-content: space-between; 
            margin-bottom: 25px;
        }}
        .footer-signatures div {{ 
            width: 40%; 
            text-align: center; 
            border-top: 1px dotted #94a3b8; 
            padding-top: 5px; 
            font-size: 9.5px; 
            color: #64748b; 
        }}

        .ksef-bottom-section {{
            border: 1px solid #cbd5e1;
            background-color: #f8fafc;
            border-radius: 6px;
            padding: 10px 14px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            font-size: 10.5px;
            line-height: 1.5;
            margin-top: 15px;
        }}
        .ksef-info-col {{ flex: 1; margin-right: 15px; }}
        .ksef-title {{ font-weight: bold; color: #1e40af; font-size: 11px; margin-bottom: 4px; }}
        
        .qr-codes-col {{ display: flex; gap: 12px; align-items: center; }}
        .qr-box {{
            width: 82px;
            height: 82px;
            border: 1px solid #cbd5e1;
            background: #fff;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            padding: 2px;
            text-align: center;
            border-radius: 4px;
        }}
        .qr-box.placeholder {{
            border: 1px dashed #94a3b8;
            background: #f1f5f9;
            color: #64748b;
            font-size: 8.5px;
            font-weight: bold;
        }}
        .qr-label {{ font-size: 7.5px; color: #475569; margin-top: 2px; font-weight: 600; }}
    </style>
</head>
<body>
    <div class='header-grid'>
        <div style='width: 15%;'></div>
        <div class='header-center'>
            <h1>{docTitle}</h1>
            <div class='doc-number'>Numer: {faktura.Faktura_Numer}</div>
        </div>
        <div class='header-right'>
            Data wystawienia: <strong>{faktura.Faktura_Data:dd.MM.yyyy}</strong><br/>
            Data usługi/sprzedaży: <strong>{faktura.Usluga_Data:dd.MM.yyyy}</strong><br/>
            Miejsce wystawienia: <strong>{(string.IsNullOrEmpty(faktura.Faktura_MiejsceWystawienia) ? faktura.Sprzedawca_Miejscowosc : faktura.Faktura_MiejsceWystawienia)}</strong>
        </div>
    </div>

    {correctionInfoBlock}

    <hr class='divider' />

    <div class='parties'>
        <div class='box'>
            <div class='box-title'>Sprzedawca:</div>
            <strong>{faktura.Sprzedawca_Nazwa}</strong><br/>
            {faktura.Sprzedawca_KodPocztowy} {faktura.Sprzedawca_Miejscowosc}<br/>
            ul. {faktura.Sprzedawca_Ulica} {faktura.Sprzedawca_Numer}<br/>
            {sprzedawcaNipLine}
        </div>
        <div class='box'>
            <div class='box-title'>Nabywca:</div>
            <strong>{faktura.NabywcaDisplayName}</strong><br/>
            {faktura.Nabywca_KodPocztowy} {faktura.Nabywca_Miejscowosc}<br/>
            ul. {faktura.Nabywca_Ulica} {faktura.Nabywca_UlicaNumer}<br/>
            {nabywcaNipLine}
        </div>
    </div>

    {odbiorcaBlock}

    {pozycjeTableHtml}

    <div class='vat-wrapper'>
        <div class='vat-container'>
            <div class='title'>Podsumowanie stawek VAT</div>
            <table class='vat-table'>
                <thead>
                    <tr>
                        <th class='text-center'>Stawka</th>
                        <th class='text-right'>Netto</th>
                        <th class='text-right'>VAT</th>
                        <th class='text-right'>Brutto</th>
                    </tr>
                </thead>
                <tbody>
                    {vatRows}
                    <tr style='font-weight: bold; background-color: #f1f5f9;'>
                        <td class='text-center'>Razem:</td>
                        <td class='text-right'>{razemNetto:N2} zł</td>
                        <td class='text-right'>{razemVat:N2} zł</td>
                        <td class='text-right'>{razemBrutto:N2} zł</td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>

    <div class='summary'>
        <div class='total'>Do zapłaty / Zwrotu: {faktura.Kwota:N2} PLN</div>
        <div>Słownie: <strong>{kwotaSlownieText}</strong></div>
        <div>Sposób płatności: <strong>{platnoscText}</strong></div>
        <div>Termin płatności: <strong>{terminPlatnosciText}</strong></div>
    </div>

    <div class='footer-signatures'>
        <div>Osoba upoważniona do wystawienia</div>
        <div>Osoba upoważniona do odbioru</div>
    </div>

    <!-- SEKCJA DOLNA: METADANE KSEF ORAZ 2 KODY QR OBOK SIEBIE -->
    <div class='ksef-bottom-section'>
        <div class='ksef-info-col'>
            <div class='ksef-title'>Informacje Krajowego Systemu e-Faktur (KSeF)</div>
            <div>Numer KSeF: <strong>{(string.IsNullOrEmpty(ksefInfo?.Resp_ksefNumber) ? "Oczekuje na zarejestrowanie" : ksefInfo.Resp_ksefNumber)}</strong></div>
            <div>Data przyjęcia / rejestracji: <strong>{(string.IsNullOrEmpty(ksefInfo?.Resp_acquisitionDate) ? faktura.Faktura_Data.ToString("dd.MM.yyyy") : ksefInfo.Resp_acquisitionDate)}</strong></div>
            <div>Numer referencyjny sesji: <strong>{(string.IsNullOrEmpty(ksefInfo?.Resp_referenceNumber) ? "-" : ksefInfo.Resp_referenceNumber)}</strong></div>
        </div>
        <div class='qr-codes-col'>
            {qrOnlineBox}
            {qrOfflineBox}
        </div>
    </div>
</body>
</html>";

        return html;
    }

    private static string BuildStandardPositionsTableHtml(List<FakturaPozycja> positions)
    {
        string rows = "";
        int lp = 1;
        foreach (var p in positions)
        {
            rows += $@"
            <tr>
                <td class='text-center'>{lp}</td>
                <td>{p.Pozycja_Nazwa}</td>
                <td class='text-center'>{p.Pozycja_Ilosc}</td>
                <td class='text-right'>{p.Pozycja_Cena_Netto:N2} zł</td>
                <td class='text-center'>{p.Pozycja_Stawka_VAT?.Replace("%", "")}%</td>
                <td class='text-right'>{p.Pozycja_Wartosc_N:N2} zł</td>
                <td class='text-right'><strong>{p.Pozycja_Wartosc_B:N2} zł</strong></td>
            </tr>";
            lp++;
        }

        return $@"
        <table class='items-table'>
            <thead>
                <tr>
                    <th style='width: 30px;' class='text-center'>Lp</th>
                    <th>Nazwa towaru / usługi</th>
                    <th class='text-center' style='width: 45px;'>Ilość</th>
                    <th class='text-right' style='width: 80px;'>Cena netto</th>
                    <th class='text-center' style='width: 50px;'>VAT</th>
                    <th class='text-right' style='width: 80px;'>Wart. netto</th>
                    <th class='text-right' style='width: 85px;'>Wart. brutto</th>
                </tr>
            </thead>
            <tbody>
                {rows}
            </tbody>
        </table>";
    }

    private static string BuildCorrectionPositionsTableHtml(List<FakturaPozycja> basePositions, List<FakturaPozycja> positions)
    {
        string rows = "";

        rows += "<tr><td colspan='7' class='section-header'>PRZED KOREKTĄ</td></tr>";
        int lp = 1;
        foreach (var p in basePositions)
        {
            rows += $@"
            <tr>
                <td class='text-center'>{lp}</td>
                <td>{p.Pozycja_Nazwa}</td>
                <td class='text-center'>{p.Pozycja_Ilosc}</td>
                <td class='text-right'>{p.Pozycja_Cena_Netto:N2} zł</td>
                <td class='text-center'>{p.Pozycja_Stawka_VAT?.Replace("%", "")}%</td>
                <td class='text-right'>{p.Pozycja_Wartosc_N:N2} zł</td>
                <td class='text-right'>{p.Pozycja_Wartosc_B:N2} zł</td>
            </tr>";
            lp++;
        }

        rows += "<tr><td colspan='7' class='section-header'>PO KOREKCIE</td></tr>";
        lp = 1;
        foreach (var p in positions)
        {
            rows += $@"
            <tr>
                <td class='text-center'>{lp}</td>
                <td>{p.Pozycja_Nazwa}</td>
                <td class='text-center'>{p.Pozycja_Ilosc}</td>
                <td class='text-right'>{p.Pozycja_Cena_Netto:N2} zł</td>
                <td class='text-center'>{p.Pozycja_Stawka_VAT?.Replace("%", "")}%</td>
                <td class='text-right'>{p.Pozycja_Wartosc_N:N2} zł</td>
                <td class='text-right'>{p.Pozycja_Wartosc_B:N2} zł</td>
            </tr>";
            lp++;
        }

        decimal diffNetto = positions.Sum(p => p.Pozycja_Wartosc_N) - basePositions.Sum(p => p.Pozycja_Wartosc_N);
        decimal diffBrutto = positions.Sum(p => p.Pozycja_Wartosc_B) - basePositions.Sum(p => p.Pozycja_Wartosc_B);

        rows += $@"
        <tr class='diff-row'>
            <td colspan='5' class='text-right'><strong>RÓŻNICA / KOREKTA (PO - PRZED):</strong></td>
            <td class='text-right'><strong>{diffNetto:N2} zł</strong></td>
            <td class='text-right'><strong>{diffBrutto:N2} zł</strong></td>
        </tr>";

        return $@"
        <table class='items-table'>
            <thead>
                <tr>
                    <th style='width: 30px;' class='text-center'>Lp</th>
                    <th>Nazwa towaru / usługi</th>
                    <th class='text-center' style='width: 45px;'>Ilość</th>
                    <th class='text-right' style='width: 80px;'>Cena netto</th>
                    <th class='text-center' style='width: 50px;'>VAT</th>
                    <th class='text-right' style='width: 80px;'>Wart. netto</th>
                    <th class='text-right' style='width: 85px;'>Wart. brutto</th>
                </tr>
            </thead>
            <tbody>
                {rows}
            </tbody>
        </table>";
    }

    private static readonly string[] Jednosci = { "", "jeden", "dwa", "trzy", "cztery", "pięć", "sześć", "siedem", "osiem", "dziewięć" };
    private static readonly string[] Nascie = { "dziesięć", "jedenaście", "dwanaście", "trzynaście", "czternaście", "piętnaście", "szesnaście", "siedemnaście", "osiemnaście", "dziewiętnaście" };
    private static readonly string[] Dziesiatki = { "", "", "dwadzieścia", "trzydzieści", "czterdzieści", "pięćdziesiąt", "sześćdziesiąt", "siedemdziesiąt", "osiemdziesiąt", "dziewięćdziesiąt" };
    private static readonly string[] Setki = { "", "sto", "dwieście", "trzysta", "czterysta", "pięćset", "sześćset", "siedemset", "osiemset", "dziewięćset" };

    private static string TrzyCyfrowoSlownie(int liczba)
    {
        var czesci = new List<string>();
        int setki = liczba / 100;
        int reszta = liczba % 100;

        if (setki > 0) czesci.Add(Setki[setki]);

        if (reszta >= 10 && reszta < 20)
        {
            czesci.Add(Nascie[reszta - 10]);
        }
        else
        {
            int dziesiatki = reszta / 10;
            int jednosci = reszta % 10;
            if (dziesiatki > 0) czesci.Add(Dziesiatki[dziesiatki]);
            if (jednosci > 0) czesci.Add(Jednosci[jednosci]);
        }

        return string.Join(" ", czesci);
    }

    private static string OdmianaZlotych(long liczba)
    {
        if (liczba == 1) return "złoty";
        long ostatniaCyfra = liczba % 10;
        long ostatnieDwie = liczba % 100;

        if (ostatnieDwie >= 12 && ostatnieDwie <= 14) return "złotych";
        if (ostatniaCyfra >= 2 && ostatniaCyfra <= 4) return "złote";
        return "złotych";
    }

    private static string OdmianaGroszy(int liczba)
    {
        if (liczba == 1) return "grosz";
        int ostatniaCyfra = liczba % 10;
        int ostatnieDwie = liczba % 100;

        if (ostatnieDwie >= 12 && ostatnieDwie <= 14) return "groszy";
        if (ostatniaCyfra >= 2 && ostatniaCyfra <= 4) return "grosze";
        return "grosze";
    }

    private static string OdmianaTysiecy(int liczbaTysiecy)
    {
        if (liczbaTysiecy == 1) return "tysiąc";
        int ostatniaCyfra = liczbaTysiecy % 10;
        int ostatnieDwie = liczbaTysiecy % 100;

        if (ostatnieDwie >= 12 && ostatnieDwie <= 14) return "tysięcy";
        if (ostatniaCyfra >= 2 && ostatniaCyfra <= 4) return "tysiące";
        return "tysięcy";
    }

    public static string LiczbaSlownie(decimal kwota)
    {
        kwota = Math.Abs(kwota);
        long zlote = (long)Math.Floor(kwota);
        int grosze = (int)Math.Round((kwota - zlote) * 100, MidpointRounding.AwayFromZero);

        var czesci = new List<string>();

        if (zlote == 0)
        {
            czesci.Add("zero");
        }
        else
        {
            int tysiace = (int)(zlote / 1000);
            int reszta = (int)(zlote % 1000);

            if (tysiace > 0)
            {
                if (tysiace == 1)
                    czesci.Add("tysiąc");
                else
                    czesci.Add($"{TrzyCyfrowoSlownie(tysiace)} {OdmianaTysiecy(tysiace)}");
            }

            if (reszta > 0)
            {
                czesci.Add(TrzyCyfrowoSlownie(reszta));
            }
        }

        czesci.Add(OdmianaZlotych(zlote));

        if (grosze > 0)
        {
            czesci.Add(TrzyCyfrowoSlownie(grosze));
            czesci.Add(OdmianaGroszy(grosze));
        }

        return string.Join(" ", czesci);
    }
}