using KsefIntegration3.Components;
using KsefIntegration3.Services;
using Microsoft.EntityFrameworkCore;
using Radzen;
using SkiControl.Data;
using SkiControl.Services;
using KsefIntegration_ModelLibrary.Services;
using PuppeteerSharp;

await new BrowserFetcher().DownloadAsync();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();
builder.Services.AddScoped<NotificationService>();

// Pobranie Connection Stringa i rejestracja fabryki DbContext dla Blazora
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Serwisy aplikacji
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<FakturaService>();
builder.Services.AddScoped<TowaryService>();
builder.Services.AddScoped<KontrahentService>();
builder.Services.AddScoped<PreferencesService>();
builder.Services.AddScoped<ConfigurationService>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<FakturaDokumentService>();
builder.Services.AddScoped<LoadingService>();
builder.Services.AddSingleton<KsefApiIntegrationHelper>();



builder.Services.AddHttpClient("KsefApiIntegration", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["KsefApiIntegration:BaseUrl"] 
        ?? throw new ArgumentException("Brak podanego adresu URL dla api integracyjnego api"));

    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    SslOptions = new System.Net.Security.SslClientAuthenticationOptions
    {
        RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();