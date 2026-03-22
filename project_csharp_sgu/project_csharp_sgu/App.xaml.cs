using project_csharp_sgu.Services;

namespace project_csharp_sgu;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        //Khởi tạo dịch vụ langauge serice
        ILanguageService languageService = new LanguageService();
        //lấy ngôn ngữ đã lưu trong local, nếu chưa thì mặc định là english
        AppState.CurrentLanguage = languageService.GetLanguage();

        if (string.IsNullOrEmpty(AppState.CurrentLanguage))
        {
            AppState.CurrentLanguage = "en";
        }

        MainPage = new AppShell();
    }
}