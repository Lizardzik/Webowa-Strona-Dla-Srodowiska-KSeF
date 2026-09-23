using Microsoft.EntityFrameworkCore;
using SkiControl.Data;
using SkiControl.Models;

namespace SkiControl.Services;

public class DashboardService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public DashboardService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<SummaryModel> GetSummaryForDynamicPeriodAsync(string periodType, string detail, string yearStr)
    {
        if (!int.TryParse(yearStr, out int year)) year = DateTime.Now.Year;

        using var context = await _dbContextFactory.CreateDbContextAsync();
        var query = context.Faktury.Where(i => i.Data.Year == year && !i.is_Deleted);

        if (periodType == "Miesiąc")
        {
            int m = HelperService.GetMonthNumber(detail);
            query = query.Where(i => i.Data.Month == m);
        }
        else if (periodType == "Kwartał")
        {
            List<int> months = detail switch
            {
                "I kwartał" => new() { 1, 2, 3 },
                "II kwartał" => new() { 4, 5, 6 },
                "III kwartał" => new() { 7, 8, 9 },
                "IV kwartał" => new() { 10, 11, 12 },
                _ => new()
            };
            query = query.Where(i => months.Contains(i.Data.Month));
        }

        var list = await query.ToListAsync();
        await CalculateInvoicesTotalsAsync(list, context);

        return new SummaryModel
        {
            TotalSales = list.Sum(i => i.Kwota),
            InvoicesCount = list.Count,
            UnpaidInvoicesCount = 0,
            KsefSentCount = list.Count
        };
    }

    public async Task<List<MonthlySales>> GetChartDataAsync(string periodType, string detailOption, string yearStr)
    {
        if (!int.TryParse(yearStr, out int year)) year = DateTime.Now.Year;

        List<int> monthNumbers = periodType switch
        {
            "Półrocze" => detailOption == "I półrocze" ? Enumerable.Range(1, 6).ToList() : Enumerable.Range(7, 6).ToList(),
            "Kwartał" => detailOption switch
            {
                "I kwartał" => new() { 1, 2, 3 },
                "II kwartał" => new() { 4, 5, 6 },
                "III kwartał" => new() { 7, 8, 9 },
                "IV kwartał" => new() { 10, 11, 12 },
                _ => Enumerable.Range(1, 12).ToList()
            },
            _ => Enumerable.Range(1, 12).ToList()
        };

        using var context = await _dbContextFactory.CreateDbContextAsync();
        var invoices = await context.Faktury.Where(i => i.Data.Year == year && !i.is_Deleted).ToListAsync();
        await CalculateInvoicesTotalsAsync(invoices, context);

        var allMonths = HelperService.GetPolishMonths();

        return monthNumbers.Select(m => new MonthlySales
        {
            Month = allMonths[m - 1],
            Amount = (double)invoices.Where(i => i.Data.Month == m).Sum(i => i.Kwota)
        }).ToList();
    }

    private async Task CalculateInvoicesTotalsAsync(List<Faktura> invoices, AppDbContext context)
    {
        if (!invoices.Any()) return;

        var invoiceGuids = invoices.Select(i => i.Guid).ToList();

        var totals = await context.FakturyPozycje
            .Where(p => invoiceGuids.Contains(p.Faktura_Guid) && !p.is_Deleted)
            .GroupBy(p => p.Faktura_Guid)
            .Select(g => new {
                FakturaGuid = g.Key,
                Total = g.Sum(p => p.Pozycja_Wartosc_B)
            })
            .ToDictionaryAsync(x => x.FakturaGuid, x => x.Total);

        foreach (var inv in invoices)
        {
            if (totals.TryGetValue(inv.Guid, out decimal total))
            {
                inv.Kwota = total;
            }
        }
    }
}