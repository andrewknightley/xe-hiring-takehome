namespace RateAlerts.Api.Models;

/// <summary>
/// Request to create a new rate alert.
/// </summary>
public class CreateAlertRequest
{
    /// <summary>
    /// Currency pair (e.g., "USD/CAD").
    /// </summary>
    public string Pair { get; set; } = string.Empty;

    /// <summary>
    /// The threshold rate value.
    /// </summary>
    public decimal Threshold { get; set; }

    /// <summary>
    /// Direction to monitor: "above" or "below".
    /// </summary>
    public string Direction { get; set; } = string.Empty;
}
