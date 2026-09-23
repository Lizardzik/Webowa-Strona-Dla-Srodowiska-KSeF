using KsefIntegration3.Models;
using Microsoft.EntityFrameworkCore;
using SkiControl.Models;

namespace SkiControl.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<Konfiguracja> KonfiguracjaParametry { get; set; } = null!;
    public DbSet<Kontrahent> Kontrahenci { get; set; } = null!;
    public DbSet<Faktura> Faktury { get; set; } = null!;
    public DbSet<FakturaPozycja> FakturyPozycje { get; set; } = null!;
    public DbSet<Paragon> Paragony { get; set; } = null!;
    public DbSet<Numeracja> Numeracja { get; set; } = null!;
    public DbSet<KsefFaktura> KsefFaktury { get; set; } = null!;
    public DbSet<ParagonPozycja> ParagonyPozycje { get; set; } = null!;
    public DbSet<FakturaPodmiot> FakturyPodmioty { get; set; } = null!;
    public DbSet<MyCompany> MojaFirma { get; set; } = null!;
    public DbSet<Towary> Towary { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Kontrahent>(entity =>
        {
            entity.ToTable("Kontrahenci");
            entity.HasKey(e => e.Id);

            entity.Ignore(e => e.DisplayName);
            entity.Ignore(e => e.FullAddress);
        });

        modelBuilder.Entity<Faktura>(entity =>
        {
            entity.ToTable("Faktury");
            entity.HasKey(e => e.Guid);
        });

        modelBuilder.Entity<FakturaPozycja>(entity =>
        {
            entity.ToTable("Faktury_Pozycje");
            entity.HasKey(e => e.Guid);
        });

        modelBuilder.Entity<Paragon>(entity =>
        {
            entity.ToTable("Paragony");
            entity.HasKey(e => e.Guid);
        });

        modelBuilder.Entity<Numeracja>(entity =>
        {
            entity.ToTable("Numeracja");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<KsefFaktura>(entity =>
        {
            entity.ToTable("Ksef_Faktury");
            entity.HasKey(e => e.Guid);
        });

        modelBuilder.Entity<ParagonPozycja>(entity =>
        {
            entity.ToTable("Paragony_Pozycje");
            entity.HasKey(e => e.Guid);
        });

        modelBuilder.Entity<FakturaPodmiot>(entity =>
        {
            entity.ToTable("Faktury_Podmioty");
            entity.HasKey(e => e.Guid);
        });

        modelBuilder.Entity<MyCompany>(entity =>
        {
            entity.ToTable("MojaFirma");
            entity.HasKey(e => e.Id);
        });
    }
}