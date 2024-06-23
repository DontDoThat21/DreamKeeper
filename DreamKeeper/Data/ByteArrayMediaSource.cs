using CommunityToolkit.Maui.Views;

namespace DreamKeeper.Data
{
    public class ByteArrayMediaSource : MediaSource
    {
        public byte[] Data { get; set; }

        public ByteArrayMediaSource(byte[] data)
        {
            Data = data;
        }
    }
}
