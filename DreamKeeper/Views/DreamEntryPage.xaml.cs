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
            DreamDate = DateTime.Now // You may want to provide a way for users to select the date
        };

        // Raise event to notify parent view with the entered dream details
        DreamSaved?.Invoke(this, dream);
    }
}