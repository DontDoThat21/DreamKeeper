using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamKeeper.Data.Data
{
    public interface IAudioRecorderService
    {
        Task StartRecordingAsync();
        Task<byte[]> StopRecordingAsync();
    }
}
