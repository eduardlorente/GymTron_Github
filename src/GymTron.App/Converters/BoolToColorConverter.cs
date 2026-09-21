using System.Globalization;

namespace GymTron.App.Converters;

public class BoolToColorConverter : IValueConverter
{


    private readonly Color LIGHT_GREEN = Color.FromRgb(144, 238, 144);
    private readonly Color LIGHT_GREY = Color.FromRgb(211, 211, 211);


    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCompleted)
        {
            return isCompleted ?
                LIGHT_GREEN :
                LIGHT_GREY;
        }
        return LIGHT_GREY;
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
