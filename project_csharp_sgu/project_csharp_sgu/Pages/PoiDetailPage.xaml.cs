using Microsoft.Maui.Media;
using project_csharp_sgu.Models; // QUAN TRỌNG: Phải có dòng này

namespace project_csharp_sgu.Pages;

public partial class PoiDetailPage : ContentPage
{
    private Poi _poi;
    private bool _shouldAutoPlay;

    // Đảm bảo kiểu dữ liệu ở đây là Poi (đã được nhận diện từ namespace Models)
    public PoiDetailPage(Poi poi, bool autoPlay = false)
    {
        InitializeComponent();
        _poi = poi;
        _shouldAutoPlay = autoPlay;
        BindingContext = _poi;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_shouldAutoPlay && _poi != null && !string.IsNullOrEmpty(_poi.Description))
        {
            await TextToSpeech.Default.SpeakAsync(_poi.Description);
        }
    }

    private async void OnPlayAudioClicked(object sender, EventArgs e)
    {
        if (_poi != null)
        {
            await TextToSpeech.Default.SpeakAsync(_poi.Description);
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}