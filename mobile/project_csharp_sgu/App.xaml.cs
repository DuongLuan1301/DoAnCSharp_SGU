using project_csharp_sgu.Services;

#nullable enable

namespace project_csharp_sgu;

public partial class App : Application
{
    //Constructor của App, chạy khi app khởi động
    public App()
    {
        InitializeComponent();

        //Khởi tạo dịch vụ langauge serice
        ILanguageService languageService = new LanguageService();

        //lấy ngôn ngữ đã lưu trong local
        AppState.CurrentLanguage = languageService.GetLanguage();

        //Kiểm tra CurrentLanguage
        if (string.IsNullOrEmpty(AppState.CurrentLanguage))
        {
            AppState.CurrentLanguage = "en";
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}