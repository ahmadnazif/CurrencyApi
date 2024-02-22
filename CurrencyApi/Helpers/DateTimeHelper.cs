using System.Runtime.Serialization;
using DateTimeFormat = CurrencyApi.Enums.DateTimeFormat;

namespace CurrencyApi.Helpers;

public class DateTimeHelper
{
    private static string GetFormatString(DateTimeFormat format)
    {
        return format switch
        {
            DateTimeFormat.DbDate => DB_DATE_STRING,
            DateTimeFormat.DbDateTime => DB_DATETIME_STRING,
            _ => DB_DATETIME_STRING,
        };
    }

    public static DateTime? Parse(string dateString, DateTimeFormat format)
    {
        var succ = DateTime.TryParseExact(dateString, GetFormatString(format), null, System.Globalization.DateTimeStyles.None, out var result);

        if (succ) return result;
        else return null;
    }

    public static List<DateTime> ListHours(DateTime from, DateTime to)
    {
        var hourList = Enumerable.Range(0, 1 + (int)to.Subtract(from).TotalHours).Select(offset => from.AddHours(offset)).ToList();
        return hourList;
    }

    public static List<DateTime> ListDates(DateTime from, DateTime to)
    {
        var dateList = Enumerable.Range(0, 1 + (int)to.Subtract(from).TotalDays).Select(offset => from.AddDays(offset)).ToList();
        return dateList;
    }
}
