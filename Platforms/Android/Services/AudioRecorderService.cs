using Android.Media;
using DreamKeeper.Data.Data;

namespace DreamKeeper.Platforms.Android.Services
{
    /// <summary>
    /// Android AudioRecorderService using MediaRecorder for native audio capture.
    /// </summary>
    public class AudioRecorderService : IAudioRecorderService
    {
        private MediaRecorder? _mediaRecorder;
        private string? _filePath;

        public async Task StartRecordingAsync()
        {
            _filePath = Path.Combine(FileSystem.CacheDirectory, $"dream_recording_{Guid.NewGuid()}.mp4");

            _mediaRecorder = new MediaRecorder();
            _mediaRecorder.SetAudioSource(AudioSource.Mic);
            _mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
            _mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);
            _mediaRecorder.SetAudioEncodingBitRate(128000);
            _mediaRecorder.SetAudioSamplingRate(44100);
            _mediaRecorder.SetOutputFile(_filePath);
            _mediaRecorder.Prepare();
            _mediaRecorder.Start();

            await Task.CompletedTask;
        }

        public async Task<byte[]> StopRecordingAsync()
        {
            try
            {
                if (_mediaRecorder != null)
                {
                    _mediaRecorder.Stop();
                    _mediaRecorder.Release();
                    _mediaRecorder = null;
                }

                if (_filePath != null && File.Exists(_filePath))
                {
                    var bytes = await File.ReadAllBytesAsync(_filePath);
                    File.Delete(_filePath);
                    return bytes;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android StopRecording error: {ex.Message}");
            }

            return Array.Empty<byte>();
        }
    }
}
