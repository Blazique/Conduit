namespace Conduit.Components;

internal static class DateTimeOffsetExtensions
{

    public static string FormatWithOrdinal(this DateTimeOffset date)
    {
        string suffix = (date.Day % 100) switch
        {
            11 or 12 or 13 => "th",
            _ => (date.Day % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th",
            },
        };
        return string.Format("{0:MMM} {1}{2}", date, date.Day, suffix);
    }
}