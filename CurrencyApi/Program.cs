global using CurrencyApi.Models;
using CurrencyApi.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddSingleton<IDb, Db>();
builder.Services.AddSingleton<Country>();

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
