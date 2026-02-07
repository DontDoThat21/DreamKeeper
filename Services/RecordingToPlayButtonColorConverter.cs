using System.Globalization;

namespace DreamKeeper.Services
{
    /// <summary>
    /// Converts a byte[] (DreamRecording) to play button color.
    /// Non-null with length > 0 → Purple, otherwise → Gray
    /// </summary>
    public class RecordingToPlayButtonColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is byte[] recording && recording.Length > 0)
                return Colors.Purple;
            return Colors.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
