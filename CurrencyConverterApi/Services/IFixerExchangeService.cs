using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CurrencyConverterApi.Infrastructure;

namespace CurrencyConverterApi.Services
{
    public interface IFixerExchangeService
    {
        Task<ExchangeRatesResponse?> GetLatestExchangeRatesAsync(string baseCurrency);
        Task<decimal> ConvertCurrencyAsync(string fromCurrency, string toCurrency, decimal amount);
        Task<PaginatedHistoricalRatesResponse> GetHistoricalRatesAsync(string baseCurrency, string startDate, string endDate, int page, int pageSize);
    }
}