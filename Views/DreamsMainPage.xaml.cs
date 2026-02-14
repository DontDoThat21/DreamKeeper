using DreamKeeper.Data;
using DreamKeeper.Data.Data;
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
            var transcriptionService = Application.Current?.MainPage?.Handler?.MauiContext?.Services.GetService<ITranscriptionService>();
            _viewModel = new DreamsViewModel(new DreamService(), new AudioManager(), transcriptionService);
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
        /// Opens the DreamEntryPage modal
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
    }
}
