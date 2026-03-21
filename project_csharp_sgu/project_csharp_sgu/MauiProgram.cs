using Microsoft.Extensions.Logging;
using project_csharp_sgu.Pages;

namespace project_csharp_sgu;

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

        // Register Pages
		builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<LanguagePage>();
        builder.Services.AddSingleton<PoiListPage>();
        builder.Services.AddSingleton<PoiDetailPage>();
        builder.Services.AddSingleton<QrPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}