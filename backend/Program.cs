using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Register controllers
builder.Services.AddControllers();

// Add configuration options
builder.Services.Configure<XeApiOptions>(builder.Configuration.GetSection("Xecd"));

// Register typed HttpClient for XE API
builder.Services.AddHttpClient<RateAlerts.Api.Services.XeRatesClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<XeApiOptions>>().Value;
    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.AccountId}:{options.ApiKey}"));
    client.BaseAddress = new Uri(options.BaseAddress);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(options.AuthScheme, credentials);
});

// Register rates provider
builder.Services.AddScoped<RateAlerts.Api.Services.IRatesProvider, RateAlerts.Api.Services.XeRatesProvider>();

var app = builder.Build();

app.MapControllers();

app.Run();
