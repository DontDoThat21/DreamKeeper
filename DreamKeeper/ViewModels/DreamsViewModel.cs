using CommunityToolkit.Maui.Views;
using Dapper;
using DreamKeeper.Data;
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
    public class DreamsViewModel : INotifyPropertyChanged
    {
        private readonly DreamService _dreamService;
        private readonly IAudioManager _audioManager;
        private readonly IAudioRecorder _audioRecorder;
        private bool _isRecording;
        private string _recordingDuration;
        private int _lastRecordingId;
        private Dream _selectedDream;

        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                _isRecording = value;
                OnPropertyChanged(nameof(IsRecording));
            }
        }

        public Dream SelectedDream
        {
            get => _selectedDream;
            set
            {
                _selectedDream = value;
                OnPropertyChanged(nameof(SelectedDream));
            }
        }

        private MediaElement _audioPlayer;
        public MediaElement AudioPlayer
        {
            get => _audioPlayer;
            set
            {
                _audioPlayer = value;
                OnPropertyChanged(nameof(AudioPlayer));
            }
        }

        public ICommand ToggleRecordingCommand { get; set; }
        public ICommand PlayRecordingCommand { get; }

        public ObservableCollection<Dream> Dreams { get; set; }
        public ObservableCollection<ByteArrayMediaElement> AudioElements { get; } = new ObservableCollection<ByteArrayMediaElement>();


        public DreamsViewModel(DreamService dreamService, IAudioManager audioManager)
        {
            _dreamService = dreamService;
            DeleteDreamCommand = new Command<Dream>(async (dream) => await OnDeleteDream(dream));
            _audioManager = audioManager;
            _audioRecorder = audioManager.CreateRecorder();
            _elapsedTime = TimeSpan.Zero;

            ToggleRecordingCommand = new Command(async () => await ToggleRecording());
            PlayRecordingCommand = new Command<Dream>(PlayRecording);
            Dreams = new ObservableCollection<Dream>();

            AudioElements = new ObservableCollection<ByteArrayMediaElement>();
            PerformInitialSetup();
            // Load dreams and populate audio elements asynchronously

        }

        private async void PerformInitialSetup()
        {
            await LoadDreamsAndAudioElements();
        }

        private async Task LoadDreamsAndAudioElements()
        {
            // Load dreams asynchronously
            var loadedDreams = await _dreamService.GetDreams();

            // Populate the Dreams collection
            foreach (var dream in loadedDreams)
            {
                Dreams.Add(dream);

                // Create MediaElement from dream recording byte array
                var audioElement = CreateMediaElementFromByteArray(dream.DreamRecording);
                AudioElements.Add(audioElement);
            }
        }

        private ByteArrayMediaElement CreateMediaElementFromByteArray(byte[] audioData)
        {
            // Create MediaElement
            //MediaElement mediaElement = new MediaElement();
            ByteArrayMediaElement byteArrayMediaElement = new ByteArrayMediaElement();

            try
            {
                // Convert byte array to Stream
                Stream stream = new MemoryStream(audioData);
                
                ByteArrayMediaSource src = new ByteArrayMediaSource(audioData);
                //mediaElement.Source = src;// (stream, "audio/mpeg");
                byteArrayMediaElement.Source = src;
                byteArrayMediaElement.AudioData = audioData;
            }
            catch (Exception)
            {

            }            

            return byteArrayMediaElement;
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
            if (IsRecording)
            {
                if (SelectedDream != null)
                {
                    await StopRecording(SelectedDream.Id);
                    IsRecording = false;
                }
            }
            else
            {
                await StartRecording();
                IsRecording = true;
            }
        }

        private async Task StartRecording()
        {
            await this._audioRecorder.StartAsync();

        }

        public async Task StopRecording(int dreamId)
        {
            IAudioSource audioSource = await this._audioRecorder.StopAsync();
            Stream audioStream = audioSource.GetAudioStream();

            using (MemoryStream ms = new MemoryStream())
            {
                await audioStream.CopyToAsync(ms);
                byte[] audioData = ms.ToArray();

                // Save the audio data to the database
                SaveRecordingToDatabase(dreamId, audioData);
            }

            audioStream.Close();
        }

        private async Task SaveRecordingToDatabase(int dreamId, byte[] audioData)
        {
            using var connection = SQLiteDbService.CreateConnection();
            connection.Open();

            // Insert or update the dream recording
            var updateQuery = @"
            UPDATE Dreams
            SET DreamRecording = @DreamRecording
            WHERE Id = @Id";

            var parameters = new { DreamRecording = audioData, Id = dreamId };

            await connection.ExecuteAsync(updateQuery, parameters);
        }

        public void PlayRecording(Dream dream)
        {
            if (dream != null && dream.DreamRecording != null)
            {
                // Convert byte array to stream
                Stream audioStream = new MemoryStream(dream.DreamRecording);

                // Set the source of the MediaElement to the audio stream
                //AudioPlayer.SetSource(audioStream, "audio/mpeg");

                // Play the audio
                AudioPlayer.Play();
            }
        }

        public async void PlayAudio(byte[] audioData)
        {
            // Convert byte array to stream
            Stream audioStream = new MemoryStream(audioData);

            // Set the source of the MediaElement to the audio stream
            ByteArrayMediaSource source = new ByteArrayMediaSource(audioData);
            AudioPlayer.Source = source; //new ByteArrayMediaSource(audioData);

            // Play the audio
            AudioPlayer.Play();
        }

    }
}
