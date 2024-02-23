using CurrencyApi.Services;
using System.Diagnostics;

namespace CurrencyApi.Workers;

public class LatestRatesProcessor(ILogger<LatestRatesProcessor> logger, IDb db, OpenExchangeRatesApi api, IConfiguration config) : BackgroundService
{
    private readonly ILogger<LatestRatesProcessor> logger = logger;
    private readonly IDb db = db;
    private readonly OpenExchangeRatesApi api = api;
    private readonly IConfiguration config = config;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(3000, stoppingToken);
        logger.LogInformation($"Starting '{nameof(LatestRatesProcessor)}' worker..");

        var refreshTs = TimeSpan.FromMinutes(int.Parse(config["OpenExchangeRates:LatestRatesRefreshMinute"]));
        logger.LogInformation($"Rates refresh delay set to {refreshTs}");

        if (refreshTs.TotalMinutes < 30)
        {
            refreshTs = new(0, 30, 0);
            logger.LogInformation($"Rates refresh delay set to minimum {refreshTs}");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation($"Executing '{nameof(LatestRatesProcessor)}' worker..");
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                var latestTimeStr = await db.GetSettingAsync(SettingId.LatestRatesRefreshTime, stoppingToken);

                // First refresh
                if (string.IsNullOrWhiteSpace(latestTimeStr)) 
                {
                    await ProcessAsync(stoppingToken);
                    sw.Stop();

                    var nextRefreshTime = DateTime.Now.Add(refreshTs);
                    logger.LogInformation($"Done. Next refresh at {nextRefreshTime.ToDbDateTimeString()} in {refreshTs} delay [{sw.Elapsed}]");
                    await Task.Delay(refreshTs, stoppingToken);
                }

                // Subsequent refresh
                else
                {
                    var lastRefreshTime = DateTimeHelper.Parse(latestTimeStr, DateTimeFormat.DbDateTime);

                    if (DateTime.Now.Subtract(lastRefreshTime.Value) > refreshTs)
                    {
                        await ProcessAsync(stoppingToken);
                        sw.Stop();

                        var nextRefreshTime = DateTime.Now.Add(refreshTs);
                        logger.LogInformation($"Done. Next refresh at {nextRefreshTime.ToDbDateTimeString()} in {refreshTs} delay [{sw.Elapsed}]");
                        await Task.Delay(refreshTs, stoppingToken);
                    }
                    else
                    {
                        sw.Stop();

                        var nextRefreshTime = lastRefreshTime.Value.Add(refreshTs);
                        logger.LogInformation($"Refresh not needed. Next refresh at {nextRefreshTime.ToDbDateTimeString()} in {refreshTs} delay [{sw.Elapsed}]");

                        var newRefreshTs = nextRefreshTime.Subtract(DateTime.Now);
                        logger.LogInformation($"NEW Refresh TS: {newRefreshTs}"); // TODO: delete
                        await Task.Delay(newRefreshTs, stoppingToken);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                sw.Stop();
                logger.LogInformation("Worker is stopped");
            }
            catch (Exception ex)
            {
                sw.Stop();
                logger.LogError(ex.Message);

                var nextRefreshTime = DateTime.Now.Add(refreshTs);
                logger.LogInformation($"Error occured. Will refresh again at {nextRefreshTime} [{sw.Elapsed}]");
                await Task.Delay(refreshTs, stoppingToken);
            }
        }
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        Stopwatch sw = Stopwatch.StartNew();
        var raw = await api.GetLatestRatesAsync(ct);
        if (raw == null)
        {
            sw.Stop();
            logger.LogInformation($"Can't process. No data returned from API. Please check API error log [{sw.Elapsed}]");
            return;
        }

        //var timestamp = DateTimeOffset.FromUnixTimeSeconds(raw.Timestamp);
        var rates = raw.Rates.Select(x => new CurrencyRateBase
        {
            CurrencyCode = x.Key,
            Rate = x.Value,
            AgainstOne = raw.BaseCurrency
        });

        await db.RefreshLatestRateAsync(rates.ToList(), ct);
        await db.AddRateHistoryAsync(raw.Rates.ToDictionary(x => x.Key, y => y.Value), ct);
        await db.SaveSettingAsync(SettingId.LatestRatesRefreshTime, DateTime.Now.ToDbDateTimeString(), ct);

        sw.Stop();
        logger.LogInformation($"Refresh success [{sw.Elapsed}]");
    }

    private async Task Process2Async(CancellationToken ct)
    {
        await db.SaveSettingAsync(SettingId.LatestRatesRefreshTime, DateTime.Now.ToDbDateTimeString(), ct);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}

