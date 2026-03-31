using Microsoft.Maui.Controls;
using project_csharp_sgu.Services;

namespace project_csharp_sgu.Pages;

public partial class LanguagePage : ContentPage
{
    public LanguagePage()
    {
        InitializeComponent(); // 🔹 load UI từ XAML
    }

    // 🔹 Hàm này được gọi khi user bấm vào 1 button ngôn ngữ
    private async void OnLanguageSelected(object sender, EventArgs e)
    {
        // 🔹 kiểm tra xem cái được click có phải Button không
        if (sender is Button btn)
        {
            string langCode = "en"; // mặc định English

            // 🔹 xác định ngôn ngữ dựa trên text của button
            switch (btn.Text)
            {
                case "English": langCode = "en"; break;
                case "日本語": langCode = "ja"; break;
                case "中文": langCode = "zh"; break;
            }

            // 🔹 tạo service để lưu ngôn ngữ
            ILanguageService languageService = new LanguageService();

            // 🔹 lưu ngôn ngữ vào Preferences (bộ nhớ local)
            languageService.SetLanguage(langCode);

            // 🔥 cập nhật global state
            AppState.CurrentLanguage = langCode;
        }

        // 🔥 reload app để áp dụng ngôn ngữ mới
        Application.Current.MainPage = new AppShell();
    }
}