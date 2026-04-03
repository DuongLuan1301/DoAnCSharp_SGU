using Microsoft.Extensions.Logging;
using project_csharp_sgu.Pages;
using project_csharp_sgu.Services;

namespace project_csharp_sgu;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {   
        //Tạo builder
        var builder = MauiApp.CreateBuilder();

        //Register fonts
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {                 
                //fonts.AddFont("fileName", "alias");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
        // Register Services
        builder.Services.AddSingleton<IAudioService, AudioService>();
        builder.Services.AddSingleton<LocationService>();

        // Register Pages
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<LanguagePage>();
        builder.Services.AddSingleton<PoiListPage>();
        builder.Services.AddSingleton<PoiDetailPage>();
        builder.Services.AddSingleton<QrPage>();

        return builder.Build();
    }
}