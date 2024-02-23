using CurrencyApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CurrencyApi.Controllers;

[Route("api/currency")]
[ApiController]
public class CurrencyController(IDb db, CacheService cache) : ControllerBase
{
    private readonly IDb db = db;

    [HttpGet("list-all-currency")]
    public async Task<ActionResult<Dictionary<string, string>>> ListAllCurrency(CancellationToken ct)
    {
        return await Task.FromResult(cache.CurrenciesData); //db.ListAllCurrencyAsDictionaryAsync(ct);
    }

    [HttpGet("list-all-latest-rate")]
    public async Task<ActionResult<List<CurrencyRate>>> ListAllLatestRate(CancellationToken ct)
    {
        return await db.ListAllLatestRateAsync(ct);
    }

    [HttpGet("get-latest-rate")]
    public async Task<ActionResult<CurrencyRate>> GetLatestRate([FromQuery] string currencyCode, CancellationToken ct)
    {
        return await db.GetLatestRateAsync(currencyCode, ct);
    }

    [HttpGet("list-all-rate-history")]
    public async Task<ActionResult<List<CurrencyRateHistory>>> ListAllRateHistory([FromQuery] string date, CancellationToken ct)
    {
        var d = DateOnlyHelper.Parse(date, DateTimeFormat.DbDate);

        if (!d.HasValue)
            return new List<CurrencyRateHistory>();

        return await db.ListAllRateHistoryAsync(d.Value, ct);
    }

    [HttpGet("convert")]
    public async Task<ActionResult<CurrencyConversionResponse>> Convert([FromQuery] string from, [FromQuery] string to, decimal amount, CancellationToken ct)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var f = await db.GetLatestRateAsync(from, ct);
        var t = await db.GetLatestRateAsync(to, ct);

        if (f == null)
            return new CurrencyConversionResponse { IsSuccess = false, Message = $"No rate data for currency '{from}'" };

        if (t == null)
            return new CurrencyConversionResponse { IsSuccess = false, Message = $"No rate data for currency '{to}'" };

        var result = t.Rate / f.Rate * amount;
        sw.Stop();

        return new CurrencyConversionResponse
        {
            IsSuccess = true,
            From = f,
            To = t,
            Amount = amount,
            Result = result,
            Message = $"Elapsed {sw.Elapsed}"
        };
    }


}
