using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using CurrencyConverterApi.Services;
using System.ComponentModel.DataAnnotations;


namespace CurrencyConverterApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyConverterController : ControllerBase
    {
        #region properties
        
        private readonly ILogger<CurrencyConverterController> _logger;
        private readonly ICurrencyExchangeService _currencyExchangeService;
        
        #endregion
        
        #region constructor

        public CurrencyConverterController(ILogger<CurrencyConverterController> logger,
            ICurrencyExchangeService currencyExchangeService)
        {
            _logger = logger;
            _currencyExchangeService = currencyExchangeService;
        }

        #endregion

        #region methods

        [HttpGet("rates")]
        public async Task<IActionResult> GetLatestExchangeRates([FromQuery][Required] string baseCurrency)
        {   
            try
            {
                var exchangeRates = await _currencyExchangeService.GetLatestExchangeRatesAsync(baseCurrency);
                return Ok(exchangeRates);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting latest exchange rates. Details: " + ex.Message);
                return NotFound(ex.Message);
            }
        }

        [HttpGet("convert")]
        public async Task<IActionResult> ConvertCurrency([FromQuery][Required] string fromCurrency,
            [FromQuery][Required] string toCurrency,
            [FromQuery][Required] decimal amount)
        {
            // Input parameter validation
            if (amount <= 0)
            {
                _logger.LogError("Paramerter amount is invalid should be positive number.");
                return BadRequest("Invalid parameters. Please provide a positive amount.");
            }

            // exclude currencies validation (TRY, PLN, THB and MXN)
            string fromCurrencyInvariantCulture = fromCurrency.ToUpper(CultureInfo.InvariantCulture);
            string toCurrencyInvariantCulture = toCurrency.ToUpper(CultureInfo.InvariantCulture);

            if (fromCurrencyInvariantCulture == "TRY" || toCurrencyInvariantCulture == "TRY" ||
                fromCurrencyInvariantCulture == "PLN" || toCurrencyInvariantCulture == "PLN" ||
                fromCurrencyInvariantCulture == "THB" || toCurrencyInvariantCulture == "THB" ||
                fromCurrencyInvariantCulture == "MXN" || toCurrencyInvariantCulture == "MXN")
            {
                return BadRequest($"Currency conversion for either {fromCurrencyInvariantCulture} or {toCurrencyInvariantCulture} is not supported.");
            }

            try
            {
                var convertedAmount = await _currencyExchangeService.ConvertCurrencyAsync(fromCurrency, toCurrency, amount);
                return Ok(new { FromCurrency = fromCurrency, ToCurrency = toCurrency, Amount = amount, ConvertedAmount = convertedAmount });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while converting. Details: " + ex.Message);
                return NotFound(ex.Message);
            }
        }

        [HttpGet("historical-rates")]
        public async Task<IActionResult> GetHistoricalRates([FromQuery][Required] string baseCurrency,
            [FromQuery][Required] string startDate,
            [FromQuery][Required] string endDate,
            [FromQuery][Required] int page = 1,
            [FromQuery][Required] int pageSize = 10)
        {
            
            if (page <= 0 || pageSize <= 0)
            {
                _logger.LogError("Page and page size must be positive integers.");
                return BadRequest("Page and page size must be positive integers.");
            }
            try
            {
                var historicalRates = await _currencyExchangeService.GetHistoricalRatesAsync(baseCurrency, startDate, endDate, page, pageSize);
                return Ok(historicalRates);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting historical rates. Details: " + ex.Message);
                return NotFound(ex.Message);
            }
        }

        #endregion
    }
}