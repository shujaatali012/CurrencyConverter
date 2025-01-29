using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverterApi.Infrastructure
{
    public class PaginatedHistoricalRatesResponse
    {
        public string? Base { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public Dictionary<string, Dictionary<string, decimal>>? Rates { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
    }
}