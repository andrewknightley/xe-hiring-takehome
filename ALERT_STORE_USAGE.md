## Alert Store Abstraction - Usage Guide

### Overview
The alert storage has been abstracted into an `IAlertStore` interface with three implementations:

1. **IAlertStore** - The interface defining the contract
2. **InMemoryAlertStore** - Production implementation with file-based persistence
3. **FakeAlertStore** - Test implementation for unit tests

### Production Usage
The dependency injection in `Program.cs` automatically registers:
```csharp
builder.Services.AddSingleton<IAlertStore, InMemoryAlertStore>();
```

The `InMemoryAlertService` now depends on `IAlertStore`:
```csharp
public InMemoryAlertService(IRatesProvider ratesProvider, IAlertStore alertStore)
```

### Test Usage Example

#### Basic Test Setup
```csharp
[Fact]
public async Task CreateAlertAsync_ShouldStoreAlert()
{
	// Arrange
	var fakeStore = new FakeAlertStore();
	var mockRatesProvider = new Mock<IRatesProvider>();
	var service = new InMemoryAlertService(mockRatesProvider.Object, fakeStore);

	var request = new CreateAlertRequest 
	{ 
		Pair = "USD/CAD", 
		Direction = "above", 
		Threshold = 1.50m 
	};

	// Act
	var alert = await service.CreateAlertAsync(request);

	// Assert
	var stored = await service.GetAllAlertsAsync();
	Assert.Single(stored);
	Assert.Equal(alert.Id, stored[0].Id);
}
```

#### Test with Pre-populated Data
```csharp
[Fact]
public async Task DeleteAlertAsync_ShouldRemoveAlert()
{
	// Arrange
	var alert1 = new Alert 
	{ 
		Id = Guid.NewGuid(), 
		Pair = "USD/CAD", 
		Direction = "above", 
		Threshold = 1.50m,
		CreatedAt = DateTime.UtcNow
	};
	var alert2 = new Alert 
	{ 
		Id = Guid.NewGuid(), 
		Pair = "GBP/USD", 
		Direction = "below", 
		Threshold = 1.20m,
		CreatedAt = DateTime.UtcNow
	};

	var fakeStore = new FakeAlertStore();
	fakeStore.SetupAlerts(alert1, alert2);  // Pre-populate with test data

	var mockRatesProvider = new Mock<IRatesProvider>();
	var service = new InMemoryAlertService(mockRatesProvider.Object, fakeStore);

	// Act
	var deleted = await service.DeleteAlertAsync(alert1.Id);

	// Assert
	Assert.True(deleted);
	var remaining = await service.GetAllAlertsAsync();
	Assert.Single(remaining);
	Assert.Equal(alert2.Id, remaining[0].Id);
}
```

### Benefits
- **Testability**: Use `FakeAlertStore` to avoid file I/O in tests
- **Flexibility**: Easy to add new storage implementations (database, cache, etc.)
- **Separation of Concerns**: Business logic (InMemoryAlertService) is decoupled from storage logic
- **No Side Effects**: Each test can fully control the alert state
