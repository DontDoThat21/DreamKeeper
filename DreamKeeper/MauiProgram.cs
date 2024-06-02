using DreamKeeper.Data;
using DreamKeeper.Data.Data;
using DreamKeeper.Data.Services;
using DreamKeeper.Services;
using DreamKeeper.ViewModels;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;

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

            builder.Services.AddSingleton<DreamsViewModel>();

            // Register platform-specific implementations for IAudioRecorderService
            builder.Services.AddSingleton<IAudioRecorderService>(s =>
            {
#if ANDROID
                return new YourNamespace.Droid.Services.AudioRecorderService();
#elif IOS
                return new YourNamespace.iOS.Services.AudioRecorderService();
#else
                throw new NotImplementedException("Audio recording is not implemented for this platform.");
#endif
            });

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
