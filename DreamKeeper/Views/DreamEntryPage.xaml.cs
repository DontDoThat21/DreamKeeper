using DreamKeeper.Models;

namespace DreamKeeper.Views;

public partial class DreamEntryPage : ContentPage
{
    public event EventHandler<Dream> DreamSaved;

    public DreamEntryPage()
    {
        InitializeComponent();
    }

    private void SaveButton_Clicked(object sender, EventArgs e)
    {
        var dream = new Dream
        {
            DreamName = DreamNameEntry.Text,
            DreamDescription = DreamDescriptionEntry.Text,
            DreamDate = DreamDatePicker.Date
        };

        // Raise event to notify parent view with the entered dream details
        DreamSaved?.Invoke(this, dream);
    }
    
    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}