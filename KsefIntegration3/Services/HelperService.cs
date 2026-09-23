using System.Globalization;

namespace SkiControl.Services;

public static class HelperService
{
    private static readonly CultureInfo Culture = new CultureInfo("pl-PL");

    public static List<string> GetPolishMonths()
    {
        return Culture.DateTimeFormat.MonthNames
            .Where(m => !string.IsNullOrEmpty(m))
            .Select(m => Culture.TextInfo.ToTitleCase(m))
            .ToList();
    }

    public static List<string> GetQuarters() => new() { "I kwartał", "II kwartał", "III kwartał", "IV kwartał" };

    public static List<string> GetHalfYears() => new() { "I półrocze", "II półrocze" };

    public static List<string> GetIsoCountries()
    {
        return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(culture =>
            {
                try
                {
                    return new RegionInfo(culture.Name).TwoLetterISORegionName;
                }
                catch
                {
                    return null;
                }
            })
            .Where(code => !string.IsNullOrEmpty(code))
            .Distinct()
            .OrderBy(code => code)
            .ToList();
    }

    public static int GetMonthNumber(string monthName)
    {
        var months = GetPolishMonths();
        int index = months.FindIndex(m => m.Equals(monthName, StringComparison.OrdinalIgnoreCase));
        return index >= 0 ? index + 1 : DateTime.Now.Month;
    }

    public static int GetQuarterNumber(DateTime date) => (date.Month - 1) / 3 + 1;
}