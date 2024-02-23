namespace CurrencyApi.Models;

public class RateHistory
{
    public DateTime Time { get; set; }
    public Dictionary<string, decimal> Rates { get; set; }
}
