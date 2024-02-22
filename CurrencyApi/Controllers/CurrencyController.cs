using CurrencyApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyApi.Controllers;

[Route("api/currency")]
[ApiController]
public class CurrencyController(IDb db) : ControllerBase
{
    private readonly IDb db = db;

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

}
