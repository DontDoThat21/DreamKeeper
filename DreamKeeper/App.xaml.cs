using DreamKeeper.Data;

namespace DreamKeeper
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

    }
}
