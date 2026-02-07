using System.Globalization;

namespace DreamKeeper.Services
{
    /// <summary>
    /// Pass-through converter for IsVisible binding (bool → bool).
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue;
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue;
            return false;
        }
    }
}
