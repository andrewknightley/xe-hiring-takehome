using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RateAlerts.Api.Controllers;
using RateAlerts.Api.Models;
using RateAlerts.Api.Services;
using System.Net.Http.Headers;
using System.Text;

namespace RateAlerts.Api.Tests
{
    public class AlertsControllerTests
    {
        private readonly IConfiguration _configuration;
        private readonly IRatesProvider _ratesProvider;

        public AlertsControllerTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();
            var client = new HttpClient();
            var options = _configuration.GetSection("Xecd").Get<XeApiOptions>();
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.AccountId}:{options.ApiKey}"));
            client.BaseAddress = new Uri(options.BaseAddress);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(options.AuthScheme, credentials);
            var xceRatesClient = new XeRatesClient(client);
            _ratesProvider = new XeRatesProvider(xceRatesClient);
        }

        private AlertsController CreateController()
        {
            var alertService = new InMemoryAlertService(_ratesProvider, new FakeAlertStore());
            return new AlertsController(alertService, _ratesProvider, new AlertEvaluator());
        }

        [Fact]
        public async Task List_ReturnsEmptyList_WhenNoAlertsExist()
        {
            var controller = CreateController();
            var result = await controller.List();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var alerts = Assert.IsAssignableFrom<List<AlertDto>>(okResult.Value);
            Assert.Empty(alerts);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAlert_WithValidInput()
        {
            var controller = CreateController();
            var createRequest = new CreateAlertRequest
            {
                Pair = "EUR/USD",
                Threshold = 1.05m,
                Direction = "above"
            };

            var result = await controller.Create(createRequest);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var alert = Assert.IsType<AlertDto>(createdResult.Value);
            Assert.Equal("EUR/USD", alert.Pair);
            Assert.Equal(1.05m, alert.Threshold);
        }

        [Fact]
        public async Task Create_PersistsAlert_AndIsRetrievable()
        {
            var controller = CreateController();
            var createRequest = new CreateAlertRequest
            {
                Pair = "GBP/USD",
                Threshold = 1.27m,
                Direction = "below"
            };

            var createResult = await controller.Create(createRequest);
            var createdAlert = Assert.IsType<CreatedAtActionResult>(createResult).Value as AlertDto;

            var listResult = await controller.List();
            var okResult = Assert.IsType<OkObjectResult>(listResult);
            var alerts = Assert.IsAssignableFrom<List<AlertDto>>(okResult.Value);

            Assert.Single(alerts);
            Assert.Equal(createdAlert.Id, alerts.First().Id);
        }

        [Fact]
        public async Task Delete_RemovesAlert_FromCollection()
        {
            var controller = CreateController();
            var createRequest = new CreateAlertRequest
            {
                Pair = "USD/CAD",
                Threshold = 150.0m,
                Direction = "above"
            };

            var createResult = await controller.Create(createRequest);
            var createdAlert = Assert.IsType<CreatedAtActionResult>(createResult).Value as AlertDto;

            var deleteResult = await controller.Delete(createdAlert.Id);
            Assert.IsType<NoContentResult>(deleteResult);

            var listResult = await controller.List();
            var okResult = Assert.IsType<OkObjectResult>(listResult);
            var alerts = Assert.IsAssignableFrom<List<AlertDto>>(okResult.Value);
            Assert.Empty(alerts);
        }

        [Theory]
        [InlineData("EUR/USD", 1.05)]
        [InlineData("GBP/USD", 1.27)]
        [InlineData("USD/CAD", 150.0)]
        public async Task Create_WithVariousCurrencyPairs_Succeeds(string pair, double rate)
        {
            var controller = CreateController();
            var createRequest = new CreateAlertRequest
            {
                Pair = pair,
                Threshold = (decimal)rate,
                Direction = "above"
            };

            var result = await controller.Create(createRequest);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var alert = Assert.IsType<AlertDto>(createdResult.Value);
            Assert.Equal(pair, alert.Pair);
            Assert.Equal((decimal)rate, alert.Threshold);
        }

        [Fact]
        public async Task MultipleAlerts_CanBeCreatedAndRetrieved()
        {
            var controller = CreateController();
            var pairs = new[] { "EUR/USD", "GBP/USD", "USD/CAD" };

            foreach (var pair in pairs)
            {
                await controller.Create(new CreateAlertRequest
                {
                    Pair = pair,
                    Threshold = 1.0m,
                    Direction = "above"
                });
            }

            var listResult = await controller.List();
            var okResult = Assert.IsType<OkObjectResult>(listResult);
            var allAlerts = Assert.IsAssignableFrom<List<AlertDto>>(okResult.Value);

            Assert.Equal(3, allAlerts.Count);
            Assert.All(allAlerts, alert => Assert.Contains(alert.Pair, pairs));
        }
    }
}
