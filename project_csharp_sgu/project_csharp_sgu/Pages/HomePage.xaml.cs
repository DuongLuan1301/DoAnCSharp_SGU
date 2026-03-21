using Microsoft.Maui.Controls;

namespace project_csharp_sgu.Pages;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    // Nút chuyển sang LanguagePage
    private async void OnLanguageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LanguagePage());
    }

    // (Optional) Khi load trang
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Sau này có thể load GPS tại đây
        // Hiện tại chỉ demo
        Console.WriteLine("HomePage Loaded");
    }
}