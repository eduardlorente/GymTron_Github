using System.Globalization;
using GymTron.App.Services;

namespace GymTron.App.Helpers;

public static class DateUtils
{
    public static string MonthName(int month, CultureInfo? culture = null)
    {
        if (month < 1 || month > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }

        CultureInfo currentCulture = culture ?? LocalizationService.CurrentCulture ?? CultureInfo.CurrentCulture;
        return currentCulture.DateTimeFormat.GetMonthName(month);
    }

    public static string MonthNameInCatalan(int month)
        => MonthName(month, new CultureInfo("ca-ES"));
}
