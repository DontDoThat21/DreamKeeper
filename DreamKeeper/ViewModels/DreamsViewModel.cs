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
        public ICommand SaveDreamCommand { get; }

        public ObservableCollection<Dream> Dreams { get; set; }
        public ObservableCollection<ByteArrayMediaElement> AudioElements { get; } = new ObservableCollection<ByteArrayMediaElement>();


        public DreamsViewModel(DreamService dreamService, IAudioManager audioManager)
        {
            _dreamService = dreamService;
            DeleteDreamCommand = new Command<Dream>(async (dream) => await OnDeleteDream(dream));
            SaveDreamCommand = new Command<Dream>(async (dream) => await OnSaveDream(dream));
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
                // Mark loaded dreams as saved (no unsaved changes)
                dream.MarkAsSaved();
                Dreams.Add(dream);

                // Create MediaElement from dream recording byte array
                var audioElement = CreateMediaElementFromByteArray(dream.DreamRecording);
                AudioElements.Add(audioElement);
            }
        }

        private ByteArrayMediaElement CreateMediaElementFromByteArray(byte[] audioData)
        {
            // Create a ByteArrayMediaElement
            ByteArrayMediaElement byteArrayMediaElement = new ByteArrayMediaElement();

            try
            {
                if (audioData != null && audioData.Length > 0)
                {
                    byteArrayMediaElement.AudioData = audioData;
                    byteArrayMediaElement.ShouldAutoPlay = false;
                    byteArrayMediaElement.ShouldShowPlaybackControls = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating media element: {ex.Message}");
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
                    await _dreamService.DeleteDream(dream.Id);
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
            try
            {
                IAudioSource audioSource = await this._audioRecorder.StopAsync();
                Stream audioStream = audioSource.GetAudioStream();

                using (MemoryStream ms = new MemoryStream())
                {
                    await audioStream.CopyToAsync(ms);
                    byte[] audioData = ms.ToArray();

                    if (audioData.Length > 0)
                    {
                        // Save the audio data to the database
                        await SaveRecordingToDatabase(dreamId, audioData);

                        // Update the dream in the collection
                        var dream = Dreams.FirstOrDefault(d => d.Id == dreamId);
                        if (dream != null)
                        {
                            // Temporarily disable change tracking to avoid marking as unsaved
                            var originalChangeState = dream.HasUnsavedChanges;
                            dream.DreamRecording = audioData;
                            dream.HasUnsavedChanges = originalChangeState; // Restore original state since recording is automatically saved

                            // Update the audio element 
                            var existingIndex = Dreams.IndexOf(dream);
                            if (existingIndex >= 0 && existingIndex < AudioElements.Count)
                            {
                                AudioElements[existingIndex].AudioData = audioData;
                            }
                        }
                    }
                }

                audioStream.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping recording: {ex.Message}");
            }
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
            try
            {
                if (dream != null && dream.DreamRecording != null && dream.DreamRecording.Length > 0)
                {
                    // Create a new ByteArrayMediaElement if AudioPlayer is null
                    if (AudioPlayer == null)
                    {
                        AudioPlayer = new ByteArrayMediaElement();
                    }

                    // Set the AudioData property which will trigger UpdateMediaSource
                    if (AudioPlayer is ByteArrayMediaElement byteArrayMediaElement)
                    {
                        byteArrayMediaElement.AudioData = dream.DreamRecording;
                    }
                    else
                    {
                        // Fallback using direct source
                        string extension = DeviceInfo.Platform == DevicePlatform.iOS ? "m4a" : "mp4";
                        string tempFile = Path.Combine(FileSystem.CacheDirectory, $"temp_playback_{Guid.NewGuid()}.{extension}");
                        File.WriteAllBytes(tempFile, dream.DreamRecording);
                        AudioPlayer.Source = MediaSource.FromFile(tempFile);
                    }

                    // Play the audio
                    AudioPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing recording: {ex.Message}");
            }
        }

        public async void PlayAudio(byte[] audioData)
        {
            try
            {
                if (audioData != null && audioData.Length > 0)
                {
                    // Create a new ByteArrayMediaElement if AudioPlayer is null
                    if (AudioPlayer == null)
                    {
                        AudioPlayer = new ByteArrayMediaElement();
                    }

                    // Set the AudioData property directly if it's a ByteArrayMediaElement
                    if (AudioPlayer is ByteArrayMediaElement byteArrayMediaElement)
                    {
                        byteArrayMediaElement.AudioData = audioData;
                    }
                    else
                    {
                        // Fallback to ByteArrayMediaSource
                        ByteArrayMediaSource source = new ByteArrayMediaSource(audioData);
                        AudioPlayer.Source = source;
                    }

                    // Play the audio
                    AudioPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing audio: {ex.Message}");
            }
        }

        private async Task OnSaveDream(Dream dream)
        {
            if (dream != null && dream.HasUnsavedChanges)
            {
                try
                {
                    await _dreamService.UpsertDream(dream);
                    dream.MarkAsSaved();
                    
                    // Show a brief confirmation
                    await App.Current.MainPage.DisplayAlert("Success", "Dream saved successfully!", "OK");
                }
                catch (Exception ex)
                {
                    await App.Current.MainPage.DisplayAlert("Error", $"Failed to save dream: {ex.Message}", "OK");
                }
            }
        }

    }
}
