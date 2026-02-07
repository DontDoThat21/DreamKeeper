using CommunityToolkit.Maui.Views;

namespace DreamKeeper.Data
{
    /// <summary>
    /// Custom MediaElement that bridges in-memory byte[] audio data to the file-based MediaSource API.
    /// Writes the byte array to a temporary file and sets Source = MediaSource.FromFile(path).
    /// Defers source assignment until the native handler is connected to avoid silent failures
    /// when used inside a CollectionView DataTemplate.
    /// </summary>
    public class ByteArrayMediaElement : MediaElement
    {
        private byte[]? _pendingAudioData;
        private bool _handlerConnected;
        private string? _currentTempFile;

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

        public ByteArrayMediaElement()
        {
            ShouldShowPlaybackControls = true;
            ShouldAutoPlay = false;

            MediaOpened += OnMediaOpened;
        }

        private void OnMediaOpened(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"ByteArrayMediaElement: Media opened successfully. Duration: {Duration}");
        }

        private static void OnAudioDataChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ByteArrayMediaElement element && newValue is byte[] data && data.Length > 0)
            {
                if (element._handlerConnected)
                {
                    element.LoadAudioFromBytes(data);
                }
                else
                {
                    element._pendingAudioData = data;
                }
            }
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            _handlerConnected = Handler is not null;

            if (_handlerConnected && _pendingAudioData is { Length: > 0 })
            {
                LoadAudioFromBytes(_pendingAudioData);
                _pendingAudioData = null;
            }
            else if (!_handlerConnected)
            {
                CleanupOldTempFile();
            }
        }

        private void LoadAudioFromBytes(byte[] data)
        {
            try
            {
                CleanupOldTempFile();

                string extension;
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    extension = ".m4a";
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                    extension = ".mp4";
                else
                    extension = ".mp3";

                var fileName = $"dream_audio_{Guid.NewGuid()}{extension}";
                var tempPath = Path.Combine(FileSystem.CacheDirectory, fileName);

                File.WriteAllBytes(tempPath, data);
                _currentTempFile = tempPath;

                Stop();
                Source = MediaSource.FromFile(tempPath);

                System.Diagnostics.Debug.WriteLine($"ByteArrayMediaElement: Loaded audio file: {tempPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ByteArrayMediaElement error: {ex.Message}");
            }
        }

        public new void Play()
        {
            try
            {
                if (Source != null)
                {
                    base.Play();
                    System.Diagnostics.Debug.WriteLine("ByteArrayMediaElement: Play() called");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ByteArrayMediaElement: Play() called but Source is null");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ByteArrayMediaElement Play error: {ex.Message}");
            }
        }

        private void CleanupOldTempFile()
        {
            if (!string.IsNullOrEmpty(_currentTempFile) && File.Exists(_currentTempFile))
            {
                try
                {
                    File.Delete(_currentTempFile);
                }
                catch
                {
                }
            }
        }
    }
}
