using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamKeeper.Models
{
    public class Dream : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string ?DreamName { get; set; } = "Enter dream title here...";
        public string? DreamDescription { get; set; } = "Enter dream details here...";
        public DateTime DreamDate { get; set; } = DateTime.Now;
        private byte[]? _dreamRecording;
        public byte[]? DreamRecording
        {
            get => _dreamRecording;
            set
            {
                _dreamRecording = value;
                OnPropertyChanged(nameof(DreamRecording));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
