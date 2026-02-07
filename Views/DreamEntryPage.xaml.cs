using DreamKeeper.Data.Models;

namespace DreamKeeper.Views
{
    public partial class DreamEntryPage : ContentPage
    {
        /// <summary>
        /// Event raised when a dream is saved. The parent page handles persistence.
        /// </summary>
        public event EventHandler<Dream>? DreamSaved;

        public DreamEntryPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates a new Dream object, raises the DreamSaved event, and dismisses the modal.
        /// </summary>
        private async void OnSaveDreamClicked(object? sender, EventArgs e)
        {
            var dream = new Dream
            {
                DreamName = string.IsNullOrWhiteSpace(TitleEntry.Text)
                    ? "Untitled Dream"
                    : TitleEntry.Text,
                DreamDescription = DescriptionEditor.Text ?? string.Empty,
                DreamDate = DreamDatePicker.Date ?? DateTime.Now
            };

            DreamSaved?.Invoke(this, dream);
            await Navigation.PopModalAsync();
        }

        /// <summary>
        /// Cancel: dismisses the modal without saving.
        /// </summary>
        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
