using RateAlerts.Api.Models;

namespace RateAlerts.Api.Services;

/// <summary>
/// In-memory implementation of alert service.
/// Delegates alert storage to an <see cref="IAlertStore"/> implementation.
/// </summary>
public class InMemoryAlertService : IAlertService
{
    private readonly IAlertStore _alertStore;
    private readonly IRatesProvider _ratesProvider;

    public InMemoryAlertService(IRatesProvider ratesProvider, IAlertStore alertStore)
    {
        _ratesProvider = ratesProvider;
        _alertStore = alertStore;
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

        _alertStore.Add(alert);

        return Task.FromResult(alert);
    }

    public Task<IReadOnlyList<Alert>> GetAllAlertsAsync()
    {
        return Task.FromResult(_alertStore.GetAll());
    }

    public Task<bool> DeleteAlertAsync(Guid id)
    {
        var result = _alertStore.RemoveById(id);
        return Task.FromResult(result);
    }

    public async Task<bool> IsPairSupportedAsync(string pair)
    {
        var rates = await _ratesProvider.GetRatesAsync(new[]
        {
            ("USD", "CAD"),
            ("GBP", "USD"),
            ("EUR", "USD")
        });

        return rates.Any(r => r.Pair == pair);
    }
}

