using System.Net.Http.Headers;
using System.Text.Json;

namespace CurrencyApi.Services;

public class OpenExchangeRatesApi
{
    private readonly ILogger<OpenExchangeRatesApi> logger;
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions JSON_OPT = new() { PropertyNameCaseInsensitive = true };

    public OpenExchangeRatesApi(ILogger<OpenExchangeRatesApi> logger, IConfiguration config, HttpClient httpClient)
    {
        this.logger = logger;
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", config["OpenExchangeRates:AppId"]);
        this.httpClient = httpClient;
    }

    public async Task<OerApiRates> GetLatestRatesAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"latest.json?prettyprint=false&show_alternative=false", ct);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<OerApiRates>(cancellationToken: ct);
            else
            {
                logger.LogError(response.StatusCode.ToString());
                return null;
            }
        }
        catch(Exception ex)
        {
            logger.LogError($"Exception: {ex.Message}");
            return null;
        }
    }

}
