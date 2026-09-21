namespace RateAlerts.Api.Models;

/// <summary>
/// Response projection for an alert that includes computed triggered state.
/// </summary>
public class AlertDto
{
    public Guid Id { get; set; }
    public string Pair { get; set; } = string.Empty;
    public decimal Threshold { get; set; }
    public string Direction { get; set; } = string.Empty;
    public bool Triggered { get; set; }
    public DateTime CreatedAt { get; set; }
}
