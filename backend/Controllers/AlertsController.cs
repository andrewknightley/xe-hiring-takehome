using Microsoft.AspNetCore.Mvc;
using RateAlerts.Api.Models;
using RateAlerts.Api.Services;

namespace RateAlerts.Api.Controllers;

/// <summary>
/// Manages rate alerts: creation, listing, and deletion.
/// </summary>
[ApiController]
[Route("api/alerts")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;
    private readonly IRatesProvider _ratesProvider;
    private readonly AlertEvaluator _alertEvaluator;

    public AlertsController(IAlertService alertService, IRatesProvider ratesProvider, AlertEvaluator alertEvaluator)
    {
        _alertService = alertService;
        _ratesProvider = ratesProvider;
        _alertEvaluator = alertEvaluator;
    }

    /// <summary>
    /// GET /api/alerts
    /// Retrieves all alerts with their current triggered state.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var alerts = await _alertService.GetAllAlertsAsync();

        if (alerts.Count == 0)
            return Ok(new List<AlertDto>());

        // Get current rates for all unique pairs in alerts
        var uniquePairs = alerts
            .Select(a => a.Pair)
            .Distinct()
            .Select(pair => ParsePair(pair))
            .ToList();

        IReadOnlyList<RateResult> rates;
        try
        {
            rates = await _ratesProvider.GetRatesAsync(uniquePairs);
        }
        catch
        {
            // If rates fetch fails, return alerts without triggered state
            return Ok(alerts.Select(a => new AlertDto
            {
                Id = a.Id,
                Pair = a.Pair,
                Threshold = a.Threshold,
                Direction = a.Direction,
                CreatedAt = a.CreatedAt,
                Triggered = false // Conservative: assume not triggered if rates unavailable
            }).ToList());
        }

        // Build rate lookup
        var rateMap = rates.ToDictionary(r => r.Pair, r => r.Rate);

        // Project alerts with triggered state
        var result = alerts
            .Select(a => new AlertDto
            {
                Id = a.Id,
                Pair = a.Pair,
                Threshold = a.Threshold,
                Direction = a.Direction,
                CreatedAt = a.CreatedAt,
                Triggered = rateMap.TryGetValue(a.Pair, out var rate) 
                    ? _alertEvaluator.IsTriggered(a, rate)
                    : false
            })
            .ToList();

        return Ok(result);
    }

    /// <summary>
    /// POST /api/alerts
    /// Creates a new alert.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAlertRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Pair))
            return BadRequest(new { error = "Pair is required." });

        if (request.Direction is not ("above" or "below"))
            return BadRequest(new { error = "Direction must be 'above' or 'below'." });

        if (request.Threshold < 0)
            return BadRequest(new { error = "Threshold cannot be negative." });

        // Check if pair is supported
        var isSupported = await _alertService.IsPairSupportedAsync(request.Pair);
        if (!isSupported)
            return BadRequest(new { error = $"Unsupported currency pair '{request.Pair}'." });

        // Create alert
        Alert alert;
        try
        {
            alert = await _alertService.CreateAlertAsync(request);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return CreatedAtAction(nameof(List), new { id = alert.Id }, new AlertDto
        {
            Id = alert.Id,
            Pair = alert.Pair,
            Threshold = alert.Threshold,
            Direction = alert.Direction,
            CreatedAt = alert.CreatedAt,
            Triggered = false // New alerts are never triggered
        });
    }

    /// <summary>
    /// DELETE /api/alerts/{id}
    /// Deletes an alert by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _alertService.DeleteAlertAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Parses a pair string like "USD/CAD" into ("USD", "CAD").
    /// </summary>
    private static (string from, string to) ParsePair(string pair)
    {
        var parts = pair.Split('/');
        if (parts.Length != 2)
            throw new ArgumentException($"Invalid pair format: {pair}");
        return (parts[0], parts[1]);
    }
}
