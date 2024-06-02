using Android.Media;
using YourNamespace.Services;
using System.IO;
using System.Threading.Tasks;
using Plugin.Maui.Audio;
using System.Runtime.CompilerServices;

[assembly: Dependency(typeof(YourNamespace.Droid.Services.AudioRecorderService))]
namespace YourNamespace.Droid.Services
{
    public class AudioRecorderService : IAudioRecorderService
    {
        private MediaRecorder _recorder;
        private string _filePath;

        public async Task StartRecordingAsync()
        {
            _filePath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "tempRecording.mp4");
            _recorder = new MediaRecorder();
            _recorder.SetAudioSource(AudioSource.Mic);
            _recorder.SetOutputFormat(OutputFormat.Mpeg4);
            _recorder.SetAudioEncoder(AudioEncoder.AmrNb);
            _recorder.SetOutputFile(_filePath);
            _recorder.Prepare();
            _recorder.Start();
        }

        public async Task<byte[]> StopRecordingAsync()
        {
            _recorder.Stop();
            _recorder.Release();
            return File.ReadAllBytes(_filePath);
        }
    }
}
