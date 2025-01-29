using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using CurrencyConverterApi.Infrastructure;

namespace CurrencyConverterApi.Services
{
    public class FrankfurterExchangeService : IFrankfurterExchangeService
    {
        #region properties

        private static readonly ConcurrentDictionary<string, ExchangeRatesResponse> Cache = new();
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        
        #endregion
        
        #region constructor
        
        public FrankfurterExchangeService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        
        #endregion

        #region methods

        public async Task<ExchangeRatesResponse?> GetLatestExchangeRatesAsync(string baseCurrency)
        {
            baseCurrency = baseCurrency.ToUpper(CultureInfo.InvariantCulture);

            if (Cache.TryGetValue(baseCurrency, out var cachedResponse))
            {
                return cachedResponse;
            }

            var endpoint = _configuration["ExchangeApiKeys:FrankfurterLatestEndpoint"];
            var response = await _httpClient.GetAsync($"{endpoint}?baseCurrency={baseCurrency}");

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("Frankfurter api is unavailable.");

            var json = await response.Content.ReadAsStringAsync();

            var exchangeRates = JsonSerializer.Deserialize<ExchangeRatesResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (exchangeRates != null)
            {
                Cache[baseCurrency] = exchangeRates;
            }

            return exchangeRates;
        }

        public async Task<decimal> ConvertCurrencyAsync(string fromCurrency, string toCurrency, decimal amount)
        {
            fromCurrency = fromCurrency.ToUpper(CultureInfo.InvariantCulture);
            toCurrency = toCurrency.ToUpper(CultureInfo.InvariantCulture);

            var exchangeRates = await GetLatestExchangeRatesAsync(fromCurrency);

            if (exchangeRates == null || 
                exchangeRates.Rates == null || 
                !exchangeRates.Rates.TryGetValue(toCurrency, out var rate))
            {
                throw new KeyNotFoundException($"Exchange rate for {toCurrency} not found.");
            }

            return amount * rate;
        }

        public async Task<PaginatedHistoricalRatesResponse> GetHistoricalRatesAsync(string baseCurrency, 
            string startDate, 
            string endDate, 
            int page, 
            int pageSize)
        {
            baseCurrency = baseCurrency.ToUpper(CultureInfo.InvariantCulture);
            var endpoint = _configuration["ExchangeApiKeys:FrankfurterDevEndpoint"];
    
            // todo use caching
            var response = await _httpClient.GetAsync($"{endpoint}{startDate}..{endDate}?base={baseCurrency}");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to fetch historical rates for {baseCurrency}.");
            }

            var json = await response.Content.ReadAsStringAsync();

            var historicalRates = JsonSerializer.Deserialize<HistoricalRatesResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (historicalRates == null || historicalRates.Rates == null)
            {
                throw new HttpRequestException($"No historical rates found for {baseCurrency}.");
            }

            var paginatedRates = historicalRates.Rates
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToDictionary(k => k.Key, v => v.Value);

            return new PaginatedHistoricalRatesResponse
            {
                Base = historicalRates.Base,
                StartDate = historicalRates.StartDate,
                EndDate = historicalRates.EndDate,
                Rates = paginatedRates,
                Page = page,
                PageSize = pageSize,
                TotalRecords = historicalRates.Rates.Count
            };
        }

        #endregion
    }
}