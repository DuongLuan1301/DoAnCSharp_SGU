using Microsoft.Maui.Controls;

namespace project_csharp_sgu.Pages;

public partial class LanguagePage : ContentPage
{
    public LanguagePage()
    {
        InitializeComponent();
    }

    private async void OnLanguageSelected(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            string selectedLanguage = btn.Text;

            // TODO: Sau này lưu vào Preferences
            Console.WriteLine($"Selected language: {selectedLanguage}");
        }

        // Quay về HomePage
        await Navigation.PopAsync();
    }
}