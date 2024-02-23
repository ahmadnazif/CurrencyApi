using CurrencyApi.Models;
using MySql.Data.MySqlClient;
using System.Reflection;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CurrencyApi.Services;

public class Db(ILogger<Db> logger, IConfiguration config) : IDb
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

    /// <summary>
    /// Already handled if value is NULL or empty or whitespace
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    private static Dictionary<T1, T2> DeserializeDict<T1, T2>(string value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value))
                return [];

            var result = JsonSerializer.Deserialize<Dictionary<T1, T2>>(value);
            return result ?? [];
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

    #region Currency
    public async Task<int> CountAllCurrencyAsync(CancellationToken ct)
    {
        try
        {
            int total = 0;
            string sql = $"SELECT COUNT(*) FROM currency;";

            using (MySqlConnection connection = new(this.dbConString))
            {
                await connection.OpenAsync(ct);
                using MySqlCommand cmd = new(sql, connection);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    total = GetIntValue(reader[0]).Value;
                }
            }

            return total;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return 0;
        }
    }

    public async Task<PostResponse> InitializeCurrencyTableDataAsync(Dictionary<string, string> data, CancellationToken ct)
    {
        try
        {
            PostResponse resp = new() { IsSuccess = false };

            List<string> row = [];

            foreach (var d in data)
                row.Add($"('{d.Key}', '{d.Value.Replace("\'", string.Empty)}')");

            string query =
                $"INSERT INTO currency (id, currency_name) VALUES " +
                    $"{string.Join(",", row)} " +
                    $"ON DUPLICATE KEY UPDATE " +
                    $"currency_name = VALUES(currency_name);";

            using (MySqlConnection connection = new(this.dbConString))
            {
                await connection.OpenAsync(ct);
                using MySqlCommand cmd = new(query, connection);
                await cmd.ExecuteNonQueryAsync(ct);
                resp = new PostResponse
                {
                    IsSuccess = true
                };
            }

            return resp;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return new PostResponse
            {
                IsSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<Dictionary<string, string>> ListAllCurrencyAsync(CancellationToken ct)
    {
        try
        {
            Dictionary<string, string> data = [];
            string sql = $"SELECT * FROM currency;";

            using (MySqlConnection connection = new(this.dbConString))
            {
                await connection.OpenAsync(ct);
                using MySqlCommand cmd = new(sql, connection);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    data.Add(GetStringValue(reader["id"]), GetStringValue(reader["currency_name"]));
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

    #endregion

    #region Latest rates
    public async Task<List<CurrencyRate>> ListAllLatestRateAsync(CancellationToken ct)
    {
        try
        {
            List<CurrencyRate> data = [];
            string sql = $"SELECT * FROM latest_rate LEFT JOIN currency ON latest_rate.currency_code = currency.id;";

            using (MySqlConnection connection = new(this.dbConString))
            {
                await connection.OpenAsync(ct);
                using MySqlCommand cmd = new(sql, connection);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    data.Add(new()
                    {
                        Currency = new(GetStringValue(reader["currency_code"]), GetStringValue(reader["currency_name"])),
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
            string sql =
                $"SELECT * FROM latest_rate " +
                $"LEFT JOIN currency ON latest_rate.currency_code = currency.id " +
                $"WHERE latest_rate.currency_code = '{currencyCode}';";

            using (MySqlConnection connection = new(this.dbConString))
            {
                await connection.OpenAsync(ct);
                using MySqlCommand cmd = new(sql, connection);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    data = new()
                    {
                        Currency = new(GetStringValue(reader["currency_code"]), GetStringValue(reader["currency_name"])),
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

    public async Task<PostResponse> RefreshLatestRateAsync(List<CurrencyRateBase> data, CancellationToken ct)
    {
        try
        {
            PostResponse resp = new() { IsSuccess = false };

            List<string> row = [];

            foreach (var d in data)
                row.Add($"('{d.Currency.Key}', '{d.Rate}', '{d.AgainstOne}')");

            string query =
                $"INSERT INTO latest_rate (currency_code, rate, against_one) VALUES " +
                    $"{string.Join(",", row)} " +
                    $"ON DUPLICATE KEY UPDATE " +
                    $"rate = VALUES(rate), " +
                    $"against_one = VALUES(against_one);";

            using (MySqlConnection connection = new(this.dbConString))
            {
                await connection.OpenAsync(ct);
                using MySqlCommand cmd = new(query, connection);
                await cmd.ExecuteNonQueryAsync(ct);
                resp = new PostResponse
                {
                    IsSuccess = true
                };
            }

            return resp;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return new PostResponse
            {
                IsSuccess = false,
                Message = ex.Message
            };
        }
    }
    #endregion

    #region Rates history
    public async Task<List<CurrencyRateHistory>> ListAllRateHistoryAsync(DateOnly date, CancellationToken ct)
    {
        try
        {
            List<CurrencyRateHistory> data = [];
            string sql = $"SELECT * FROM rate_history WHERE created_time >= '{date.ToDbDateTimeString()}' AND created_time < '{date.AddDays(1).ToDbDateTimeString()}';";

            using (MySqlConnection connection = new(this.dbConString))
            {
                await connection.OpenAsync(ct);
                using MySqlCommand cmd = new(sql, connection);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    data.Add(new()
                    {
                        Time = GetDateTimeValue(reader["created_time"]).Value,
                        Rates = DeserializeDict<string, decimal>(GetStringValue(reader["data"]))
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

    public async Task<PostResponse> AddRateHistoryAsync(Dictionary<string, decimal> data, CancellationToken ct)
    {
        try
        {
            PostResponse resp = new() { IsSuccess = false };
            string query =
                "INSERT INTO rate_history (data) VALUES (@a);";

            using (MySqlConnection connection = new(this.dbConString))
            {
                await connection.OpenAsync(ct);
                using MySqlCommand cmd = new(query, connection);
                cmd.Parameters.AddWithValue("@a", JsonSerializer.Serialize(data));

                await cmd.ExecuteNonQueryAsync(ct);
                resp = new PostResponse
                {
                    IsSuccess = true
                };
            }

            return resp;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return new PostResponse
            {
                IsSuccess = false,
                Message = ex.Message
            };
        }
    }


    #endregion
    #region Setting
    public async Task<PostResponse> SaveSettingAsync(SettingId id, string value, CancellationToken ct)
    {
        try
        {
            PostResponse resp = new() { IsSuccess = false };
            string query =
                "INSERT INTO setting (setting_id, setting_value) VALUES " +
                "(@a, @b)" +
                "ON DUPLICATE KEY UPDATE " +
                "setting_value = @b;";

            using (MySqlConnection connection = new(this.dbConString))
            {
                await connection.OpenAsync(ct);
                using MySqlCommand cmd = new(query, connection);
                cmd.Parameters.AddWithValue("@a", (int)id);
                cmd.Parameters.AddWithValue("@b", value);

                await cmd.ExecuteNonQueryAsync(ct);
                resp = new PostResponse
                {
                    IsSuccess = true
                };
            }

            return resp;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return new PostResponse
            {
                IsSuccess = false,
                Message = ex.Message
            };
        }
    }
    public async Task<string> GetSettingAsync(SettingId id, CancellationToken ct)
    {
        try
        {
            string data = null;
            string sql = $"SELECT * FROM setting WHERE setting_id = '{(int)id}';";

            using (MySqlConnection connection = new(this.dbConString))
            {
                await connection.OpenAsync(ct);
                using MySqlCommand cmd = new(sql, connection);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    data = GetStringValue(reader["setting_value"]);
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
    #endregion
}
