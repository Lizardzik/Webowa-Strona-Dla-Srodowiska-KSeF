using Microsoft.JSInterop;
using System.Text.Json;
using SkiControl.Models;

namespace SkiControl.Services;

public class SettingsService
{
    private readonly IJSRuntime _jsRuntime;
    private const string StorageKey = "SkiControl_Settings";
    public event Action? OnChange;
    public void NotifyStateChanged() => OnChange?.Invoke();

    public SettingsService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
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
        }

        return new AppSettings();
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }
}