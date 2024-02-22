global using static CurrencyApi.Constants;
global using CurrencyApi.Models;
global using CurrencyApi.Enums;
global using CurrencyApi.Helpers;
global using CurrencyApi.Extensions;
using CurrencyApi.Services;
using CurrencyApi.Workers;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddHostedService<CurrencyTableInitializer>();
builder.Services.AddHostedService<LatestRateProcessor>();

builder.Services.AddSingleton<IDb, Db>();
builder.Services.AddSingleton<CountryService>();
builder.Services.AddHttpClient<OpenExchangeRatesApi>(x => x.BaseAddress = new("https://openexchangerates.org/api/"));

builder.Services.AddCors(x => x.AddDefaultPolicy(y => y.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.ConfigureKestrel(x =>
{
    var httpPort = int.Parse(config["Port"]);
    x.ListenAnyIP(httpPort);
});

var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
