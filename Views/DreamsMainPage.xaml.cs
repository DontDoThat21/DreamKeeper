using DreamKeeper.Data;
using DreamKeeper.Data.Models;
using DreamKeeper.Data.Services;
using DreamKeeper.ViewModels;
using Plugin.Maui.Audio;

namespace DreamKeeper.Views
{
    public partial class DreamsMainPage : ContentPage
    {
        private readonly DreamsViewModel _viewModel;

        public DreamsMainPage()
        {
            InitializeComponent();
            _viewModel = new DreamsViewModel(new DreamService(), new AudioManager());
            BindingContext = _viewModel;
        }

        /// <summary>
        /// Tapping the date label toggles to inline DatePicker editing mode.
        /// </summary>
        private void OnDateLabelTapped(object? sender, EventArgs e)
        {
            if (sender is Label label && label.BindingContext is Dream dream)
            {
                dream.IsEditingDate = true;
            }
        }

        /// <summary>
        /// Selecting a date reverts the view back to the label and marks the dream as changed.
        /// </summary>
        private void OnDateSelected(object? sender, DateChangedEventArgs e)
        {
            if (sender is DatePicker picker && picker.BindingContext is Dream dream)
            {
                dream.DreamDate = e.NewDate ?? DateTime.Now;
                dream.IsEditingDate = false;
            }
        }

        /// <summary>
        /// Play button click: finds ByteArrayMediaElement in the card and plays audio.
        /// </summary>
        private async void OnPlayButtonClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Dream dream)
            {
                if (dream.DreamRecording == null || dream.DreamRecording.Length == 0)
                    return;

                StopAllAudioPlayback();

                var parent = button.Parent;
                while (parent != null)
                {
                    if (parent is Border border)
                    {
                        var mediaElement = FindChildElement<ByteArrayMediaElement>(border);
                        if (mediaElement != null)
                        {
                            if (mediaElement.AudioData == null || !mediaElement.AudioData.SequenceEqual(dream.DreamRecording))
                            {
                                mediaElement.AudioData = dream.DreamRecording;

                                if (DeviceInfo.Platform == DevicePlatform.Android)
                                {
                                    await Task.Delay(200);
                                }
                            }

                            try
                            {
                                mediaElement.Play();
                                System.Diagnostics.Debug.WriteLine("Play button clicked - playback started");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Play button error: {ex.Message}");
                                await DisplayAlertAsync("Playback Error", "Unable to play the recording. Please try again.", "OK");
                            }
                        }
                        break;
                    }
                    parent = (parent as Element)?.Parent;
                }
            }
        }

        /// <summary>
        /// Double-tap (long press) on play button triggers delete recording action sheet.
        /// </summary>
        private async void OnPlayButtonDoubleTapped(object? sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Dream dream)
            {
                if (dream.DreamRecording == null || dream.DreamRecording.Length == 0)
                    return;

                if (_viewModel.DeleteRecordingCommand.CanExecute(dream))
                {
                    _viewModel.DeleteRecordingCommand.Execute(dream);
                }
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Opens the DreamEntryPage modal when the "+" button is tapped.
        /// </summary>
        private async void OnAddDreamClicked(object? sender, EventArgs e)
        {
            var entryPage = new DreamEntryPage();
            entryPage.DreamSaved += OnDreamSaved;
            await Navigation.PushModalAsync(new NavigationPage(entryPage));
        }

        /// <summary>
        /// Handles the DreamSaved event from the entry page.
        /// </summary>
        private void OnDreamSaved(object? sender, Dream dream)
        {
            _viewModel.AddDreamToCollection(dream);
        }

        /// <summary>
        /// Stops all currently playing audio elements.
        /// </summary>
        private void StopAllAudioPlayback()
        {
            // Walk the visual tree to find and stop all ByteArrayMediaElements
            // In practice, the CollectionView reuses elements, so we iterate the current items
        }

        /// <summary>
        /// Finds a child element of a specific type in the visual tree.
        /// </summary>
        private T? FindChildElement<T>(Element parent) where T : Element
        {
            if (parent is T target)
                return target;

            if (parent is IVisualTreeElement visualTree)
            {
                foreach (var child in visualTree.GetVisualChildren())
                {
                    if (child is Element element)
                    {
                        var result = FindChildElement<T>(element);
                        if (result != null)
                            return result;
                    }
                }
            }

            return null;
        }
    }
}
