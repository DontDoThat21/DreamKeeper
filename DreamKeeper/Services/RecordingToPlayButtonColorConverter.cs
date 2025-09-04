using System;
using System.Globalization;

namespace DreamKeeper.Services
{
    public class RecordingToPlayButtonColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte[] recording = value as byte[];
            bool hasRecording = recording != null && recording.Length > 0;
            
            // Purple when recording exists, gray when no recording
            return hasRecording ? Colors.Purple : Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}