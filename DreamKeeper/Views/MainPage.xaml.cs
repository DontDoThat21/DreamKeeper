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
        //int count = 0;
        //ObservableCollection<DreamsViewModel> DreamsList { get; set; }

        private readonly DreamsViewModel _viewModel;
        public DreamService _dreamService { get; }

        public MainPage()
        {
            InitializeComponent();

            _dreamService = new DreamService(); // Instantiate DreamService here
            _viewModel = new DreamsViewModel(_dreamService);
            BindingContext = _viewModel;
        }

        private void DreamAddButton_ClickedAsync(object sender, EventArgs e)
        {
            var dreamEntryPage = new DreamEntryPage();
            dreamEntryPage.DreamSaved += DreamEntryPage_DreamSaved;
            Navigation.PushModalAsync(new NavigationPage(dreamEntryPage));
        }

        private async void DreamEntryPage_DreamSaved(object sender, Dream dream)
        {
            // Handle the received dream details here
            _dreamService.AddDream(dream);

            // Close the sub-content view
            await Navigation.PopModalAsync();
        }
        //private void OnCounterClicked(object sender, EventArgs e)
        //{
        //count++;
        //}
    }

}
