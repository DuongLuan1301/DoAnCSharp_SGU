using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using project_csharp_sgu.Models;

namespace project_csharp_sgu.Pages;

public partial class PoiDetailPage : ContentPage
{
    private Poi _poi;

    // ✅ constructor nhận dữ liệu
    public PoiDetailPage(Poi poi)
    {
        InitializeComponent(); // ⚠️ dòng này phải có

        _poi = poi;
        BindingContext = _poi;
    }

    // ✅ nút play audio
    private async void OnPlayAudioClicked(object sender, EventArgs e)
    {
        if (_poi != null)
        {
            await TextToSpeech.SpeakAsync(_poi.Description);
        }
    }

    // ✅ nút đóng
    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}