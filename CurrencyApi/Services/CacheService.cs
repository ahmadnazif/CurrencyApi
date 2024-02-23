namespace CurrencyApi.Services;

public class CacheService
{
    public Dictionary<string, string> CurrenciesData { get; private set; }

    public void SetCurrenciesData(Dictionary<string, string> data) => CurrenciesData = data;
    public string GetCurrencyData(string currencyCode)
    {
        var success = CurrenciesData.TryGetValue(currencyCode, out string value);
        return success ? value : null;
    }
}
