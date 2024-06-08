namespace Static.Services;

public static class Utils
{
    public static string Replace(this string strTemplate, Dictionary<string, string> dictParams)
    {
        foreach (KeyValuePair<string, string> item in dictParams)
        {
            if (strTemplate.Contains(item.Key))
            {
                strTemplate = strTemplate.Replace(item.Key, item.Value);
            }
        }

        return strTemplate;
    }

    public static DateTime ParseDateTimeFromString(string dateString, string format)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dateString, nameof(dateString));

        if (DateTime.TryParseExact(dateString, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dateTime))
        {
            return dateTime;
        }

        throw new FormatException($"{nameof(dateString)} should be in ${format}");
    }

    public static DateTime ConvertToDate(this string dateString)
    {
        return ParseDateTimeFromString(dateString.Substring(0,10), "yyyy-MM-dd");
    }

    public static DateTime ConvertToDateTime(this string dateString)
    {
        return ParseDateTimeFromString(dateString, "yyyy-MM-ddTHH:mm:ss");
    }

    public static IEnumerable<DateTime> ConvertListToDate(this IEnumerable<string> dateStrings)
    {
        return dateStrings.Select(x => x.ConvertToDate()).ToList();
    }

    public static string ToProcessTimeString(this TimeSpan timeSpan)
    {
        string result = string.Empty;

        if (timeSpan.Days > 0)
        {
            result += $"{timeSpan.Days} day{(timeSpan.Days > 1 ? "s" : "")} ";
        }

        if (timeSpan.Hours > 0 || timeSpan.Days > 0)
        {
            result += $"{timeSpan.Hours} hour{(timeSpan.Hours > 1 ? "s" : "")} ";
        }

        if (timeSpan.Minutes > 0 || timeSpan.Hours > 0 || timeSpan.Days > 0)
        {
            result += $"{timeSpan.Minutes} min{(timeSpan.Minutes > 1 ? "s" : "")} ";
        }

        result += $"{timeSpan.Seconds} sec{(timeSpan.Seconds > 1 ? "s" : "")}";

        return result.Trim();
    }
}
