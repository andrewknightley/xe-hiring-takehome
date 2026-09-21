using Microsoft.AspNetCore.Mvc;
using RateAlerts.Api.Services;

namespace RateAlerts.Api.Controllers;

[ApiController]
[Route("api/rates")]
public class RatesController : ControllerBase
{
    private readonly IRatesProvider _ratesProvider;

    public RatesController(IRatesProvider ratesProvider)
    {
        _ratesProvider = ratesProvider;
    }

    [HttpGet]
    public async Task<IActionResult> GetRates()
    {
        var results = await _ratesProvider.GetRatesAsync(DisplayedCurrencyPairs.All);
        return Ok(results);
    }
}

public static class DisplayedCurrencyPairs
{
    public static readonly IReadOnlyCollection<(string, string)> All = new[]
    {
        ("USD", "CAD"),
        ("GBP", "USD"),
        ("EUR", "USD")
    }.AsReadOnly();
}
