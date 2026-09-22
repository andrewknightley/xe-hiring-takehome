namespace RateAlerts.Api.Services;

public record RateResult(string Pair, decimal Rate, string AsOf);

public interface IRatesProvider
{
    Task<IReadOnlyList<RateResult>> GetRatesAsync(IReadOnlyCollection<(string from, string to)> pairs);
}
