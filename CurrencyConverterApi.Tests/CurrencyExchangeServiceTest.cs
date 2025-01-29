using Xunit;
using Microsoft.Extensions.Configuration;
using CurrencyConverterApi.Services;
using System.Threading.Tasks;

namespace CurrencyConverterApi.Tests;

public class CalculatorServiceTests
{
    private readonly CurrencyExchangeService _currencyExchangeService;
    private readonly IConfiguration _configuration;
    

    public CalculatorServiceTests(IConfiguration configuration)
    {
        HttpClient httpClient = new HttpClient();
        IFrankfurterExchangeService primaryCurrecyExchangeService = new FrankfurterExchangeService(httpClient, configuration);
        IFixerExchangeService backupCurrecyExchangeService = new FixerExchangeService(httpClient, configuration);
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
        
        Assert.Equal(104.0421m, result);
    }
}
