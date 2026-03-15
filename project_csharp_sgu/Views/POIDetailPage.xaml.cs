using System.Diagnostics;
using project_csharp_sgu.Models;
using project_csharp_sgu.Services;

namespace project_csharp_sgu.Views;

public partial class POIDetailPage : ContentPage
{
    private readonly POI _poi;
    private readonly AudioService _audioService;

    public POIDetailPage(POI poi)
    {
        InitializeComponent();

        _poi = poi;
        _audioService = new AudioService();

        // Gán dữ liệu lên UI
        PoiTitle.Text = _poi.Title;
        PoiDescription.Text = _poi.Description;

        if (!string.IsNullOrEmpty(_poi.ImagePath))
            PoiImage.Source = _poi.ImagePath;
    }

    private async void PlayAudio(object sender, EventArgs e)
    {
        if (_poi == null)
        {
            await DisplayAlert("Error", "POI data is missing.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(_poi.AudioPath))
        {
            await DisplayAlert("Audio Missing", "This POI has no audio file.", "OK");
            return;
        }

        try
        {
            Console.WriteLine(">>> Request play: " + _poi.AudioPath);
            await _audioService.Play(_poi.AudioPath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await DisplayAlert("Audio Error", "Unable to play audio file.", "OK");
        }
    }

    private async void OpenMap(object sender, EventArgs e)
    {
        try
        {
            await Launcher.OpenAsync(_poi.MapUrl);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await DisplayAlert("Map Error", "Cannot open the map link.", "OK");
        }
    }
}