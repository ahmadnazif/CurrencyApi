namespace CurrencyApi.Models;

public class CurrencyRateHistory
{
    public DateTime Time { get; set; }
    public Dictionary<string, decimal> Rates { get; set; }
}
