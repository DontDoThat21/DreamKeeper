using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamKeeper.Data.Models
{
    public class AudioRecording
    {
        public int Id { get; set; }
        public byte[] AudioData { get; set; }
    }
}
