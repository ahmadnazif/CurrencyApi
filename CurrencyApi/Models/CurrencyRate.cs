namespace CurrencyApi.Models;

public class CurrencyRate
{
    public string? CurrencyCode { get; set; }
    public decimal Rate { get; set; }
    public string? AgainstOne { get; set; }
    public DateTime UpdateTime { get; set; }
}
