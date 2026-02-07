using System.Globalization;

namespace DreamKeeper.Services
{
    /// <summary>
    /// Converts a byte[] to bool visibility.
    /// true if non-null and length > 0, false otherwise.
    /// </summary>
    public class ByteArrayToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is byte[] data && data.Length > 0)
                return true;
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
