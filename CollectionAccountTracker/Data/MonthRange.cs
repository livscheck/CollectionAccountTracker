namespace CollectionAccountTracker.Data;

// The start and end of a calendar month, for filtering by date.
// Filter with  date >= Start && date < End  (exclusive end), so every time on the
// last day of the month is included, up to 11:59:59.999 PM.
public static class MonthRange
{
    public static (DateTime Start, DateTime End) For(int year, int month)
    {
        var start = new DateTime(year, month, 1);
        return (start, start.AddMonths(1));
    }

    public static (DateTime Start, DateTime End) For(DateTime date) => For(date.Year, date.Month);
}
