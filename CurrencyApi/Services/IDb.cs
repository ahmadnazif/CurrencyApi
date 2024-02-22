using CurrencyApi.Models;

namespace CurrencyApi.Services;

public interface IDb
{
    Task<int> CountAllCurrencyAsync(CancellationToken ct = default);
    Task<PostResponse> InitializeCurrencyTableDataAsync(List<Currency> data, CancellationToken ct = default);
    Task<List<Currency>> ListAllCurrencyAsync(CancellationToken ct = default);
    Task<List<CurrencyRate>> ListAllLatestRateAsync(CancellationToken ct);
    Task<CurrencyRate> GetLatestRateAsync(string currencyCode, CancellationToken ct);
}
