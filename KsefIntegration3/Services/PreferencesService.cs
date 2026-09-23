using Microsoft.JSInterop;
using Radzen;
using SkiControl.Models;
using System.Text.Json;

namespace SkiControl.Services;

public class PreferencesService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly NotificationService _notificationService;
    private const string StorageKey = "SkiControl_Settings";

    public event Action? OnChange;
    public void NotifyStateChanged() => OnChange?.Invoke();

    public PreferencesService(IJSRuntime jsRuntime, NotificationService notificationService)
    {
        _jsRuntime = jsRuntime;
        _notificationService = notificationService;
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch
        {
            // Ciche ignorowanie błędów przy pierwszym ładowaniu
        }

        return new AppSettings();
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    public async Task NotifyAsync(NotificationSeverity severity, string summary, string detail)
    {
        var settings = await GetSettingsAsync();
        double duration = settings.NotificationDuration > 0 ? settings.NotificationDuration : 4000;

        _notificationService.Notify(new NotificationMessage
        {
            Severity = severity,
            Summary = summary,
            Detail = detail,
            Duration = duration
        });
    }
}