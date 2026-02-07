namespace DreamKeeper.Data.Data
{
    /// <summary>
    /// Platform-abstraction interface for native audio recording.
    /// </summary>
    public interface IAudioRecorderService
    {
        /// <summary>
        /// Begins audio capture.
        /// </summary>
        Task StartRecordingAsync();

        /// <summary>
        /// Stops capture, returns byte[] of recorded audio.
        /// </summary>
        Task<byte[]> StopRecordingAsync();
    }
}
