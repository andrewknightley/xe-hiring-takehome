using RateAlerts.Api.Models;
using RateAlerts.Api.Services;

namespace RateAlerts.Api.Tests;

public class AlertEvaluatorTests
{
    private readonly AlertEvaluator _evaluator = new();

    [Theory]
    [InlineData(1.5, 1.6, true)]   // rate above threshold: triggered
    [InlineData(1.5, 1.5, false)]  // rate equals threshold: not triggered
    [InlineData(1.5, 1.4, false)]  // rate below threshold: not triggered
    public void IsTriggered_WithAboveDirection_EvaluatesCorrectly(decimal threshold, decimal currentRate, bool expected)
    {
        // Arrange
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            Pair = "USD/CAD",
            Threshold = threshold,
            Direction = "above",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = _evaluator.IsTriggered(alert, currentRate);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1.5, 1.4, true)]   // rate below threshold: triggered
    [InlineData(1.5, 1.5, false)]  // rate equals threshold: not triggered
    [InlineData(1.5, 1.6, false)]  // rate above threshold: not triggered
    public void IsTriggered_WithBelowDirection_EvaluatesCorrectly(decimal threshold, decimal currentRate, bool expected)
    {
        // Arrange
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            Pair = "USD/CAD",
            Threshold = threshold,
            Direction = "below",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = _evaluator.IsTriggered(alert, currentRate);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsTriggered_WithInvalidDirection_ReturnsFalse()
    {
        // Arrange
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            Pair = "USD/CAD",
            Threshold = 1.5m,
            Direction = "invalid",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = _evaluator.IsTriggered(alert, 1.6m);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsTriggered_AboveThreshold_WithBoundaryCondition_NotTriggered()
    {
        // Arrange - testing boundary: rate exactly equals threshold (not "above")
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            Pair = "USD/CAD",
            Threshold = 1.5m,
            Direction = "above",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = _evaluator.IsTriggered(alert, 1.5m);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsTriggered_BelowThreshold_WithBoundaryCondition_NotTriggered()
    {
        // Arrange - testing boundary: rate exactly equals threshold (not "below")
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            Pair = "USD/CAD",
            Threshold = 1.5m,
            Direction = "below",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = _evaluator.IsTriggered(alert, 1.5m);

        // Assert
        Assert.False(result);
    }
}
