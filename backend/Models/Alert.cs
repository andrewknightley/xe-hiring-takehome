namespace RateAlerts.Api.Models;

/// <summary>
/// Represents a rate alert configured by a user.
/// </summary>
public class Alert
{
    public Guid Id { get; set; }

    /// <summary>
    /// Currency pair (e.g., "USD/CAD", "GBP/USD").
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

    /// <summary>
    /// When the alert was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
