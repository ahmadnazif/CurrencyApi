using CurrencyApi.Models;

namespace CurrencyApi.Services;

public interface IDb
{
    Task<int> CountAllCurrencyAsync(CancellationToken ct = default);
    Task<PostResponse> InitializeCurrencyTableDataAsync(List<Currency> data, CancellationToken ct = default);
    Task<List<Currency>> ListAllCurrencyAsync(CancellationToken ct = default);

    Task<List<CurrencyRate>> ListAllLatestRateAsync(CancellationToken ct);
    Task<CurrencyRate> GetLatestRateAsync(string currencyCode, CancellationToken ct);
    Task<PostResponse> RefreshLatestRateAsync(List<CurrencyRateBase> data, CancellationToken ct = default);

    Task<List<CurrencyRateHistory>> ListAllRateHistoryAsync(DateOnly date, CancellationToken ct = default);
    Task<PostResponse> AddRateHistoryAsync(Dictionary<string, decimal> data, CancellationToken ct = default);

    Task<PostResponse> SaveSettingAsync(SettingId id, string value, CancellationToken ct);
    Task<string> GetSettingAsync(SettingId id, CancellationToken ct);
}
