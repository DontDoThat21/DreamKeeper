using DreamKeeper.Data;
using DreamKeeper.Models;
using DreamKeeper.Services;
using DreamKeeper.ViewModels;
using DreamKeeper.Views;
using System.Collections.ObjectModel;

namespace DreamKeeper
{
    public partial class MainPage : ContentPage
    {

        private readonly DreamsViewModel _viewModel;
        public DreamService _dreamService { get; }

        public MainPage()
        {
            InitializeComponent();

            _dreamService = new DreamService(); // Instantiate DreamService here
            _viewModel = new DreamsViewModel(_dreamService);
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
    }

}
