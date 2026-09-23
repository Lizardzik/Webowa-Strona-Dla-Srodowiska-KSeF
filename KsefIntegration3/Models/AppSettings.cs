namespace SkiControl.Models;

public class AppSettings
{
    public bool IsDarkMode { get; set; } = false;
    public int DefaultPageSize { get; set; } = 15;
    public string StartupPage { get; set; } = "/";
    public double NotificationDuration { get; set; } = 4000;
    public bool EnableShortcuts { get; set; } = false;
}