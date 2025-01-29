using System.Threading.RateLimiting;

namespace CurrencyConverterApi.Infrastructure.HttpHandler;

public class HttpRateLimitingHandler : DelegatingHandler
{
    private readonly RateLimiter _rateLimiter;

    public HttpRateLimitingHandler(RateLimiter rateLimiter)
    {
        _rateLimiter = rateLimiter;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using (var lease = await _rateLimiter.AcquireAsync(1, cancellationToken))
        {
            if (!lease.IsAcquired)
            {
                throw new HttpRequestException("Rate limit exceeded.");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
