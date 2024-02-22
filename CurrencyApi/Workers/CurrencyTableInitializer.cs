using CurrencyApi.Services;

namespace CurrencyApi.Workers;

public class CurrencyTableInitializer(ILogger<CurrencyTableInitializer> logger, IDb db, CountryService country) : BackgroundService
{
    private readonly ILogger<CurrencyTableInitializer> logger = logger;
    private readonly IDb db = db;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LOGGER.LogInformation($"App started on {DateTime.Now.ToDbDateTimeString()}");
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        var elapsed = DateTime.Now.Subtract(cache.AppInfo.AppStartTime);
        LOGGER.LogInformation($"App stopped on {DateTime.Now.ToDbDateTimeString()}. Runtime: {elapsed}");
        return base.StopAsync(cancellationToken);
    }
}
