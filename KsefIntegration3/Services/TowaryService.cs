using Microsoft.EntityFrameworkCore;
using SkiControl.Data;
using SkiControl.Models;

namespace SkiControl.Services;

public class TowaryService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public TowaryService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<Towary>> GetDictionaryItemsAsync()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Towary
            .Where(t => t.Status)
            .OrderBy(t => t.Nazwa)
            .ToListAsync();
    }

    public async Task AddDictionaryItemAsync(Towary item)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        item.Guid = Guid.NewGuid();
        item.Status = true;

        context.Towary.Add(item);
        await context.SaveChangesAsync();
    }

    public async Task UpdateDictionaryItemAsync(Towary item)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var existing = await context.Towary.FirstOrDefaultAsync(t => t.Id == item.Id);
        if (existing != null)
        {
            existing.Nazwa = item.Nazwa;
            existing.Cena_Netto = item.Cena_Netto;
            existing.Cena_Brutto = item.Cena_Brutto;
            existing.Stawka_VAT = item.Stawka_VAT;
            existing.Kod_Towaru = item.Kod_Towaru;
            existing.Jednostka_Miary = item.Jednostka_Miary;

            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteDictionaryItemAsync(int id)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var existing = await context.Towary.FirstOrDefaultAsync(t => t.Id == id);
        if (existing != null)
        {
            context.Towary.Remove(existing);
            await context.SaveChangesAsync();
        }
    }
}