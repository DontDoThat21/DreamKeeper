using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using DreamKeeper.Data.Data;
using DreamKeeper.Data.Models;
using DreamKeeper.Data.Services;
using Plugin.Maui.Audio;

namespace DreamKeeper.ViewModels
{
    public class DreamsViewModel : INotifyPropertyChanged
    {
        private readonly DreamService _dreamService;
        private readonly IAudioManager _audioManager;
        private readonly ITranscriptionService? _transcriptionService;
        private IAudioRecorder? _audioRecorder;
        private Dream? _currentlyRecordingDream;

        private List<Dream> _allDreams = new();
        private List<Dream> _filteredDreams = new();

        private ObservableCollection<Dream> _dreams = new();
        private Dream? _selectedDream;
        private bool _isRecording;
        private string _searchText = string.Empty;
        private bool _showOnlyWithRecordings;
        private bool _showOnlyWithoutRecordings;
        private bool _isRefreshing;

        public DreamsViewModel(DreamService dreamService, IAudioManager audioManager, ITranscriptionService? transcriptionService = null)
        {
            _dreamService = dreamService;
            _audioManager = audioManager;
            _transcriptionService = transcriptionService;

            ToggleRecordingCommand = new Command<Dream>(async (dream) => await ToggleRecording(dream));
            PlayRecordingCommand = new Command<Dream>((dream) => { /* Handled in code-behind */ });
            SaveDreamCommand = new Command<Dream>(async (dream) => await SaveDream(dream));
            DeleteDreamCommand = new Command<Dream>(async (dream) => await DeleteDream(dream));
            DeleteRecordingCommand = new Command<Dream>(async (dream) => await DeleteRecording(dream));
            ClearFiltersCommand = new Command(ClearFilters);
            RefreshCommand = new Command(() => { LoadDreams(); IsRefreshing = false; });

            LoadDreams();
        }

        public ObservableCollection<Dream> Dreams
        {
            get => _dreams;
            set
            {
                _dreams = value;
                OnPropertyChanged(nameof(Dreams));
            }
        }

        public Dream? SelectedDream
        {
            get => _selectedDream;
            set
            {
                _selectedDream = value;
                OnPropertyChanged(nameof(SelectedDream));
            }
        }

        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                _isRecording = value;
                OnPropertyChanged(nameof(IsRecording));
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
                    if (value) ShowOnlyWithoutRecordings = false;
                    OnPropertyChanged(nameof(ShowOnlyWithRecordings));
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
                    if (value) ShowOnlyWithRecordings = false;
                    OnPropertyChanged(nameof(ShowOnlyWithoutRecordings));
                    ApplyFilters();
                }
            }
        }

        // Commands
        public ICommand ToggleRecordingCommand { get; }
        public ICommand PlayRecordingCommand { get; }
        public ICommand SaveDreamCommand { get; }
        public ICommand DeleteDreamCommand { get; }
        public ICommand DeleteRecordingCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand RefreshCommand { get; }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                if (_isRefreshing != value)
                {
                    _isRefreshing = value;
                    OnPropertyChanged(nameof(IsRefreshing));
                }
            }
        }

        public void LoadDreams()
        {
            var dreams = _dreamService.GetDreams();
            _allDreams = dreams.ToList();
            ApplyFilters();
        }

        public void AddDreamToCollection(Dream dream)
        {
            var savedDream = _dreamService.AddDream(dream);
            _allDreams.Insert(0, savedDream);
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            _filteredDreams = _allDreams.ToList();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLowerInvariant();
                _filteredDreams = _filteredDreams.Where(d =>
                    (d.DreamName?.ToLowerInvariant().Contains(search) ?? false) ||
                    (d.DreamDescription?.ToLowerInvariant().Contains(search) ?? false)
                ).ToList();
            }

            // Apply recording filters (mutually exclusive)
            if (ShowOnlyWithRecordings)
            {
                _filteredDreams = _filteredDreams
                    .Where(d => d.DreamRecording != null && d.DreamRecording.Length > 0)
                    .ToList();
            }
            else if (ShowOnlyWithoutRecordings)
            {
                _filteredDreams = _filteredDreams
                    .Where(d => d.DreamRecording == null || d.DreamRecording.Length == 0)
                    .ToList();
            }

            // Sort by date descending (latest first)
            _filteredDreams = _filteredDreams
                .OrderByDescending(d => d.DreamDate)
                .ToList();

            Dreams = new ObservableCollection<Dream>(_filteredDreams);
        }

        private async Task ToggleRecording(Dream dream)
        {
            try
            {
                if (_currentlyRecordingDream != null && _currentlyRecordingDream != dream)
                {
                    // Stop the current recording first
                    await StopRecording(_currentlyRecordingDream);
                }

                if (dream.IsRecording)
                {
                    // Stop recording
                    await StopRecording(dream);
                }
                else
                {
                    // Start recording
                    _audioRecorder = _audioManager.CreateRecorder();
                    await _audioRecorder.StartAsync();
                    dream.IsRecording = true;
                    _currentlyRecordingDream = dream;
                    IsRecording = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Recording error: {ex.Message}");
            }
        }

        private async Task StopRecording(Dream dream)
        {
            if (_audioRecorder != null && _audioRecorder.IsRecording)
            {
                var audioSource = await _audioRecorder.StopAsync();

                // Convert audio to byte[]
                using var memoryStream = new MemoryStream();
                var stream = audioSource.GetAudioStream();
                await stream.CopyToAsync(memoryStream);
                var audioBytes = memoryStream.ToArray();

                // Persist to database via raw SQL
                using var connection = SQLiteDbService.CreateConnection();
                await connection.OpenAsync();
                await Dapper.SqlMapper.ExecuteAsync(connection,
                    "UPDATE Dreams SET DreamRecording = @recording WHERE Id = @id",
                    new { recording = audioBytes, id = dream.Id });

                dream.DreamRecording = audioBytes;
                dream.IsRecording = false;
                dream.HasUnsavedChanges = false;
                _currentlyRecordingDream = null;
                IsRecording = false;

                await TranscribeAndAppendAsync(dream, audioBytes);
            }
        }

        /// <summary>
        /// Transcribes recorded audio and appends the text to the dream description.
        /// </summary>
        private async Task TranscribeAndAppendAsync(Dream dream, byte[] audioBytes)
        {
            if (_transcriptionService is null || audioBytes.Length == 0)
                return;

            try
            {
                var transcription = await Task.Run(() =>
                    _transcriptionService.TranscribeAsync(audioBytes));

                if (!string.IsNullOrWhiteSpace(transcription))
                {
                    dream.DreamDescription = string.IsNullOrWhiteSpace(dream.DreamDescription)
                        ? transcription
                        : $"{dream.DreamDescription}\n{transcription}";

                    _dreamService.UpsertDream(dream);
                    dream.MarkAsSaved();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Transcription error: {ex.Message}");
            }
        }

        private async Task SaveDream(Dream dream)
        {
            try
            {
                _dreamService.UpsertDream(dream);
                dream.MarkAsSaved();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save error: {ex.Message}");
            }
            await Task.CompletedTask;
        }

        private async Task DeleteDream(Dream dream)
        {
            bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
                "Delete Dream",
                "Are you sure you want to delete this dream?",
                "Delete", "Cancel");

            if (confirm)
            {
                _dreamService.DeleteDream(dream.Id);
                _allDreams.Remove(dream);
                ApplyFilters();
            }
        }

        private async Task DeleteRecording(Dream dream)
        {
            string action = await Application.Current!.Windows[0].Page!.DisplayActionSheetAsync(
                "Delete Recording?",
                "Cancel",
                "Delete Recording");

            if (action == "Delete Recording")
            {
                // Set DreamRecording = NULL via raw SQL
                using var connection = SQLiteDbService.CreateConnection();
                await connection.OpenAsync();
                await Dapper.SqlMapper.ExecuteAsync(connection,
                    "UPDATE Dreams SET DreamRecording = NULL WHERE Id = @id",
                    new { id = dream.Id });

                dream.DreamRecording = null;
                dream.HasUnsavedChanges = false;
            }
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            ShowOnlyWithRecordings = false;
            ShowOnlyWithoutRecordings = false;
            ApplyFilters();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
