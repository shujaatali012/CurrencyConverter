using CurrencyConverterApi.Infrastructure;

namespace CurrencyConverterApi.Services
{
    public class CurrencyExchangeService : ICurrencyExchangeService
    {
        #region properties

        private readonly IFrankfurterExchangeService _primaryCurrecyExchangeService;
        private readonly IFixerExchangeService _backupCurrecyExchangeService;
        
        #endregion
        
        #region constructor

        public CurrencyExchangeService(IFrankfurterExchangeService primaryCurrecyExchangeService,
            IFixerExchangeService backupCurrecyExchangeService)
        {
            _primaryCurrecyExchangeService = primaryCurrecyExchangeService;
            _backupCurrecyExchangeService = backupCurrecyExchangeService;
        }
        
        #endregion

        #region methods

        public async Task<ExchangeRatesResponse?> GetLatestExchangeRatesAsync(string baseCurrency)
        {
            try
            {
                return await _primaryCurrecyExchangeService.GetLatestExchangeRatesAsync(baseCurrency);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Primary service failed: {ex.Message}. Switching to backup service...");
                return await _backupCurrecyExchangeService.GetLatestExchangeRatesAsync(baseCurrency);
            }
        }

        public async Task<decimal> ConvertCurrencyAsync(string fromCurrency, string toCurrency, decimal amount)
        {
            try
            {
                return await _primaryCurrecyExchangeService.ConvertCurrencyAsync(fromCurrency, toCurrency, amount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Primary service failed: {ex.Message}. Switching to backup service...");
                return await _backupCurrecyExchangeService.ConvertCurrencyAsync(fromCurrency, toCurrency, amount);
            }
        }

        public async Task<PaginatedHistoricalRatesResponse> GetHistoricalRatesAsync(string baseCurrency, string startDate, string endDate, int page, int pageSize)
        {
            try
            {
                return await _primaryCurrecyExchangeService.GetHistoricalRatesAsync(baseCurrency, startDate, endDate, page, pageSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Primary service failed: {ex.Message}. Switching to backup service...");
                return await _backupCurrecyExchangeService.GetHistoricalRatesAsync(baseCurrency, startDate, endDate, page, pageSize);
            }
        }

        #endregion
    }
}