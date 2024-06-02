using AVFoundation;
using Foundation;
using DreamKeeper.Services;
using System.IO;
using System.Threading.Tasks;
using DreamKeeper.Data.Data;

[assembly: Dependency(typeof(YourNamespace.iOS.Services.AudioRecorderService))]
namespace YourNamespace.iOS.Services
{
    public class AudioRecorderService : IAudioRecorderService
    {
        private AVAudioRecorder _recorder;
        private NSUrl _url;
        private NSDictionary _settings;

        public async Task StartRecordingAsync()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filename = Path.Combine(documents, "tempRecording.m4a");
            _url = NSUrl.FromFilename(filename);

            var audioSettings = new AudioSettings
            {
                SampleRate = 44100.0,
                Format = AudioToolbox.AudioFormatType.MPEG4AAC,
                NumberChannels = 1,
                AudioQuality = AVAudioQuality.High
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

        public async Task<byte[]> StopRecordingAsync()
        {
            _recorder.Stop();
            return File.ReadAllBytes(_url.Path);
        }
    }
}
