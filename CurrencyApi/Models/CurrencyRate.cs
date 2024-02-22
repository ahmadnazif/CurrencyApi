namespace CurrencyApi.Models;

public class CurrencyRate : CurrencyRateBase
{
    public DateTime UpdateTime { get; set; }
}

public class CurrencyRateBase
{
    public string? CurrencyCode { get; set; }
    public decimal Rate { get; set; }
    public string? AgainstOne { get; set; }
}
