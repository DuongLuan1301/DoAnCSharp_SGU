using Microsoft.Maui.Controls.Maps;
using Microsoft.Extensions.Logging;
using project_csharp_sgu.Services;

namespace project_csharp_sgu;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Đăng ký Service (AddSingleton: tạo một instance duy nhất)
        builder.Services.AddSingleton<POIService>();
        builder.Services.AddSingleton<LocationService>();
        builder.Services.AddSingleton<AudioService>();
        builder.Services.AddSingleton<GeofenceEngine>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}