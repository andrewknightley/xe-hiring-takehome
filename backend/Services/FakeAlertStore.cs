using RateAlerts.Api.Models;

namespace RateAlerts.Api.Services;

/// <summary>
/// Fake/test implementation of alert storage for unit testing.
/// Allows complete control over stored alerts without disk I/O.
/// </summary>
public class FakeAlertStore : IAlertStore
{
    private readonly List<Alert> _alerts = new();

    /// <summary>
    /// Sets up the store with predefined alerts for testing.
    /// </summary>
    public void SetupAlerts(params Alert[] alerts)
    {
        _alerts.Clear();
        _alerts.AddRange(alerts);
    }

    public void Add(Alert alert)
    {
        _alerts.Add(alert);
    }

    public IReadOnlyList<Alert> GetAll()
    {
        return _alerts.ToList().AsReadOnly();
    }

    public bool RemoveById(Guid id)
    {
        var removed = _alerts.RemoveAll(a => a.Id == id);
        return removed > 0;
    }
}
