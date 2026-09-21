using Moq;
using RateAlerts.Api.Models;
using RateAlerts.Api.Services;

namespace RateAlerts.Api.Tests;

public class InMemoryAlertServiceTests
{
    private readonly Mock<IRatesProvider> _mockRatesProvider;
    private readonly InMemoryAlertService _service;

    public InMemoryAlertServiceTests()
    {
        _mockRatesProvider = new Mock<IRatesProvider>();
        _service = new InMemoryAlertService(_mockRatesProvider.Object);
    }

    [Fact]
    public async Task CreateAlertAsync_WithValidRequest_ReturnsAlert()
    {
        // Arrange
        var request = new CreateAlertRequest
        {
            Pair = "USD/CAD",
            Threshold = 1.5m,
            Direction = "above"
        };

        // Act
        var result = await _service.CreateAlertAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("USD/CAD", result.Pair);
        Assert.Equal(1.5m, result.Threshold);
        Assert.Equal("above", result.Direction);
        Assert.True(result.CreatedAt > DateTime.UtcNow.AddSeconds(-1));
    }

    [Fact]
    public async Task CreateAlertAsync_WithEmptyPair_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateAlertRequest
        {
            Pair = "",
            Threshold = 1.5m,
            Direction = "above"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAlertAsync(request));
    }

    [Fact]
    public async Task CreateAlertAsync_WithInvalidDirection_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateAlertRequest
        {
            Pair = "USD/CAD",
            Threshold = 1.5m,
            Direction = "sideways"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAlertAsync(request));
    }

    [Fact]
    public async Task CreateAlertAsync_WithNegativeThreshold_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateAlertRequest
        {
            Pair = "USD/CAD",
            Threshold = -1m,
            Direction = "above"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAlertAsync(request));
    }

    [Fact]
    public async Task GetAllAlertsAsync_WithNoAlerts_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetAllAlertsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAlertsAsync_WithMultipleAlerts_ReturnsAllAlerts()
    {
        // Arrange
        var request1 = new CreateAlertRequest { Pair = "USD/CAD", Threshold = 1.5m, Direction = "above" };
        var request2 = new CreateAlertRequest { Pair = "GBP/USD", Threshold = 1.3m, Direction = "below" };

        await _service.CreateAlertAsync(request1);
        await _service.CreateAlertAsync(request2);

        // Act
        var result = await _service.GetAllAlertsAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, a => a.Pair == "USD/CAD");
        Assert.Contains(result, a => a.Pair == "GBP/USD");
    }

    [Fact]
    public async Task DeleteAlertAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var request = new CreateAlertRequest { Pair = "USD/CAD", Threshold = 1.5m, Direction = "above" };
        var alert = await _service.CreateAlertAsync(request);

        // Act
        var result = await _service.DeleteAlertAsync(alert.Id);

        // Assert
        Assert.True(result);
        var alerts = await _service.GetAllAlertsAsync();
        Assert.Empty(alerts);
    }

    [Fact]
    public async Task DeleteAlertAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteAlertAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllAlertsAsync_ThreadSafety_ConcurrentOperations()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act - Create alerts from multiple threads
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var request = new CreateAlertRequest
                {
                    Pair = "USD/CAD",
                    Threshold = 1.5m + i,
                    Direction = "above"
                };
                await _service.CreateAlertAsync(request);
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        var result = await _service.GetAllAlertsAsync();
        Assert.Equal(10, result.Count);
    }
}
