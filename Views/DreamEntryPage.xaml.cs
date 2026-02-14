using DreamKeeper.Data.Data;
using DreamKeeper.Data.Models;
using Plugin.Maui.Audio;

namespace DreamKeeper.Views
{
    public partial class DreamEntryPage : ContentPage
    {
        private readonly IAudioManager _audioManager;
        private readonly ITranscriptionService? _transcriptionService;
        private IAudioRecorder? _audioRecorder;
        private byte[]? _recordedAudioBytes;
        private bool _isRecording;

        /// <summary>
        /// Event raised when a dream is saved. The parent page handles persistence.
        /// </summary>
        public event EventHandler<Dream>? DreamSaved;

        public DreamEntryPage()
            : this(new AudioManager(),
                   Application.Current?.MainPage?.Handler?.MauiContext?.Services.GetService<ITranscriptionService>())
        {
        }

        public DreamEntryPage(IAudioManager audioManager, ITranscriptionService? transcriptionService = null)
        {
            ArgumentNullException.ThrowIfNull(audioManager);
            _audioManager = audioManager;
            _transcriptionService = transcriptionService;
            InitializeComponent();
        }

        /// <summary>
        /// Toggles audio recording on/off, storing the captured bytes in memory.
        /// </summary>
        private async void OnToggleRecordingClicked(object? sender, EventArgs e)
        {
            try
            {
                if (_isRecording)
                {
                    await StopRecordingAsync();
                }
                else
                {
                    await StartRecordingAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Recording error: {ex.Message}");
            }
        }

        private async Task StartRecordingAsync()
        {
            _audioRecorder = _audioManager.CreateRecorder();
            await _audioRecorder.StartAsync();
            _isRecording = true;
            RecordingButton.Text = "⏹ Stop Recording";
            RecordingStatusLabel.IsVisible = true;
        }

        private async Task StopRecordingAsync()
        {
            if (_audioRecorder is null || !_audioRecorder.IsRecording)
                return;

            var audioSource = await _audioRecorder.StopAsync();

            using var memoryStream = new MemoryStream();
            var stream = audioSource.GetAudioStream();
            await stream.CopyToAsync(memoryStream);
            _recordedAudioBytes = memoryStream.ToArray();

            _isRecording = false;
            RecordingButton.Text = "✅ Recording Added";
            RecordingStatusLabel.IsVisible = false;

            await TranscribeRecordingAsync();
        }

        /// <summary>
        /// Transcribes the recorded audio using Whisper and appends the result to the description editor.
        /// </summary>
        private async Task TranscribeRecordingAsync()
        {
            if (_transcriptionService is null || _recordedAudioBytes is null || _recordedAudioBytes.Length == 0)
                return;

            try
            {
                RecordingStatusLabel.Text = "Transcribing audio...";
                RecordingStatusLabel.IsVisible = true;

                var transcription = await Task.Run(() =>
                    _transcriptionService.TranscribeAsync(_recordedAudioBytes));

                if (!string.IsNullOrWhiteSpace(transcription))
                {
                    // Append to existing text or set if empty
                    if (string.IsNullOrWhiteSpace(DescriptionEditor.Text))
                    {
                        DescriptionEditor.Text = transcription;
                    }
                    else
                    {
                        DescriptionEditor.Text = $"{DescriptionEditor.Text}\n{transcription}";
                    }
                }

                RecordingStatusLabel.IsVisible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Transcription error: {ex.Message}");
                RecordingStatusLabel.Text = "Transcription failed";
            }
        }

        /// <summary>
        /// Creates a new Dream object (including any recorded audio), raises the DreamSaved event, and dismisses the modal.
        /// </summary>
        private async void OnSaveDreamClicked(object? sender, EventArgs e)
        {
            // Stop an active recording before saving
            if (_isRecording)
            {
                await StopRecordingAsync();
            }

            var dream = new Dream
            {
                DreamName = string.IsNullOrWhiteSpace(TitleEntry.Text)
                    ? "Untitled Dream"
                    : TitleEntry.Text,
                DreamDescription = DescriptionEditor.Text ?? string.Empty,
                DreamDate = DreamDatePicker.Date ?? DateTime.Now,
                DreamRecording = _recordedAudioBytes
            };

            DreamSaved?.Invoke(this, dream);
            await Navigation.PopModalAsync();
        }

        /// <summary>
        /// Cancel: stops any active recording and dismisses the modal without saving.
        /// </summary>
        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            if (_isRecording && _audioRecorder is { IsRecording: true })
            {
                await _audioRecorder.StopAsync();
            }

            await Navigation.PopModalAsync();
        }
    }
}
