using System.Text.Json;

namespace RateAlerts.Api.Services;

public class XeRatesClient
{
    private readonly HttpClient _httpClient;

    public XeRatesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(decimal mid, string timestamp)> GetConversionRateAsync(string from, string to)
    {
        var response = await _httpClient.GetAsync($"/v1/convert_from.json/?from={from}&to={to}");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var mid = doc.RootElement.GetProperty("to")[0].GetProperty("mid").GetDecimal();
        var timestamp = doc.RootElement.GetProperty("timestamp").GetString();
        return (mid, timestamp ?? string.Empty);
    }
}
