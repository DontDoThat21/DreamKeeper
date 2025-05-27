using Android.Media;
using DreamKeeper.Data.Data;
using System.IO;
using System.Threading.Tasks;
using System;

[assembly: Dependency(typeof(DreamKeeper.Platforms.Android.Services.AudioRecorderService))]
namespace DreamKeeper.Platforms.Android.Services
{
    public class AudioRecorderService : IAudioRecorderService
    {
        private MediaRecorder _recorder;
        private string _filePath;

        public async Task StartRecordingAsync()
        {
            try
            {
                // Create a unique filename to avoid conflicts
                string fileName = DreamKeeper.Services.AudioFileHelper.GetUniqueAudioFilename();
                _filePath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, fileName);
                
                _recorder = new MediaRecorder();
                _recorder.SetAudioSource(AudioSource.Mic);
                _recorder.SetOutputFormat(OutputFormat.Mpeg4);
                _recorder.SetAudioEncoder(AudioEncoder.Aac); // Use AAC encoder for better compatibility
                _recorder.SetAudioSamplingRate(44100); // Standard sample rate
                _recorder.SetAudioEncodingBitRate(128000); // Better quality
                _recorder.SetOutputFile(_filePath);
                _recorder.Prepare();
                _recorder.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting recording: {ex.Message}");
                throw;
            }
        }

        public async Task<byte[]> StopRecordingAsync()
        {
            try
            {
                _recorder.Stop();
                _recorder.Release();
                
                // Read the file into a byte array
                if (File.Exists(_filePath))
                {
                    var bytes = File.ReadAllBytes(_filePath);
                    
                    // Clean up the temporary file
                    try { File.Delete(_filePath); } catch {}
                    
                    return bytes;
                }
                return new byte[0];
            }
            catch (Java.Lang.IllegalStateException)
            {
                // Sometimes stop() can throw if recording was too short
                _recorder.Release();
                return new byte[0];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping recording: {ex.Message}");
                if (_recorder != null)
                {
                    _recorder.Release();
                }
                return new byte[0];
            }
        }
    }
}
