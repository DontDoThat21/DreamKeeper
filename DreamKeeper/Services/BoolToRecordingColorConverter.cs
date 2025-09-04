using System;
using System.Globalization;

namespace DreamKeeper.Services
{
    public class BoolToRecordingColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isRecording = (bool)value;
            return isRecording ? Colors.Red : Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}