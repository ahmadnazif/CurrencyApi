
namespace CurrencyApi.Workers;

public class SimpleWorker(ILogger<SimpleWorker> logger) : BackgroundService
{
    private string data1 = null;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        data1 = $"Data created on {DateTime.Now}";

        while (!stoppingToken.IsCancellationRequested)
        {
            var data2 = $"Data created on {DateTime.Now}";
            logger.LogInformation($"Data1: {data1}");
            logger.LogInformation($"Data2: {data2}");
            await Task.Delay(5000, stoppingToken);
        }
    }
}
