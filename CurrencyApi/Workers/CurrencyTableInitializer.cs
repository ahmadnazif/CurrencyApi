using CurrencyApi.Services;
using System.Diagnostics;

namespace CurrencyApi.Workers;

public class CurrencyTableInitializer(ILogger<CurrencyTableInitializer> logger, IDb db, CountryService country) : BackgroundService
{
    private readonly ILogger<CurrencyTableInitializer> logger = logger;
    private readonly IDb db = db;
    private readonly CountryService country = country;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(1000, stoppingToken);
        logger.LogInformation($"Starting '{nameof(CurrencyTableInitializer)}' worker..");

        try
        {
            Stopwatch sw1 = Stopwatch.StartNew();
            var count = await db.CountAllCurrencyAsync(stoppingToken);
            sw1.Stop();

            if (count == country.CountryCount)
            {
                logger.LogInformation($"All currency data has already exist in 'currency' table [{sw1.Elapsed}]");
            }
            else
            {
                logger.LogError($"count: {count}, country count: {country.CountryCount}");
                Stopwatch sw2 = Stopwatch.StartNew();

                var data = country.ListAllCountry().Select(d => new Currency
                {
                    CountryCode = d.Iso,
                    CountryName = d.Name,
                    CurrencyCode = d.CurrencyCode?.ToString(),
                    CurrencyName = string.IsNullOrWhiteSpace(d.CurrencyName) ? "Unknown" : d.CurrencyName
                });

                await db.InitializeCurrencyTableDataAsync(data.ToList(), stoppingToken);
                sw2.Stop();

                logger.LogInformation($"All currency data has been initialized [{sw2.Elapsed}]");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}
