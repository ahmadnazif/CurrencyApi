using CurrencyApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyApi.Controllers;

[Route("api/currency")]
[ApiController]
public class CurrencyController(IDb db, OpenExchangeRatesApi api) : ControllerBase
{
    private readonly IDb db = db;
    private readonly OpenExchangeRatesApi api = api;

    [HttpGet("list-all-currency")]
    public async Task<ActionResult<List<Currency>>> ListAllCurrency(CancellationToken ct)
    {
        return await db.ListAllCurrencyAsync(ct);
    }

    [HttpGet("list-all-latest-rate")]
    public async Task<ActionResult<List<CurrencyRate>>> ListAllLatestRate(CancellationToken ct)
    {
        return await db.ListAllLatestRateAsync(ct);
    }

    [HttpGet("get-latest-rate")]
    public async Task<ActionResult<CurrencyRate>> GetLatestRate([FromQuery] string? currencyCode, CancellationToken ct)
    {
        return await db.GetLatestRateAsync(currencyCode, ct);
    }

    [HttpGet("convert")]
    public async Task<ActionResult<decimal>> Convert([FromQuery] string? from, [FromQuery] string? to, decimal amount, CancellationToken ct)
    {
        var a = await db.GetLatestRateAsync(from, ct);
        var b = await db.GetLatestRateAsync(to, ct);

        if (a == null)
            return 0;

        if (b == null)
            return 0;

        var result = b.Rate / a.Rate * amount;
        return result;
    }


}
