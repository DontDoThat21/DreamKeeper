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
        
        private string? _dreamName = "Enter dream title here...";
        public string? DreamName 
        { 
            get => _dreamName;
            set
            {
                if (_dreamName != value)
                {
                    _dreamName = value;
                    OnPropertyChanged(nameof(DreamName));
                    HasUnsavedChanges = true;
                }
            }
        }
        
        private string? _dreamDescription = "Enter dream details here...";
        public string? DreamDescription 
        { 
            get => _dreamDescription;
            set
            {
                if (_dreamDescription != value)
                {
                    _dreamDescription = value;
                    OnPropertyChanged(nameof(DreamDescription));
                    HasUnsavedChanges = true;
                }
            }
        }
        
        private DateTime _dreamDate = DateTime.Now;
        public DateTime DreamDate 
        { 
            get => _dreamDate;
            set
            {
                if (_dreamDate != value)
                {
                    _dreamDate = value;
                    OnPropertyChanged(nameof(DreamDate));
                    HasUnsavedChanges = true;
                }
            }
        }
        
        private byte[]? _dreamRecording;
        public byte[]? DreamRecording
        {
            get => _dreamRecording;
            set
            {
                if (_dreamRecording != value)
                {
                    _dreamRecording = value;
                    OnPropertyChanged(nameof(DreamRecording));
                    HasUnsavedChanges = true;
                }
            }
        }
        
        private bool _hasUnsavedChanges;
        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                if (_hasUnsavedChanges != value)
                {
                    _hasUnsavedChanges = value;
                    OnPropertyChanged(nameof(HasUnsavedChanges));
                }
            }
        }

        private bool _isRecording;
        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                if (_isRecording != value)
                {
                    _isRecording = value;
                    OnPropertyChanged(nameof(IsRecording));
                }
            }
        }

        private bool _isEditingDate;
        public bool IsEditingDate
        {
            get => _isEditingDate;
            set
            {
                if (_isEditingDate != value)
                {
                    _isEditingDate = value;
                    OnPropertyChanged(nameof(IsEditingDate));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public void MarkAsSaved()
        {
            HasUnsavedChanges = false;
        }
    }
}
