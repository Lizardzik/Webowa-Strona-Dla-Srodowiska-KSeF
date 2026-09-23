namespace SkiControl.Services;

public class LoadingService
{
    public bool IsLoading { get; private set; }
    public string? LoadingMessage { get; private set; }

    public event Action? OnChange;

    public void Show(string? message = null)
    {
        IsLoading = true;
        LoadingMessage = message;
        NotifyStateChanged();
    }

    public void Hide()
    {
        IsLoading = false;
        LoadingMessage = null;
        NotifyStateChanged();
    }
    public async Task WrapWithDelayAsync(Func<Task> action, int delayMs = 2500, string? message = null)
    {
        Show(message);
        try
        {
            var actionTask = action();
            var delayTask = Task.Delay(delayMs);
            await Task.WhenAll(actionTask, delayTask);
        }
        finally
        {
            Hide();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}