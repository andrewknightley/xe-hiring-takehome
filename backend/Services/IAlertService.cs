using RateAlerts.Api.Models;

namespace RateAlerts.Api.Services;

/// <summary>
/// Service for managing rate alerts.
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// Creates a new alert.
    /// </summary>
    Task<Alert> CreateAlertAsync(CreateAlertRequest request);

    /// <summary>
    /// Retrieves all alerts.
    /// </summary>
    Task<IReadOnlyList<Alert>> GetAllAlertsAsync();

    /// <summary>
    /// Deletes an alert by ID.
    /// </summary>
    Task<bool> DeleteAlertAsync(Guid id);

    /// <summary>
    /// Checks if a currency pair is supported.
    /// </summary>
    Task<bool> IsPairSupportedAsync(string pair);
}
