using DreamKeeper.Data.Models;
using DreamKeeper.Data.Services;
using DreamKeeper.Models;
using DreamKeeper.Services;
using Plugin.AudioRecorder;
using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DreamKeeper.ViewModels
{
    public class DreamsViewModel
    {
        private readonly DreamService _dreamService;
        private readonly IAudioManager _audioManager;
        private readonly IAudioRecorder _audioRecorder;
        private bool _isRecording;
        private string _recordingDuration;
        private int _lastRecordingId;

        public ICommand ToggleRecordingCommand { get; set; }
        public ICommand PlayRecordingCommand { get; }

        public ObservableCollection<Dream> Dreams { get; set; }

        public DreamsViewModel(DreamService dreamService, IAudioManager audioManager)
        {
            _dreamService = dreamService;
            Dreams = _dreamService.GetDreams();
            //DeleteDreamCommand = new Command<Dream>(async (dream) => await OnDeleteDream(dream));
            _audioManager = audioManager;
            _audioRecorder = audioManager.CreateRecorder();
            _elapsedTime = TimeSpan.Zero;

            ToggleRecordingCommand = new Command(async () => await ToggleRecording());
            //StopRecordingCommand = new Command(async () => await StopRecording());
            PlayRecordingCommand = new Command(async () => await PlayRecording());

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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


        private TimeSpan _elapsedTime;
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);


        public async Task ToggleRecording()
        {
            if (_isRecording)
            {
                await StopRecording();
                _isRecording = false;
            }
            else
            {
                await StartRecording();
                _isRecording = true;
            }
            OnPropertyChanged(nameof(_isRecording));
        }

        private async Task StartRecording()
        {
            await this._audioRecorder.StartAsync();

        }

        public async Task StopRecording()
        {
            IAudioSource audioSource = await this._audioRecorder.StopAsync();
            Stream audioStream = audioSource.GetAudioStream();

            // Define the local file path
            string filePath;
#if ANDROID
    var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    filePath = Path.Combine(documentsPath, "recording.m4a");
#elif IOS
    var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    filePath = Path.Combine(documentsPath, "recording.m4a");
#elif WINDOWS
    var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    filePath = Path.Combine(documentsPath, "recording.m4a");
#else
    var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    filePath = Path.Combine(documentsPath, "recording.m4a");
#endif
            // Save the stream to the file
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await audioStream.CopyToAsync(fileStream);
            }

            audioStream.Close();
        }

        private async Task PlayRecording()
        {
            var recording = await SQLiteDbService.GetRecordingAsync(_lastRecordingId);
            if (recording != null)
            {
                PlayAudio(recording.AudioData);
            }
        }

        public async void PlayAudio(byte[] audioData)
        {
            // Create a temporary file to store the audio data
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var tempFilePath = Path.Combine(documentsPath, "tempRecording.m4a");

            // Write the audio data to the file
            await File.WriteAllBytesAsync(tempFilePath, audioData);

            // Create an audio player to play the temporary file
            var audioPlayer = _audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(tempFilePath));

            // Play the audio
            audioPlayer.Play();
        }

    }
}
