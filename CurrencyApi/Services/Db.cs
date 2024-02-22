﻿using CurrencyApi.Models;
using MySql.Data.MySqlClient;
using System.Reflection;
using System.Text.Json;

namespace CurrencyApi.Services;

public class Db(ILogger<Db> logger, IConfiguration config):IDb
{
    private readonly string dbConString = GenerateConnectionString(config);

    private static string GenerateConnectionString(IConfiguration config)
    {
        var server = config["MySqlDb:Server"];
        var dbName = config["MySqlDb:DbName"];
        var userId = config["MySqlDb:UserId"];
        var encrypted = bool.Parse(config["MySqlDb:IsPasswordEncrypted"]);
        var password = config["MySqlDb:Password"]; //!encrypted ? config["AppDb:Password"] : AppSettingDecryptor.Decrypt(config["AppDb:Password"]);

        return $"Server={server};Database={dbName};User={userId};Password={password};";
    }

    #region Helper
    private static object? GetObjectValue(object obj)
    {
        if (obj == DBNull.Value) return null;
        else return obj;
    }

    private static string? GetStringValue(object obj)
    {
        if (obj == DBNull.Value) return null;
        else return obj.ToString();
    }

    private static byte[]? GetByteArrayValue(object obj)
    {
        if (obj == DBNull.Value) return null;
        else return (byte[])obj;
    }

    private static DateTime? GetDateTimeValue(object obj)
    {
        if (obj == DBNull.Value) return null;
        else return Convert.ToDateTime(obj);
    }

    private static decimal? GetDecimalValue(object obj)
    {
        if (obj == DBNull.Value) return null;
        else
        {
            return obj.ToString() == null ? null : decimal.Parse(obj.ToString());
        }
    }

    private static double? GetDoubleValue(object obj)
    {
        if (obj == DBNull.Value) return null;
        else
        {
            return obj.ToString() == null ? null : double.Parse(obj.ToString());
        }
    }

    private static int? GetIntValue(object obj)
    {
        if (obj == DBNull.Value) return null;
        else
        {
            return obj.ToString() == null ? null : int.Parse(obj.ToString());
        }
    }

    private static long? GetLongValue(object obj)
    {
        if (obj == DBNull.Value) return null;
        else
        {
            return obj.ToString() == null ? null : long.Parse(obj.ToString());
        }
    }

    /// <summary>
    /// Already handled if value is NULL or empty or whitespace
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static Dictionary<string, int> DeserializeDict(string value)
    {
        try
        {
            return string.IsNullOrWhiteSpace(value) ? [] : JsonSerializer.Deserialize<Dictionary<string, int>>(value);
        }
        catch
        {
            return [];
        }
    }

    private static List<T> DeserializeList<T>(string value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value))
                return [];

            var t = JsonSerializer.Deserialize<List<T>>(value);
            var generic = (List<T>)Convert.ChangeType(t, typeof(List<T>));

            return generic;
        }
        catch
        {
            return [];
        }
    }
    #endregion

    public async Task<List<Currency>> ListAllCurrencyAsync(CancellationToken ct)
    {
        try
        {
            List<Currency> data = [];
            string sql = $"SELECT * FROM currency;";

            using (MySqlConnection connection = new(this.dbConString))
            {
                await connection.OpenAsync(ct);
                using MySqlCommand cmd = new(sql, connection);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    data.Add(new()
                    {
                        CurrencyCode = GetStringValue(reader["id"]),
                        CountryCode = GetStringValue(reader["country_code"]),
                        CountryName = GetStringValue(reader["country_name"]),
                    });
                }
            }

            return data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return [];
        }
    }

    public async Task<List<CurrencyRate>> ListAllLatestRateAsync(CancellationToken ct)
    {
        try
        {
            List<CurrencyRate> data = [];
            string sql = $"SELECT * FROM latest_rate;";

            using (MySqlConnection connection = new(this.dbConString))
            {
                await connection.OpenAsync(ct);
                using MySqlCommand cmd = new(sql, connection);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    data.Add(new()
                    {
                        CurrencyCode = GetStringValue(reader["currency_code"]),
                        Rate = GetDecimalValue(reader["rate"]).Value,
                        AgainstOne = GetStringValue(reader["against_one"]),
                        UpdateTime = GetDateTimeValue(reader["update_time"]).Value,
                    });
                }
            }

            return data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return [];
        }
    }

    public async Task<CurrencyRate> GetLatestRateAsync(string currencyCode, CancellationToken ct)
    {
        try
        {
            CurrencyRate data = null;
            string sql = $"SELECT * FROM latest_rate WHERE currency_code = '{currencyCode}';";

            using (MySqlConnection connection = new(this.dbConString))
            {
                await connection.OpenAsync(ct);
                using MySqlCommand cmd = new(sql, connection);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    data = new()
                    {
                        CurrencyCode = currencyCode,
                        Rate = GetDecimalValue(reader["rate"]).Value,
                        AgainstOne = GetStringValue(reader["against_one"]),
                        UpdateTime = GetDateTimeValue(reader["update_time"]).Value,
                    };
                }
            }

            return data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return null;
        }
    }

}