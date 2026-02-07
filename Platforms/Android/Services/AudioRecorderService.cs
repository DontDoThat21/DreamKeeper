using DreamKeeper.Data.Data;

namespace DreamKeeper.Platforms.Android.Services
{
    /// <summary>
    /// Android AudioRecorderService using MediaRecorder as a fallback mechanism.
    /// The primary recording path uses Plugin.Maui.Audio.
    /// </summary>
    public class AudioRecorderService : IAudioRecorderService
    {
        private string? _filePath;

        public async Task StartRecordingAsync()
        {
            _filePath = Path.Combine(FileSystem.CacheDirectory, $"dream_recording_{Guid.NewGuid()}.mp4");

            // Use Plugin.Maui.Audio as the primary path; this is a fallback stub
            // A full MediaRecorder implementation would go here for native Android recording
            await Task.CompletedTask;
        }

        public async Task<byte[]> StopRecordingAsync()
        {
            if (_filePath != null && File.Exists(_filePath))
            {
                var bytes = await File.ReadAllBytesAsync(_filePath);
                File.Delete(_filePath);
                return bytes;
            }

            return Array.Empty<byte>();
        }
    }
}
