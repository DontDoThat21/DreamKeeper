using DreamKeeper.Data;
using DreamKeeper.Data.Services;
using DreamKeeper.Services;
using DreamKeeper.ViewModels;
using Microsoft.Extensions.Logging;

namespace DreamKeeper
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register database context as a service


            //builder.Services.AddTransient<DreamService>(); // Register DreamService if needed

            //builder.Services.AddTransient<DreamsViewModel>(); // Register DreamsViewModel if needed

            //builder.Services.AddSingleton<MainPage>(); // Register MainPage if needed

#if DEBUG
            //SQLiteDbService.DisposeDatabase();
            SQLiteDbService.InitializeDatabase();
            builder.Logging.AddDebug();
#else
            SqliteDbService.InitializeDatabase();
#endif

            return builder.Build();
        }
    }
}
