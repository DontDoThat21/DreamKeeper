using CommunityToolkit.Maui;
using DreamKeeper.Data.Data;
using DreamKeeper.Data.Services;
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
                .UseMauiApp<App>();

#if ANDROID26_0_OR_GREATER || IOS15_0_OR_GREATER || MACCATALYST15_0_OR_GREATER || TIZEN6_5_OR_GREATER || WINDOWS10_0_17763_0_OR_GREATER
            builder.UseMauiCommunityToolkitMediaElement(isAndroidForegroundServiceEnabled: true);
#endif

            builder.ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

            // ViewModel (singleton)
            builder.Services.AddSingleton<DreamsViewModel>();

            // Audio manager
            builder.Services.AddSingleton(AudioManager.Current);

            // Platform-specific audio recording service
            builder.Services.AddSingleton<IAudioRecorderService>(sp =>
            {
#if ANDROID
                return new DreamKeeper.Platforms.Android.Services.AudioRecorderService();
#elif IOS
                return new DreamKeeper.Platforms.iOS.Services.AudioRecorderService();
#elif WINDOWS
                return new DreamKeeper.Platforms.Windows.Services.AudioRecorderService();
#else
                throw new NotImplementedException("Audio recording service not available on this platform.");
#endif
            });

            // Database initialization
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "dream_database.db3");
            ConfigurationLoader.SetDatabasePath(dbPath);
            SQLiteDbService.InitializeDatabase();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
