using DreamKeeper.Models;
using DreamKeeper.Services;
using Plugin.AudioRecorder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DreamKeeper.ViewModels
{
    public class DreamsViewModel
    {
        private readonly DreamService _dreamService;
        private readonly AudioRecorderService _audioRecorderService;
        private bool _isRecording;
        private string _recordingDuration;
        public ObservableCollection<Dream> Dreams { get; set; }

        public DreamsViewModel(DreamService dreamService)
        {
            _dreamService = dreamService;
            Dreams = _dreamService.GetDreams();
            DeleteDreamCommand = new Command<Dream>(async (dream) => await OnDeleteDream(dream));
            StartRecordingCommand = new Command<Dream>(async (dream) => await OnStartRecording(dream));
            _audioRecorderService = new AudioRecorderService();
            _elapsedTime = TimeSpan.Zero;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task OnDeleteDream(Dream dream)
        {
            if (dream != null)
            {
                bool isConfirmed = await App.Current.MainPage.DisplayAlert(
                    "Confirm Delete",
                    "Are you sure you want to delete this dream?",
                    "Yes",
                    "No");

                if (isConfirmed)
                {
                    _dreamService.DeleteDream(dream.Id);
                    Dreams.Remove(dream);
                }
            }
        }

        public ICommand DeleteDreamCommand { get; }
        public ICommand StartRecordingCommand { get; }


        private TimeSpan _elapsedTime;
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);

        private async Task OnStartRecording(Dream dream)
        {
            if (_isRecording)
            {
                // Stop recording
                _isRecording = false;
                await _audioRecorderService.StopRecording();
                RecordingDuration = string.Empty;
            }
            else
            {
                // Start recording
                await _audioRecorderService.StartRecording();
                _isRecording = true;
                Device.StartTimer(_updateInterval, () =>
                {
                    _elapsedTime += _updateInterval;
                    RecordingDuration = _elapsedTime.ToString(@"hh\:mm\:ss");
                    return _isRecording;
                });
            }
        }

        public string RecordingDuration
        {
            get => _recordingDuration;
            set
            {
                _recordingDuration = value;
                OnPropertyChanged(nameof(RecordingDuration));
            }
        }

    }
}
