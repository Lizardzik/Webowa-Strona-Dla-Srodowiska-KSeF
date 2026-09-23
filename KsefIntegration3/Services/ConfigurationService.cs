using KsefIntegration3.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SkiControl.Data;
using SkiControl.Models;

namespace KsefIntegration3.Services
{
    public class ConfigurationData
    {
        public string Sprzedawca_Nazwa { get; set; } = string.Empty;
        public string Sprzedawca_NIP { get; set; } = string.Empty;
        public string Sprzedawca_Ulica { get; set; } = string.Empty;
        public string Sprzedawca_UlicaNr { get; set; } = string.Empty;
        public string Sprzedawca_Miejscowosc { get; set; } = string.Empty;
        public string Sprzedawca_KodPocztowy { get; set; } = string.Empty;
        public string Sprzedawca_BankNazwa { get; set; } = string.Empty;
        public string Sprzedawca_BankKonto { get; set; } = string.Empty;
        public string Faktura_MiejsceWystawienia { get; set; } = string.Empty;
        public string Wystawca_Nazwa { get; set; } = string.Empty;
        public string Wystawca_Miejscowosc { get; set; } = string.Empty;
        public string Wystawca_Ulica { get; set; } = string.Empty;
        public string Wystawca_UlicaNr { get; set; } = string.Empty;
        public string Sprzedawca_Numer_BDO { get; set; } = string.Empty;
        public string Wystawca_KodPocztowy { get; set; } = string.Empty;
        public string KSEF_Url { get; set; } = string.Empty;
        public string KSEF_Mode { get; set; } = string.Empty;
        public string Wystawca_NIP { get; set; } = string.Empty;
        public string Wystawca_Ksef_Id { get; set; } = string.Empty;
    }

    public class ConfigurationService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly IConfiguration _configuration;

        public ConfigurationService(IDbContextFactory<AppDbContext> dbContextFactory, IConfiguration configuration)
        {
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
        }

        public ConfigurationData ConfigurationData { get; private set; } = new ConfigurationData();

        private string GetActiveUrzadzenieId()
        {
            string? devId = _configuration["DeviceSettings:UrzadzenieId"];
            return !string.IsNullOrWhiteSpace(devId) ? devId : "asdihjadjs231";
        }

        public async Task<ConfigurationData> GetConfigurationDataAsync()
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var configData = await context.KonfiguracjaParametry.AsNoTracking().ToListAsync();

            ConfigurationData.Sprzedawca_Nazwa = GetVal(configData, "Sprzedawca_Nazwa");
            ConfigurationData.Sprzedawca_NIP = GetVal(configData, "Sprzedawca_NIP");
            ConfigurationData.Sprzedawca_Ulica = GetVal(configData, "Sprzedawca_Ulica");
            ConfigurationData.Sprzedawca_UlicaNr = GetVal(configData, "Sprzedawca_UlicaNr");
            ConfigurationData.Sprzedawca_Miejscowosc = GetVal(configData, "Sprzedawca_Miejscowosc");
            ConfigurationData.Sprzedawca_KodPocztowy = GetVal(configData, "Sprzedawca_KodPocztowy");
            ConfigurationData.Sprzedawca_BankNazwa = GetVal(configData, "Sprzedawca_BankNazwa");
            ConfigurationData.Sprzedawca_BankKonto = GetVal(configData, "Sprzedawca_BankKonto");
            ConfigurationData.Faktura_MiejsceWystawienia = GetVal(configData, "Faktura_MiejsceWystawienia");
            ConfigurationData.Wystawca_Nazwa = GetVal(configData, "Wystawca_Nazwa");
            ConfigurationData.Wystawca_Miejscowosc = GetVal(configData, "Wystawca_Miejscowosc");
            ConfigurationData.Wystawca_Ulica = GetVal(configData, "Wystawca_Ulica");
            ConfigurationData.Wystawca_UlicaNr = GetVal(configData, "Wystawca_UlicaNr");
            ConfigurationData.Sprzedawca_Numer_BDO = GetVal(configData, "Sprzedawca_Numer_BDO");
            ConfigurationData.Wystawca_KodPocztowy = GetVal(configData, "Wystawca_KodPocztowy");
            ConfigurationData.KSEF_Url = GetVal(configData, "KSEF_Url");
            ConfigurationData.KSEF_Mode = GetVal(configData, "KSEF_Mode");
            ConfigurationData.Wystawca_NIP = GetVal(configData, "Wystawca_NIP");
            ConfigurationData.Wystawca_Ksef_Id = GetVal(configData, "Wystawca_Ksef_Id");

            return ConfigurationData;
        }

        public async Task SaveConfigurationDataAsync(ConfigurationData data)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var existing = await context.KonfiguracjaParametry.ToListAsync();

            void Upsert(string name, string val)
            {
                var item = existing.FirstOrDefault(c => c.Parametr_Nazwa == name);
                if (item != null)
                {
                    item.Parametr_Wartosc = val ?? string.Empty;
                    item.Timestamp = DateTime.Now;
                    context.KonfiguracjaParametry.Update(item);
                }
                else
                {
                    context.KonfiguracjaParametry.Add(new Konfiguracja
                    {
                        Parametr_Nazwa = name,
                        Parametr_Wartosc = val ?? string.Empty,
                        Timestamp = DateTime.Now
                    });
                }
            }

            Upsert("Sprzedawca_Nazwa", data.Sprzedawca_Nazwa);
            Upsert("Sprzedawca_NIP", data.Sprzedawca_NIP);
            Upsert("Sprzedawca_Ulica", data.Sprzedawca_Ulica);
            Upsert("Sprzedawca_UlicaNr", data.Sprzedawca_UlicaNr);
            Upsert("Sprzedawca_Miejscowosc", data.Sprzedawca_Miejscowosc);
            Upsert("Sprzedawca_KodPocztowy", data.Sprzedawca_KodPocztowy);
            Upsert("Sprzedawca_BankNazwa", data.Sprzedawca_BankNazwa);
            Upsert("Sprzedawca_BankKonto", data.Sprzedawca_BankKonto);
            Upsert("Faktura_MiejsceWystawienia", data.Faktura_MiejsceWystawienia);
            Upsert("Wystawca_Nazwa", data.Wystawca_Nazwa);
            Upsert("Wystawca_Miejscowosc", data.Wystawca_Miejscowosc);
            Upsert("Wystawca_Ulica", data.Wystawca_Ulica);
            Upsert("Wystawca_UlicaNr", data.Wystawca_UlicaNr);
            Upsert("Sprzedawca_Numer_BDO", data.Sprzedawca_Numer_BDO);
            Upsert("Wystawca_KodPocztowy", data.Wystawca_KodPocztowy);
            Upsert("KSEF_Url", data.KSEF_Url);
            Upsert("KSEF_Mode", data.KSEF_Mode);
            Upsert("Wystawca_NIP", data.Wystawca_NIP);
            Upsert("Wystawca_Ksef_Id", data.Wystawca_Ksef_Id);

            await context.SaveChangesAsync();
        }

        public async Task<(int RawNumber, string FormattedNumber)> GetNextInvoiceNumberDataAsync(bool isCorrection = false)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var now = DateTime.Now;
            string docType = isCorrection ? "FV Korekta" : "Faktura";
            string urzadzenieId = GetActiveUrzadzenieId();

            var lastRecord = await context.Numeracja
                .Where(n => n.Document_Type == docType && n.Year == now.Year)
                .OrderByDescending(n => n.TimeStamp)
                .ThenByDescending(n => n.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            string prefix = isCorrection ? "FK" : "FV";
            string sufix = now.Year.ToString();

            if (lastRecord != null)
            {
                nextNumber = lastRecord.Number + 1;
                if (!string.IsNullOrWhiteSpace(lastRecord.Prefix)) prefix = lastRecord.Prefix;
                if (!string.IsNullOrWhiteSpace(lastRecord.Sufix)) sufix = lastRecord.Sufix;
            }

            string formattedNumber = $"{prefix}/{urzadzenieId}/{nextNumber}/{now.Month:D2}/{sufix}";

            return (nextNumber, formattedNumber);
        }

        public async Task IncrementInvoiceNumberAsync(bool isCorrection = false)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var now = DateTime.Now;
            string docType = isCorrection ? "FV Korekta" : "Faktura";
            string urzadzenieId = GetActiveUrzadzenieId();

            var lastRecord = await context.Numeracja
                .Where(n => n.Document_Type == docType && n.Year == now.Year)
                .OrderByDescending(n => n.TimeStamp)
                .ThenByDescending(n => n.Id)
                .FirstOrDefaultAsync();

            int newNumber = (lastRecord != null) ? lastRecord.Number + 1 : 1;

            context.Numeracja.Add(new Numeracja
            {
                Urzadzenie_Id = urzadzenieId,
                Document_Type = docType,
                Number = newNumber,
                Prefix = isCorrection ? "FK" : "FV",
                Sufix = now.Year.ToString(),
                Year = now.Year,
                Month = now.Month,
                TimeStamp = DateTime.Now
            });

            await context.SaveChangesAsync();
        }

        private static string GetVal(List<Konfiguracja> list, string key)
        {
            return list.FirstOrDefault(c => c.Parametr_Nazwa == key)?.Parametr_Wartosc ?? string.Empty;
        }
    }
}