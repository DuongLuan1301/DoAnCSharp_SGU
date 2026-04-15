using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using project_csharp_sgu.Models;
using project_csharp_sgu.Services;

#nullable enable

namespace project_csharp_sgu.Pages;

public partial class PoiDetailPage : ContentPage
{
    private Poi? _poi;
    private IAudioService? _audioService;

        //constructor
        public PoiDetailPage(Poi poi)
        {
            InitializeComponent();

            _poi = poi;

            //lấy AudioService từ DI container
            _audioService = Application.Current?
                .Handler
                .MauiContext
                .Services
                .GetService<IAudioService>();

            // Debug: in ra dữ liệu POI để xem có gì
            System.Diagnostics.Debug.WriteLine($"[PoiDetailPage] ===== POI DETAILS =====");
            System.Diagnostics.Debug.WriteLine($"  Name: {_poi?.Name}");
            System.Diagnostics.Debug.WriteLine($"  Address: {_poi?.Address}");
            System.Diagnostics.Debug.WriteLine($"  Image: {_poi?.Image}");
            System.Diagnostics.Debug.WriteLine($"  Image is null or empty: {string.IsNullOrWhiteSpace(_poi?.Image)}");
            System.Diagnostics.Debug.WriteLine($"  Description: {_poi?.Description?.Substring(0, Math.Min(50, _poi.Description?.Length ?? 0))}...");
            System.Diagnostics.Debug.WriteLine($"[PoiDetailPage] =====================");

            // bind dữ liệu sang UI
            BindingContext = _poi;
        }

      private async void OnPlayAudioClicked(object sender, EventArgs e)
    {
        if (_poi == null || string.IsNullOrWhiteSpace(_poi.Description) || _audioService == null)
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

        // 🔥 GỌI API GHI NHẬN NGHE AUDIO CHẠY NGẦM
        if (!string.IsNullOrEmpty(_poi.Id))
        {
            _ = TrackInteractionAsync(_poi.Id, "listen-audio");
        }

        await _audioService.PlayAsync(_poi, AppState.CurrentLanguage);

        // khi đọc xong
        AudioButton.Text = "▶️ Play Audio";
    }

    // 🔥 HÀM GỌI API LƯU LÊN MONGODB
    private async Task TrackInteractionAsync(string poiId, string action)
    {
        try {
            using var client = new HttpClient();
            string url = $"http://10.0.2.2:5188/api/poi/{poiId}/{action}";
            var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");
            await client.PostAsync(url, content);
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"[Tracking Error] {action}: {ex.Message}");
        }
    }}