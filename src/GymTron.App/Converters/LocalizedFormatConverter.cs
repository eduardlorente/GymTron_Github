using System.Globalization;
using GymTron.App.Services;

namespace GymTron.App.Converters;

public class LocalizedFormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter is not string formatKey)
            return value?.ToString();

        var format = LocalizationService.GetString(formatKey);
        return string.Format(format, value);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
