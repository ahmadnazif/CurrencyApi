namespace CurrencyApi.Helpers;

public class DateOnlyHelper
{
    private static string GetFormatString(DateTimeFormat format)
    {
        return format switch
        {
            DateTimeFormat.DbDate => DB_DATE_STRING,
            DateTimeFormat.DbDateTime => DB_DATETIME_STRING,
            DateTimeFormat.yyyyMMdd => "yyyyMMdd",
            _ => DB_DATETIME_STRING,
        };
    }

    public static DateOnly? Parse(string dateString, DateTimeFormat format)
    {
        var succ = DateTime.TryParseExact(dateString, GetFormatString(format), null, System.Globalization.DateTimeStyles.None, out var result);

        if (!succ)
            return null;

        return DateOnly.FromDateTime(result);
    }
}
