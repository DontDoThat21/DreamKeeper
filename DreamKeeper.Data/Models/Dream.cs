using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamKeeper.Models
{
    public class Dream
    {
        public int Id { get; set; }
        public string ?DreamName { get; set; } = "Enter dream title here...";
        public string? DreamDescription { get; set; } = "Enter dream details here...";
        public DateTime DreamDate { get; set; } = DateTime.Now;
        public byte[] ?DreamRecording { get; set; }
    }
}
