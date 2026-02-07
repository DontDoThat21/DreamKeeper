using CommunityToolkit.Maui.Views;

namespace DreamKeeper.Data
{
    /// <summary>
    /// Custom MediaElement that bridges in-memory byte[] audio data to the file-based MediaSource API.
    /// Writes the byte array to a temporary file and sets Source = MediaSource.FromFile(path).
    /// </summary>
    public class ByteArrayMediaElement : MediaElement
    {
        public static readonly BindableProperty AudioDataProperty =
            BindableProperty.Create(
                nameof(AudioData),
                typeof(byte[]),
                typeof(ByteArrayMediaElement),
                null,
                propertyChanged: OnAudioDataChanged);

        public byte[]? AudioData
        {
            get => (byte[]?)GetValue(AudioDataProperty);
            set => SetValue(AudioDataProperty, value);
        }

        private static void OnAudioDataChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ByteArrayMediaElement element && newValue is byte[] data && data.Length > 0)
            {
                element.LoadAudioFromBytes(data);
            }
        }

        private void LoadAudioFromBytes(byte[] data)
        {
            try
            {
                // Determine platform-appropriate extension
                string extension;
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    extension = ".m4a";
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                    extension = ".mp4";
                else
                    extension = ".mp3"; // Windows/default

                var fileName = $"dream_audio_{Guid.NewGuid()}{extension}";
                var tempPath = Path.Combine(FileSystem.CacheDirectory, fileName);

                File.WriteAllBytes(tempPath, data);
                Source = MediaSource.FromFile(tempPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ByteArrayMediaElement error: {ex.Message}");
            }
        }
    }
}
