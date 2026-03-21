using Microsoft.Maui.Controls;

namespace project_csharp_sgu.Pages;

public partial class QrPage : ContentPage
{
    public QrPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Console.WriteLine("QR Page Opened");

        // TODO:
        // Sau này tích hợp ZXing.Net.MAUI để scan QR
    }
}