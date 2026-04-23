using Microsoft.Maui.Controls;
using project_csharp_sgu.Services;

#nullable enable

namespace project_csharp_sgu.Pages;

public partial class LanguagePage : ContentPage
{
    public LanguagePage()
    {
        InitializeComponent(); // 🔹 load UI từ XAML

        // 🔥 KÍCH HOẠT NHỊP TIM ONLINE
        project_csharp_sgu.Services.HeartbeatService.StartHeartbeat();
    }

    //Hàm này được gọi khi user bấm vào 1 button ngôn ngữ
    private async void OnLanguageSelected(object sender, EventArgs e)
    {
        //kiểm tra xem cái được click có phải Button không
        if (sender is Button btn)
        {
            string langCode = "en"; // mặc định English

            //xác định ngôn ngữ dựa trên text của button
            switch (btn)
            {
                case Button b when b == enButton:
                    langCode = "en";
                    break;

                case Button b when b == jpButton:
                    langCode = "ja";
                    break;

                case Button b when b == cnButton:
                    langCode = "zh";
                    break;
            }

            //tạo service để lưu ngôn ngữ
            ILanguageService languageService = new LanguageService();

            //lưu ngôn ngữ vào Preferences (bộ nhớ local)
            languageService.SetLanguage(langCode);

            //cập nhật global state
            AppState.CurrentLanguage = langCode;
        }

        //reload app để áp dụng ngôn ngữ mới
        if (Application.Current?.Windows.Count > 0)
        {
            Application.Current.Windows[0].Page = new AppShell();
        }
    }
}