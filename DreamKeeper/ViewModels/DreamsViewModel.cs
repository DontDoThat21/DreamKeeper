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
        private Dream _currentlyRecordingDream; // Track which dream is currently recording
        
        // Search and filter properties
        private string _searchText = string.Empty;
        private bool _showOnlyWithRecordings = false;
        private bool _showOnlyWithoutRecordings = false;
        private ObservableCollection<Dream> _allDreams; // Store all dreams
        private ObservableCollection<Dream> _filteredDreams; // Display filtered dreams

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

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    ApplyFilters();
                }
            }
        }

        public bool ShowOnlyWithRecordings
        {
            get => _showOnlyWithRecordings;
            set
            {
                if (_showOnlyWithRecordings != value)
                {
                    _showOnlyWithRecordings = value;
                    OnPropertyChanged(nameof(ShowOnlyWithRecordings));
                    if (value)
                    {
                        ShowOnlyWithoutRecordings = false;
                    }
                    ApplyFilters();
                }
            }
        }

        public bool ShowOnlyWithoutRecordings
        {
            get => _showOnlyWithoutRecordings;
            set
            {
                if (_showOnlyWithoutRecordings != value)
                {
                    _showOnlyWithoutRecordings = value;
                    OnPropertyChanged(nameof(ShowOnlyWithoutRecordings));
                    if (value)
                    {
                        ShowOnlyWithRecordings = false;
                    }
                    ApplyFilters();
                }
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
        public ICommand ClearFiltersCommand { get; }
        public ICommand DeleteRecordingCommand { get; }

        public ObservableCollection<Dream> Dreams 
        { 
            get => _filteredDreams ?? new ObservableCollection<Dream>();
            private set
            {
                _filteredDreams = value;
                OnPropertyChanged(nameof(Dreams));
            }
        }
        
        public ObservableCollection<ByteArrayMediaElement> AudioElements { get; } = new ObservableCollection<ByteArrayMediaElement>();


        public DreamsViewModel(DreamService dreamService, IAudioManager audioManager)
        {
            _dreamService = dreamService;
            DeleteDreamCommand = new Command<Dream>(async (dream) => await OnDeleteDream(dream));
            SaveDreamCommand = new Command<Dream>(async (dream) => await OnSaveDream(dream));
            ClearFiltersCommand = new Command(ClearFilters);
            DeleteRecordingCommand = new Command<Dream>(async (dream) => await OnDeleteRecording(dream));
            _audioManager = audioManager;
            _audioRecorder = audioManager.CreateRecorder();
            _elapsedTime = TimeSpan.Zero;

            ToggleRecordingCommand = new Command<Dream>(async (dream) => await ToggleRecording(dream));
            PlayRecordingCommand = new Command<Dream>(PlayRecording);
            
            _allDreams = new ObservableCollection<Dream>();
            _filteredDreams = new ObservableCollection<Dream>();

            AudioElements = new ObservableCollection<ByteArrayMediaElement>();
            PerformInitialSetup();
        }

        private async void PerformInitialSetup()
        {
            await LoadDreamsAndAudioElements();
        }

        private async Task LoadDreamsAndAudioElements()
        {
            // Load dreams asynchronously
            var loadedDreams = await _dreamService.GetDreams();

            // Sort chronologically (oldest first)
            var sortedDreams = loadedDreams.OrderBy(d => d.DreamDate).ToList();

            // Clear existing collections
            _allDreams.Clear();
            AudioElements.Clear();

            // Populate the collections
            foreach (var dream in sortedDreams)
            {
                // Mark loaded dreams as saved (no unsaved changes)
                dream.MarkAsSaved();
                _allDreams.Add(dream);

                // Create MediaElement from dream recording byte array
                var audioElement = CreateMediaElementFromByteArray(dream.DreamRecording);
                AudioElements.Add(audioElement);
            }

            // Apply filters to populate the filtered dreams
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _allDreams.AsEnumerable();

            // Apply search text filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(d => 
                    (d.DreamName?.ToLower().Contains(searchLower) ?? false) ||
                    (d.DreamDescription?.ToLower().Contains(searchLower) ?? false));
            }

            // Apply recording filters
            if (ShowOnlyWithRecordings)
            {
                filtered = filtered.Where(d => d.DreamRecording != null && d.DreamRecording.Length > 0);
            }
            else if (ShowOnlyWithoutRecordings)
            {
                filtered = filtered.Where(d => d.DreamRecording == null || d.DreamRecording.Length == 0);
            }

            // Sort chronologically (oldest first) - maintain order after filtering
            var sortedFiltered = filtered.OrderBy(d => d.DreamDate).ToList();

            // Update the filtered collection
            Dreams.Clear();
            foreach (var dream in sortedFiltered)
            {
                Dreams.Add(dream);
            }
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            ShowOnlyWithRecordings = false;
            ShowOnlyWithoutRecordings = false;
        }

        public void AddNewDream(Dream dream)
        {
            // Add to all dreams collection
            _allDreams.Add(dream);
            
            // Create audio element
            var audioElement = CreateMediaElementFromByteArray(dream.DreamRecording);
            AudioElements.Add(audioElement);
            
            // Reapply filters to maintain proper order and filtering
            ApplyFilters();
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
                    
                    // Remove from all collections
                    _allDreams.Remove(dream);
                    var dreamIndex = Dreams.IndexOf(dream);
                    if (dreamIndex >= 0)
                    {
                        Dreams.Remove(dream);
                        if (dreamIndex < AudioElements.Count)
                        {
                            AudioElements.RemoveAt(dreamIndex);
                        }
                    }
                }
            }
        }

        public ICommand DeleteDreamCommand { get; }

        // ...existing code for recording functionality...
        private TimeSpan _elapsedTime;
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);


        public async Task ToggleRecording(Dream dream)
        {
            if (dream == null) return;

            if (_currentlyRecordingDream != null && _currentlyRecordingDream != dream)
            {
                // Stop any other recording first
                await StopRecording(_currentlyRecordingDream);
            }

            if (dream.IsRecording)
            {
                await StopRecording(dream);
            }
            else
            {
                await StartRecording(dream);
            }
        }

        public async Task ToggleRecording()
        {
            if (SelectedDream != null)
            {
                await ToggleRecording(SelectedDream);
            }
        }

        private async Task StartRecording(Dream dream)
        {
            try
            {
                // Stop any other recording first
                if (_currentlyRecordingDream != null && _currentlyRecordingDream != dream)
                {
                    await StopRecording(_currentlyRecordingDream);
                }

                await this._audioRecorder.StartAsync();
                dream.IsRecording = true;
                _currentlyRecordingDream = dream;
                IsRecording = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting recording: {ex.Message}");
                dream.IsRecording = false;
                _currentlyRecordingDream = null;
                IsRecording = false;
            }
        }

        private async Task StopRecording(Dream dream)
        {
            try
            {
                if (dream == null || !dream.IsRecording) return;

                IAudioSource audioSource = await this._audioRecorder.StopAsync();
                Stream audioStream = audioSource.GetAudioStream();

                using (MemoryStream ms = new MemoryStream())
                {
                    await audioStream.CopyToAsync(ms);
                    byte[] audioData = ms.ToArray();

                    if (audioData.Length > 0)
                    {
                        // Save the audio data to the database
                        await SaveRecordingToDatabase(dream.Id, audioData);

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
                        
                        // Reapply filters in case recording status affects filtering
                        ApplyFilters();
                    }
                }

                audioStream.Close();
                
                // Update recording state
                dream.IsRecording = false;
                if (_currentlyRecordingDream == dream)
                {
                    _currentlyRecordingDream = null;
                    IsRecording = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping recording: {ex.Message}");
                dream.IsRecording = false;
                if (_currentlyRecordingDream == dream)
                {
                    _currentlyRecordingDream = null;
                    IsRecording = false;
                }
            }
        }

        public async Task StopRecording(int dreamId)
        {
            var dream = Dreams.FirstOrDefault(d => d.Id == dreamId);
            if (dream != null)
            {
                await StopRecording(dream);
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

        private async Task OnDeleteRecording(Dream dream)
        {
            if (dream != null && dream.DreamRecording != null && dream.DreamRecording.Length > 0)
            {
                bool isConfirmed = await App.Current.MainPage.DisplayAlert(
                    "Delete Recording",
                    "Are you sure you want to delete this recording?",
                    "Yes",
                    "No");

                if (isConfirmed)
                {
                    try
                    {
                        // Remove recording from database
                        await DeleteRecordingFromDatabase(dream.Id);

                        // Update the dream object
                        var originalChangeState = dream.HasUnsavedChanges;
                        dream.DreamRecording = null;
                        dream.HasUnsavedChanges = originalChangeState; // Recording deletion is automatically saved

                        // Update the audio element
                        var existingIndex = Dreams.IndexOf(dream);
                        if (existingIndex >= 0 && existingIndex < AudioElements.Count)
                        {
                            AudioElements[existingIndex].AudioData = null;
                        }

                        // Reapply filters in case recording status affects filtering
                        ApplyFilters();

                        await App.Current.MainPage.DisplayAlert("Success", "Recording deleted successfully!", "OK");
                    }
                    catch (Exception ex)
                    {
                        await App.Current.MainPage.DisplayAlert("Error", $"Failed to delete recording: {ex.Message}", "OK");
                    }
                }
            }
        }

        private async Task DeleteRecordingFromDatabase(int dreamId)
        {
            using var connection = SQLiteDbService.CreateConnection();
            connection.Open();

            var updateQuery = @"
            UPDATE Dreams
            SET DreamRecording = NULL
            WHERE Id = @Id";

            var parameters = new { Id = dreamId };

            await connection.ExecuteAsync(updateQuery, parameters);
        }
    }
}
