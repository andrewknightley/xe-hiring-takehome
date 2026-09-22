using RateAlerts.Api.Models;

namespace RateAlerts.Api.Services;

/// <summary>
/// In-memory implementation of alert storage with file-based persistence.
/// Thread-safe using lock-based synchronization.
/// Alerts are persisted to disk and reloaded on application startup.
/// </summary>
public class InMemoryAlertStore : IAlertStore
{
    private readonly List<Alert> _alerts = new();
    private readonly object _sync = new();
    private readonly string _persistencePath;

    public InMemoryAlertStore()
    {
        _persistencePath = Path.Combine(AppContext.BaseDirectory, "alerts.json");
        LoadAlertsFromDisk();
    }

    public void Add(Alert alert)
    {
        lock (_sync)
        {
            _alerts.Add(alert);
            SaveAlertsToDisk();
        }
    }

    public IReadOnlyList<Alert> GetAll()
    {
        lock (_sync)
        {
            return _alerts.ToList().AsReadOnly();
        }
    }

    public bool RemoveById(Guid id)
    {
        lock (_sync)
        {
            var removed = _alerts.RemoveAll(a => a.Id == id);
            if (removed > 0)
                SaveAlertsToDisk();
            return removed > 0;
        }
    }

    private void SaveAlertsToDisk()
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(_alerts);
            File.WriteAllText(_persistencePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to persist alerts: {ex.Message}");
        }
    }

    private void LoadAlertsFromDisk()
    {
        try
        {
            if (File.Exists(_persistencePath))
            {
                var json = File.ReadAllText(_persistencePath);
                var loaded = System.Text.Json.JsonSerializer.Deserialize<List<Alert>>(json);
                if (loaded != null)
                    _alerts.AddRange(loaded);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load alerts from disk: {ex.Message}");
        }
    }
}
