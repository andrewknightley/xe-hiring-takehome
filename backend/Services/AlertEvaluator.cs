using RateAlerts.Api.Models;

namespace RateAlerts.Api.Services;

/// <summary>
/// Evaluates whether an alert should be triggered based on current rates.
/// Single responsibility: determine if an alert condition is met.
/// </summary>
public class AlertEvaluator
{
    /// <summary>
    /// Determines if an alert is triggered given a current rate.
    /// </summary>
    /// <param name="alert">The alert to evaluate.</param>
    /// <param name="currentRate">The current rate for the alert's currency pair.</param>
    /// <returns>True if the alert condition is met.</returns>
    public bool IsTriggered(Alert alert, decimal currentRate)
    {
        return alert.Direction switch
        {
            "above" => currentRate > alert.Threshold,
            "below" => currentRate < alert.Threshold,
            _ => false
        };
    }
}
