using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System;

using CurrencyConverterApi.Services;

namespace CurrencyConverterApi.Tests;

public class CalculatorServiceTests
{
    private readonly CurrencyExchangeService _currencyExchangeService;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    
    public CalculatorServiceTests()
    {
        // setup dependency injection container
        var serviceCollection = new ServiceCollection();

        // in-memory configuration
        var configValues = new Dictionary<string, string>
            {
                { "ExchangeApiKeys:FrankfurterLatestEndpoint", "https://api.frankfurter.app/latest" },
                { "ExchangeApiKeys:FrankfurterDevEndpoint", "https://api.frankfurter.dev/v1/" },
                { "ExchangeApiKeys:FixerLatestEndpoint", "https://data.fixer.io/api/latest" },
                { "ExchangeApiKeys:FixerTimeSeriestEndpoint", "https://data.fixer.io/api/timeseries" },
                { "ExchangeApiKeys:FixerKey", "ADD_YOUR_FIXER_API_KEY_HERE" }
            };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        serviceCollection.AddSingleton<IConfiguration>(_configuration); // configuration
        serviceCollection.AddHttpClient(); // client factory
        var serviceProvider = serviceCollection.BuildServiceProvider();

        _configuration = serviceProvider.GetRequiredService<IConfiguration>();
        _httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        IFrankfurterExchangeService primaryCurrecyExchangeService = new FrankfurterExchangeService(_httpClientFactory, _configuration);
        IFixerExchangeService backupCurrecyExchangeService = new FixerExchangeService(_httpClientFactory, _configuration);
        _currencyExchangeService = new CurrencyExchangeService(primaryCurrecyExchangeService, backupCurrecyExchangeService);
    }

    [Fact]
    public void Should_return_correct_conversion()
    {
        string fromCurrency = "EUR";
        string toCurrency = "USD";
        decimal amount = 100m;

        decimal result = 
            _currencyExchangeService.ConvertCurrencyAsync(fromCurrency, toCurrency, amount).GetAwaiter().GetResult();
        
        Assert.Equal(103.9600m, result); // check the rate and update the value
    }
}
