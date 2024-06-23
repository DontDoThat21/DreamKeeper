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
                _viewModel.Dreams.Add(dream); // Adding the dream to the ObservableCollection
            }

            // Close the sub-content view
            await Navigation.PopModalAsync();
        }

        private async void DreamRemoveButton_ClickedAsync(object sender, EventArgs e)
        {

        }

        private async void OnToggleRecordingClicked(object sender, EventArgs e)
        {
            await _viewModel.ToggleRecording();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var bytestosave = _viewModel.AudioElements[2];
            var bytestosaveReal = bytestosave.AudioData;
            using (FileStream fs = new FileStream("C:\\Users\\Tylor\\Desktop\\file.mp3",
                FileMode.Create, FileAccess.Write))
            {
                // Write the bytes to the file
                fs.Write(bytestosaveReal, 0, bytestosaveReal.Length);
            }
            var yuwaduh = _viewModel.AudioElements;
        }
    }

}
