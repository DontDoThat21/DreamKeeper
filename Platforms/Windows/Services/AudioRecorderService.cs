using DreamKeeper.Data.Data;

namespace DreamKeeper.Platforms.Windows.Services
{
    /// <summary>
    /// Windows AudioRecorderService stub.
    /// The primary recording path on Windows uses Plugin.Maui.Audio directly in the ViewModel.
    /// This stub satisfies the DI registration for IAudioRecorderService.
    /// </summary>
    public class AudioRecorderService : IAudioRecorderService
    {
        public Task StartRecordingAsync()
        {
            // Windows uses Plugin.Maui.Audio directly via the ViewModel
            return Task.CompletedTask;
        }

        public Task<byte[]> StopRecordingAsync()
        {
            // Windows uses Plugin.Maui.Audio directly via the ViewModel
            return Task.FromResult(Array.Empty<byte>());
        }
    }
}
