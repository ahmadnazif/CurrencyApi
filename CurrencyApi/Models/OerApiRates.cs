using System.Text.Json.Serialization;

namespace CurrencyApi.Models;

public class OerApiRates
{
    public long Timestamp { get; set; }
    [JsonPropertyName("base")] public string BaseCurrency { get; set; } = null!;
    public IReadOnlyDictionary<string, decimal> Rates { get; set; } = null!;
}
