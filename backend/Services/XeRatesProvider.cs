namespace RateAlerts.Api.Services;

public class XeRatesProvider : IRatesProvider
{
    private readonly XeRatesClient _xeRatesClient;

    public XeRatesProvider(XeRatesClient xeRatesClient)
    {
        _xeRatesClient = xeRatesClient;
    }

    public async Task<IReadOnlyList<RateResult>> GetRatesAsync(IReadOnlyCollection<(string from, string to)> pairs)
    {
        if(pairs.Count == 0)
        {
            return Array.Empty<RateResult>().AsReadOnly();
        }

        var results = new List<RateResult>();

        foreach (var (from, to) in pairs)
        {
            var (mid, timestamp) = await _xeRatesClient.GetConversionRateAsync(from, to);
            var pairName = $"{from}/{to}";
            results.Add(new RateResult(pairName, Math.Round(mid, 4), timestamp));
        }

        return results.AsReadOnly();
    }
}
