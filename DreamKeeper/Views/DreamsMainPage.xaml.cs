using DreamKeeper.Data;
using DreamKeeper.Models;
using DreamKeeper.Services;
using DreamKeeper.ViewModels;
using DreamKeeper.Views;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;

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
                await _viewModel.ToggleRecording();
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
                        _viewModel.PlayRecording(dream);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing recording: {ex.Message}");
            }
        }
    }

}
