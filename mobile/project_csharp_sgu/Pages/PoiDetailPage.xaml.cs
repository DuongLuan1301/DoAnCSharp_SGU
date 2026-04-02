using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using project_csharp_sgu.Models;
using project_csharp_sgu.Services;

namespace project_csharp_sgu.Pages;

public partial class PoiDetailPage : ContentPage
{
    private Poi _poi;

    private readonly IAudioService _audioService;

    //constructor
    public PoiDetailPage(Poi poi)
    {
        InitializeComponent();

        _poi = poi;

        //lấy AudioService từ DI container
        _audioService = Application.Current
            .Handler
            .MauiContext
            .Services
            .GetService<IAudioService>();

        // bind dữ liệu sang UI
        BindingContext = _poi;
    }

    private async void OnPlayAudioClicked(object sender, EventArgs e)
    {
        if (_poi == null || string.IsNullOrWhiteSpace(_poi.description))
            return;

        // Nếu đang phát → STOP
        if (_audioService.IsPlaying)
        {
            _audioService.Stop();
            AudioButton.Text = "▶️ Play Audio";
            return;
        }

        // PLAY
        AudioButton.Text = "⏸️ Stop Audio";

        await _audioService.PlayAsync(_poi, AppState.CurrentLanguage);

        // khi đọc xong
        AudioButton.Text = "▶️ Play Audio";
    }

    // ĐÓNG POPUP
    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}