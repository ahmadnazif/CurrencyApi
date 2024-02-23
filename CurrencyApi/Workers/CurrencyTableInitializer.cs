using CurrencyApi.Services;
using System.Diagnostics;

namespace CurrencyApi.Workers;

public class CurrencyTableInitializer(ILogger<CurrencyTableInitializer> logger, IDb db, OpenExchangeRatesApi api) : BackgroundService
{
    private readonly ILogger<CurrencyTableInitializer> logger = logger;
    private readonly IDb db = db;
    private readonly OpenExchangeRatesApi api = api;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(1000, stoppingToken);
        logger.LogInformation($"Starting '{nameof(CurrencyTableInitializer)}' worker..");

        try
        {
            Stopwatch sw1 = Stopwatch.StartNew();
            var count = await db.CountAllCurrencyAsync(stoppingToken);
            sw1.Stop();

            if (count > 0)
            {
                logger.LogInformation($"All {count} currency data has already exist in 'currency' table [{sw1.Elapsed}]");
            }
            else
            {
                Stopwatch sw2 = Stopwatch.StartNew();

                var currencies = await api.GetCurrenciesAsync(stoppingToken);
                await db.InitializeCurrencyTableDataAsync(currencies.ToDictionary(), stoppingToken);

                sw2.Stop();

                logger.LogInformation($"All currency data has been initialized with {currencies.Count} data [{sw2.Elapsed}]");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken) => base.StopAsync(cancellationToken);
}
