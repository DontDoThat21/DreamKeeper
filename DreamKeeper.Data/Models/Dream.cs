using System.ComponentModel;

namespace DreamKeeper.Data.Models
{
    public class Dream : INotifyPropertyChanged
    {
        private int _id;
        private string? _dreamName = "Enter dream title here...";
        private string? _dreamDescription = "Enter dream details here...";
        private DateTime _dreamDate = DateTime.Now;
        private byte[]? _dreamRecording;
        private bool _hasUnsavedChanges;
        private bool _isRecording;
        private bool _isEditingDate;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string? DreamName
        {
            get => _dreamName;
            set
            {
                if (_dreamName != value)
                {
                    _dreamName = value;
                    HasUnsavedChanges = true;
                    OnPropertyChanged(nameof(DreamName));
                }
            }
        }

        public string? DreamDescription
        {
            get => _dreamDescription;
            set
            {
                if (_dreamDescription != value)
                {
                    _dreamDescription = value;
                    HasUnsavedChanges = true;
                    OnPropertyChanged(nameof(DreamDescription));
                }
            }
        }

        public DateTime DreamDate
        {
            get => _dreamDate;
            set
            {
                if (_dreamDate != value)
                {
                    _dreamDate = value;
                    HasUnsavedChanges = true;
                    OnPropertyChanged(nameof(DreamDate));
                }
            }
        }

        public byte[]? DreamRecording
        {
            get => _dreamRecording;
            set
            {
                if (_dreamRecording != value)
                {
                    _dreamRecording = value;
                    HasUnsavedChanges = true;
                    OnPropertyChanged(nameof(DreamRecording));
                }
            }
        }

        // UI-only properties - do NOT trigger dirty tracking
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

        /// <summary>
        /// Resets the dirty flag after a successful save.
        /// </summary>
        public void MarkAsSaved()
        {
            HasUnsavedChanges = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
