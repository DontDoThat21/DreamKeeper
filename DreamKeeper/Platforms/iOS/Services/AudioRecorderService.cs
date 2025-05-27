using AVFoundation;
using Foundation;
using DreamKeeper.Data.Data;
using System.IO;
using System.Threading.Tasks;
using System;

[assembly: Dependency(typeof(DreamKeeper.Platforms.iOS.Services.AudioRecorderService))]
namespace DreamKeeper.Platforms.iOS.Services
{
    public class AudioRecorderService : IAudioRecorderService
    {
        private AVAudioRecorder _recorder;
        private NSUrl _url;

        public async Task StartRecordingAsync()
        {
            try
            {
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filename = Path.Combine(documents, DreamKeeper.Services.AudioFileHelper.GetUniqueAudioFilename());
                _url = NSUrl.FromFilename(filename);

                var audioSettings = new AudioSettings
                {
                    SampleRate = 44100.0,
                    Format = AudioToolbox.AudioFormatType.MPEG4AAC,
                    NumberChannels = 1,
                    AudioQuality = AVAudioQuality.High,
                    BitRate = 128000 // Higher quality
                };

                NSError error;
                _recorder = AVAudioRecorder.Create(_url, audioSettings, out error);
                if (error != null)
                {
                    // Handle error
                    throw new Exception(error.LocalizedDescription);
                }

                _recorder.Record();
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
                
                // Read the file into a byte array
                if (File.Exists(_url.Path))
                {
                    var bytes = File.ReadAllBytes(_url.Path);
                    
                    // Clean up the temporary file
                    try { File.Delete(_url.Path); } catch {}
                    
                    return bytes;
                }
                return new byte[0];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping recording: {ex.Message}");
                return new byte[0];
            }
        }
    }
}
