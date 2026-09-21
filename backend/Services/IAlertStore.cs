using RateAlerts.Api.Models;

namespace RateAlerts.Api.Services;

/// <summary>
/// Abstraction for alert data storage.
/// Implementations can use different storage mechanisms (in-memory, database, etc.).
/// </summary>
public interface IAlertStore
{
    /// <summary>
    /// Adds an alert to the store.
    /// </summary>
    void Add(Alert alert);

    /// <summary>
    /// Retrieves all alerts from the store.
    /// </summary>
    IReadOnlyList<Alert> GetAll();

    /// <summary>
    /// Removes an alert by ID.
    /// </summary>
    /// <returns>True if an alert was removed, false otherwise.</returns>
    bool RemoveById(Guid id);
}
