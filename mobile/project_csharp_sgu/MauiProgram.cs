using Microsoft.Extensions.Logging;
using project_csharp_sgu.Pages;
using project_csharp_sgu.Services;
using SkiaSharp.Views.Maui.Controls.Hosting;
using ZXing.Net.Maui.Controls; // 1. Thêm namespace này

namespace project_csharp_sgu;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Tạo builder
        var builder = MauiApp.CreateBuilder();

        // Register fonts & Plugins
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .UseBarcodeReader() // 2. Kích hoạt tính năng quét QR (Cực kỳ quan trọng)
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Register Services
        builder.Services.AddSingleton<IAudioService, AudioService>();
        builder.Services.AddSingleton<LocationService>();

        // Register Pages
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<LanguagePage>();
        builder.Services.AddSingleton<PoiListPage>();
        builder.Services.AddSingleton<QrPage>();

        // 3. LƯU Ý: Xóa hoặc Comment dòng PoiDetailPage
        // builder.Services.AddSingleton<PoiDetailPage>(); 
        // Lý do: PoiDetailPage cần tham số 'Poi' truyền vào, DI không tự khởi tạo được.

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}