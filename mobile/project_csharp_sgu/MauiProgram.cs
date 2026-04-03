using Microsoft.Extensions.Logging;
using project_csharp_sgu.Pages;
using ZXing.Net.Maui.Controls; // 1. PHẢI CÓ dòng này

namespace project_csharp_sgu;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {   
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseBarcodeReader() // 2. QUAN TRỌNG: Kích hoạt camera quét QR tại đây
            .ConfigureFonts(fonts =>
            {                 
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 3. Register Pages (Dependency Injection)
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<LanguagePage>();
        builder.Services.AddSingleton<PoiListPage>();
        builder.Services.AddSingleton<QrPage>();

        // LƯU Ý: Không nên đăng ký PoiDetailPage ở đây vì nó có Constructor 
        // nhận tham số (Poi poi), DI của MAUI sẽ không tự hiểu được cái 'poi' đó là gì.
        // Bạn sẽ dùng 'new PoiDetailPage(poi, true)' như đã làm ở các trang khác.

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}