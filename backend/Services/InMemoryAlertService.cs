using RateAlerts.Api.Models;

namespace RateAlerts.Api.Services;

/// <summary>
/// In-memory implementation of alert storage.
/// Thread-safe using lock-based synchronization.
/// Trade-off: Alerts are lost when the application restarts.
/// </summary>
public class InMemoryAlertService : IAlertService
{
    private readonly List<Alert> _alerts = new();
    private readonly object _sync = new();
    private readonly IRatesProvider _ratesProvider;

    public InMemoryAlertService(IRatesProvider ratesProvider)
    {
        _ratesProvider = ratesProvider;
    }

    public Task<Alert> CreateAlertAsync(CreateAlertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Pair))
            throw new ArgumentException("Pair cannot be empty.", nameof(request.Pair));

        if (request.Direction is not ("above" or "below"))
            throw new ArgumentException("Direction must be 'above' or 'below'.", nameof(request.Direction));

        if (request.Threshold < 0)
            throw new ArgumentException("Threshold cannot be negative.", nameof(request.Threshold));

        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            Pair = request.Pair,
            Threshold = request.Threshold,
            Direction = request.Direction,
            CreatedAt = DateTime.UtcNow
        };

        lock (_sync)
        {
            _alerts.Add(alert);
        }

        return Task.FromResult(alert);
    }

    public Task<IReadOnlyList<Alert>> GetAllAlertsAsync()
    {
        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<Alert>>(_alerts.ToList().AsReadOnly());
        }
    }

    public Task<bool> DeleteAlertAsync(Guid id)
    {
        lock (_sync)
        {
            var removed = _alerts.RemoveAll(a => a.Id == id);
            return Task.FromResult(removed > 0);
        }
    }

    public async Task<bool> IsPairSupportedAsync(string pair)
    {
        // Get all supported pairs from the rates provider
        var rates = await _ratesProvider.GetRatesAsync(new[]
        {
            ("USD", "CAD"),
            ("GBP", "USD"),
            ("EUR", "USD")
        });

        return rates.Any(r => r.Pair == pair);
    }
}
