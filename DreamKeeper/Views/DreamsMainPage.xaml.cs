using DreamKeeper.Data;
using DreamKeeper.Models;
using DreamKeeper.Services;
using DreamKeeper.ViewModels;
using DreamKeeper.Views;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;

namespace DreamKeeper
{
    public partial class MainPage : ContentPage
    {

        private readonly DreamsViewModel _viewModel;
        public DreamService _dreamService { get; }
        public IAudioManager _audioManager { get; }

        public MainPage()
        {
            InitializeComponent();

            _dreamService = new DreamService(); // Instantiate DreamService here
            _audioManager = new AudioManager();
            _viewModel = new DreamsViewModel(_dreamService, _audioManager);
            BindingContext = _viewModel;
        }

        private async void DreamAddButton_ClickedAsync(object sender, EventArgs e)
        {
            var dreamEntryPage = new DreamEntryPage();
            dreamEntryPage.DreamSaved += DreamEntryPage_DreamSaved;
            await Navigation.PushModalAsync(new NavigationPage(dreamEntryPage));
        }

        private async void DreamEntryPage_DreamSaved(object sender, Dream dream)
        {
            var newDream = _dreamService.AddDream(dream);
            if (newDream.Id == -1)
            {
                await DisplayAlert("An error has occurred.", newDream.DreamDescription, "Cancel");
            }
            else
            {
                // No error(s).
                // Mark the dream as saved since it was successfully added to the database
                newDream.MarkAsSaved();
                _viewModel.Dreams.Add(newDream); // Adding the dream to the ObservableCollection
            }

            // Close the sub-content view
            await Navigation.PopModalAsync();
        }

        private async void DreamRemoveButton_ClickedAsync(object sender, EventArgs e)
        {

        }

        private async void OnToggleRecordingClicked(object sender, EventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button != null && button.BindingContext is Dream dream)
                {
                    await _viewModel.ToggleRecording(dream);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling recording: {ex.Message}");
                await DisplayAlert("Recording Error", "There was an error with the audio recording.", "OK");
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button != null)
                {
                    // Get the binding context of the button which should be a Dream object
                    if (button.BindingContext is Dream dream && dream.DreamRecording != null)
                    {
                        // Find the parent Frame and then the ByteArrayMediaElement within it
                        var parentFrame = FindParentElement<Frame>(button);
                        if (parentFrame != null)
                        {
                            var mediaElement = FindChildElement<ByteArrayMediaElement>(parentFrame);
                            if (mediaElement != null)
                            {
                                // Stop any currently playing audio first
                                StopAllAudioPlayback();
                                
                                // Start playback immediately
                                mediaElement.Play();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing recording: {ex.Message}");
            }
        }

        private void StopAllAudioPlayback()
        {
            // Find all ByteArrayMediaElement controls and stop them
            var mediaElements = FindAllChildElements<ByteArrayMediaElement>(this);
            foreach (var mediaElement in mediaElements)
            {
                try
                {
                    mediaElement.Stop();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error stopping audio: {ex.Message}");
                }
            }
        }

        private T FindParentElement<T>(Element element) where T : Element
        {
            var parent = element.Parent;
            while (parent != null)
            {
                if (parent is T targetParent)
                    return targetParent;
                parent = parent.Parent;
            }
            return null;
        }

        private T FindChildElement<T>(Element parent) where T : Element
        {
            if (parent is T target)
                return target;

            if (parent is Layout layout)
            {
                foreach (var child in layout.Children)
                {
                    if (child is Element element)
                    {
                        var result = FindChildElement<T>(element);
                        if (result != null)
                            return result;
                    }
                }
            }
            else if (parent is ContentView contentView && contentView.Content != null)
            {
                return FindChildElement<T>(contentView.Content);
            }
            else if (parent is ScrollView scrollView && scrollView.Content != null)
            {
                return FindChildElement<T>(scrollView.Content);
            }

            return null;
        }

        private List<T> FindAllChildElements<T>(Element parent) where T : Element
        {
            var results = new List<T>();
            
            if (parent is T target)
                results.Add(target);

            if (parent is Layout layout)
            {
                foreach (var child in layout.Children)
                {
                    if (child is Element element)
                    {
                        results.AddRange(FindAllChildElements<T>(element));
                    }
                }
            }
            else if (parent is ContentView contentView && contentView.Content != null)
            {
                results.AddRange(FindAllChildElements<T>(contentView.Content));
            }
            else if (parent is ScrollView scrollView && scrollView.Content != null)
            {
                results.AddRange(FindAllChildElements<T>(scrollView.Content));
            }

            return results;
        }

        private async void OnDateLabelTapped(object sender, EventArgs e)
        {
            if (sender is Label label && label.BindingContext is Dream dream)
            {
                dream.IsEditingDate = true;
            }
        }

        private async void OnDatePickerDateSelected(object sender, DateChangedEventArgs e)
        {
            if (sender is DatePicker datePicker && datePicker.BindingContext is Dream dream)
            {
                dream.DreamDate = e.NewDate;
                dream.IsEditingDate = false;
            }
        }
    }

}
