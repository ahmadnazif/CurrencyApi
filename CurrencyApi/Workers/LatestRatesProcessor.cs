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
        await Task.Delay(5000, stoppingToken);
        logger.LogInformation($"Starting '{nameof(LatestRatesProcessor)}' worker..");

        var rateRefreshdelay = TimeSpan.FromMinutes(int.Parse(config["LatestRatesRefreshDelayMinute"]));
        logger.LogInformation($"Rates refresh delay set to {rateRefreshdelay}");

        if (rateRefreshdelay.TotalMinutes < 30)
        {
            rateRefreshdelay = new(0, 30, 0);
            logger.LogInformation($"Rates refresh delay set to {rateRefreshdelay}");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation($"Executing '{nameof(LatestRatesProcessor)}' worker..");
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                var latestTimeStr = await db.GetSettingAsync(SettingId.LatestRatesRefreshTime, stoppingToken);
                if (string.IsNullOrWhiteSpace(latestTimeStr))
                {
                    await ProcessAsync(stoppingToken);
                    sw.Stop();

                    var nextRefreshTime = DateTime.Now.Add(rateRefreshdelay);
                    logger.LogInformation($"Done. Next refresh at {nextRefreshTime} [{sw.Elapsed}]");
                    await Task.Delay(rateRefreshdelay, stoppingToken);
                }
                else
                {
                    var time = DateTimeHelper.Parse(latestTimeStr, DateTimeFormat.DbDateTime);
                    if (DateTime.Now.Subtract(time.Value) > rateRefreshdelay)
                    {
                        await ProcessAsync(stoppingToken);
                        sw.Stop();

                        var nextRefreshTime = DateTime.Now.Add(rateRefreshdelay);
                        logger.LogInformation($"Done. Next refresh at {nextRefreshTime} [{sw.Elapsed}]");
                    }
                    else
                    {
                        sw.Stop();
                        var nextRefreshTime = time.Value.Add(rateRefreshdelay);
                        logger.LogInformation($"Refresh not needed. Next refresh at {nextRefreshTime.ToDbDateTimeString()} [{sw.Elapsed}]");
                    }

                    await Task.Delay(rateRefreshdelay, stoppingToken);
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

                var nextRefreshTime = DateTime.Now.Add(rateRefreshdelay);
                logger.LogInformation($"Error occured. Will refresh again at {nextRefreshTime} [{sw.Elapsed}]");
                await Task.Delay(rateRefreshdelay, stoppingToken);
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
            logger.LogInformation($"Can't process. No data returned from API [{sw.Elapsed}]");
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
        logger.LogInformation($"Refresh finised [{sw.Elapsed}]");
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

