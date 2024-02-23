namespace CurrencyApi.Models;

public class CurrencyConversionResponse : PostResponse
{
    public CurrencyRate From { get; set; }
    public CurrencyRate To { get; set; }
    public decimal Amount { get; set; }
    public decimal Result { get; set; }
    public string? Formula => $"{Result} = {To.Rate} / {From.Rate} * {Amount}";
}
