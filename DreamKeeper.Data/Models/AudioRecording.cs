namespace DreamKeeper.Data.Models
{
    /// <summary>
    /// Secondary DTO used by SQLiteDbService for standalone recording insert/query operations.
    /// </summary>
    public class AudioRecording
    {
        public int Id { get; set; }
        public byte[] AudioData { get; set; } = Array.Empty<byte>();
    }
}
