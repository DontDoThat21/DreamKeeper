using System.Globalization;

namespace DreamKeeper.Services
{
    /// <summary>
    /// Converts a bool (IsRecording) to record button text.
    /// true → "Stop Recording", false → "Start Recording"
    /// </summary>
    public class BoolToRecordingTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isRecording)
                return isRecording ? "Stop Recording" : "Start Recording";
            return "Start Recording";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
