namespace CurrencyApi.Models;

public class CurrencyConversionResponse : PostResponse
{
    public CurrencyRate FromRate { get; set; }
    public CurrencyRate ToRate { get; set; }
    public decimal Amount { get; set; }
    public decimal Result { get; set; }
    public string? Formula => $"{Result} = {ToRate.Rate} / {FromRate.Rate} * {Amount}";
}
