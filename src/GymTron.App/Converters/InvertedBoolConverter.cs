using System.Globalization;

namespace GymTron.App.Converters;

public class InvertedBoolConverter
{


    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return false;

        if (value is bool boolValue)
            return !boolValue;

        throw new InvalidOperationException("The value must be a boolean.");
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
