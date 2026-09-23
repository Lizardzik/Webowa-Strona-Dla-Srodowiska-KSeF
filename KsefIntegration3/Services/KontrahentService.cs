using Microsoft.EntityFrameworkCore;
using SkiControl.Data;
using SkiControl.Models;

namespace SkiControl.Services;

public class KontrahentService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public KontrahentService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<Kontrahent>> GetContractorsAsync()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Kontrahenci.ToListAsync();
    }

    public async Task AddContractorAsync(Kontrahent contractor)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        contractor.Guid = Guid.NewGuid();
        contractor.Status = true;

        context.Kontrahenci.Add(contractor);
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateContractorAsync(Kontrahent updatedContractor)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var existing = await context.Kontrahenci.FirstOrDefaultAsync(c => c.Id == updatedContractor.Id);
        if (existing == null) return false;

        existing.Nazwa = updatedContractor.Nazwa;
        existing.Imie = updatedContractor.Imie;
        existing.Nazwisko = updatedContractor.Nazwisko;
        existing.NIP = updatedContractor.NIP;
        existing.Regon = updatedContractor.Regon;
        existing.Ulica = updatedContractor.Ulica;
        existing.NumerDomu = updatedContractor.NumerDomu;
        existing.Kod_Pocztowy = updatedContractor.Kod_Pocztowy;
        existing.Miejscowosc = updatedContractor.Miejscowosc;
        existing.Kod_Kraju = updatedContractor.Kod_Kraju;
        existing.Status = updatedContractor.Status;
        existing.Typ = updatedContractor.Typ;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteContractorAsync(int id)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var contractor = await context.Kontrahenci.FirstOrDefaultAsync(c => c.Id == id);
        if (contractor != null)
        {
            context.Kontrahenci.Remove(contractor);
            await context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<Kontrahent?> FetchFromGusAsync(string nip)
    {
        await Task.Delay(500);
        if (string.IsNullOrWhiteSpace(nip) || nip.Length < 10) return null;

        return new Kontrahent
        {
            NIP = nip,
            Regon = "123456789",
            Nazwa = $"Firma Testowa GUS",
            Ulica = "ul. Główna",
            NumerDomu = "100",
            Kod_Pocztowy = "00-001",
            Miejscowosc = "Warszawa",
            Kod_Kraju = "PL",
            Typ = "Firma",
            Kontrahent_Rodzaj = 1
        };
    }
}