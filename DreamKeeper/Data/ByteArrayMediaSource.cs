using CommunityToolkit.Maui.Views;
using System.IO;

namespace DreamKeeper.Data
{
    public class ByteArrayMediaSource : MediaSource
    {
        public byte[] Data { get; set; }

        public ByteArrayMediaSource(byte[] data)
        {
            Data = data;
        }

        public Stream GetStream()
        {
            if (Data != null && Data.Length > 0)
            {
                return new MemoryStream(Data);
            }
            return null;
        }
    }
}
