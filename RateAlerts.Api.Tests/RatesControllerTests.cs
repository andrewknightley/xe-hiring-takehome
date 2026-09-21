using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RateAlerts.Api.Controllers;
using RateAlerts.Api.Services;
using System.Net.Http.Headers;
using System.Text;

namespace RateAlerts.Api.Tests
{
    public class RatesControllerTests
    {
        private RatesController _sut;
        private IConfiguration _configuration;
        private IRatesProvider _ratesProvider;

        public RatesControllerTests()
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
            _sut = new RatesController(_ratesProvider);
        }

        [Fact]
        public async Task GivenGetRatesIsCalledWhenSuccessfulThenResponseIs200Ok()
        {
            var response = await _sut.GetRates();
            Assert.IsType<OkObjectResult>(response);
        }
    }
}
