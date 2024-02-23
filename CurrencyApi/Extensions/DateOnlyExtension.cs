namespace CurrencyApi.Extensions;

public static class DateOnlyExtension
{
    public static string ToDbDateString(this DateOnly date) => date.ToString("yyyy-MM-dd");

    /// <summary>
    /// yyyy-MM-dd HH:mm:ss with the smallest time segment (00:00:00)
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static string ToDbDateTimeString(this DateOnly date) => date.ToDateTime().ToDbDateTimeString();
    public static string ToUndashedDbDateString(this DateOnly date) => date.ToString("yyyyMMdd");

    /// <summary>
    /// This is basically <see cref="DateOnly.ToDateTime(TimeOnly)"/> with <see cref="TimeOnly.MinValue"/> (00:00:00)
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static DateTime ToDateTime(this DateOnly date) => date.ToDateTime(TimeOnly.MinValue);
}
