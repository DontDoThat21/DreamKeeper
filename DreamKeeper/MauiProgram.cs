using DreamKeeper.Data;
using DreamKeeper.Services;
using DreamKeeper.ViewModels;
using Microsoft.EntityFrameworkCore;
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
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite("Data Source=app.db");
            });

            //builder.Services.AddTransient<DreamService>(); // Register DreamService if needed

            //builder.Services.AddTransient<DreamsViewModel>(); // Register DreamsViewModel if needed

            //builder.Services.AddSingleton<MainPage>(); // Register MainPage if needed

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
