using Serilog;
using CurrencyConverterApi.Services;
using CurrencyConverterApi.Infrastructure.HttpHandler;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Access configuration values
        var frankfurterLatestEndpoint = builder.Configuration["ExchangeApiKeys:FrankfurterLatestEndpoint"];
        var frankfurterDevEndpoint = builder.Configuration["ExchangeApiKeys:FrankfurterDevEndpoint"];
        var fixerLatestEndpoint = builder.Configuration["ExchangeApiKeys:FixerLatestEndpoint"];
        var fixerTimeSeriestEndpoint = builder.Configuration["ExchangeApiKeys:FixerTimeSeriestEndpoint"];
        var fixerKey = builder.Configuration["ExchangeApiKeys:FixerKey"];

        // Add cors services
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Add serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();

        // Add http logging service
        builder.Services.AddHttpLogging();

        // Add services to the container.
        builder.Services.AddControllers();
        
        // Api Rate Limiting
        builder.Services.AddRateLimiter(options => {
            options.AddFixedWindowLimiter("fixed", limiterOptions => 
            {
                limiterOptions.PermitLimit = 100; // number of allowed requests
                limiterOptions.Window = TimeSpan.FromMinutes(1); // time window
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 5; // number of requests to queue
            });
        });

        // Configure HttpClient with Rate Limiting (10 requests per second)
        var rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 10,  // max tokens
            TokensPerPeriod = 10,  // refill rate per period
            ReplenishmentPeriod = TimeSpan.FromSeconds(1), // refill interval
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
        
        builder.Services.AddHttpClient("CurrencyExchangeApi")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpRateLimitingHandler(rateLimiter)
            {
                InnerHandler = new HttpClientHandler()
            });

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // Add swagger services.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Register frankfurter api service
        builder.Services.AddSingleton<IFrankfurterExchangeService, FrankfurterExchangeService>();

        // Register fixer currency exchange api service
        builder.Services.AddSingleton<IFixerExchangeService, FixerExchangeService>();

        // Register common api service
        builder.Services.AddSingleton<ICurrencyExchangeService, CurrencyExchangeService>();

        // Enable https redirection in development
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                options.HttpsPort = 7289; // https port for local development
            });

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(5258); // http
                options.ListenLocalhost(7289, listenOptions =>
                {
                    listenOptions.UseHttps(); // enable https
                });
            });
        }

        var app = builder.Build();

        // Enable cors middleware
        app.UseCors();

        // enale rate limiting
        app.UseRateLimiter();
        app.MapControllers().RequireRateLimiting("fixed");

        app.UseHttpLogging(); // Logs http request details

        // Configure the http request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection(); // Redirect http to https locally

            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Currency Converter Api v1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.MapControllers();

        app.Run();
    }
}