namespace DreamKeeper.Data
{
    /// <summary>
    /// Wraps a byte[] and provides GetStream() returning a MemoryStream.
    /// Used as a fallback approach for playback.
    /// </summary>
    public class ByteArrayMediaSource
    {
        public byte[]? Data { get; set; }

        public Stream? GetStream()
        {
            if (Data != null && Data.Length > 0)
                return new MemoryStream(Data);
            return null;
        }
    }
}
