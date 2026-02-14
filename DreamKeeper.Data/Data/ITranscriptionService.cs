namespace DreamKeeper.Data.Data
{
    /// <summary>
    /// Abstracts speech-to-text transcription of recorded audio.
    /// </summary>
    public interface ITranscriptionService
    {
        /// <summary>
        /// Transcribes audio bytes (WAV format) to text.
        /// </summary>
        /// <param name="audioData">Raw WAV audio bytes.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The transcribed text, or empty string if transcription fails.</returns>
        Task<string> TranscribeAsync(byte[] audioData, CancellationToken cancellationToken = default);
    }
}
