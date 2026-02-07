using System.Globalization;

namespace DreamKeeper.Services
{
    /// <summary>
    /// Inverted visibility converter (bool → !bool).
    /// Used for showing label when not editing date, etc.
    /// </summary>
    public class BoolToInvertedVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return true;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return true;
        }
    }
}
