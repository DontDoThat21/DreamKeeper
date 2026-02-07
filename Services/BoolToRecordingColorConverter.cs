using System.Globalization;

namespace DreamKeeper.Services
{
    /// <summary>
    /// Converts a bool (IsRecording) to record button color.
    /// true → Red, false → Gray
    /// </summary>
    public class BoolToRecordingColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isRecording)
                return isRecording ? Colors.Red : Colors.Gray;
            return Colors.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
