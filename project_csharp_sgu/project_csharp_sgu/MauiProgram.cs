using Microsoft.Extensions.Logging;
using project_csharp_sgu.Pages;
using ZXing.Net.Maui.Controls; // 1. Thêm namespace này

namespace project_csharp_sgu;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader() // 2. Đăng ký tính năng quét mã
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Pages
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<LanguagePage>();
        builder.Services.AddSingleton<PoiListPage>();
        // Lưu ý: PoiDetailPage nên là Transient vì mỗi lần mở là một quán khác nhau
        builder.Services.AddTransient<PoiDetailPage>(); 
        builder.Services.AddTransient<QrPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}