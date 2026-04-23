using ZXing.Net.Maui;
using System.Net.Http.Json;
using project_csharp_sgu.Models;

namespace project_csharp_sgu.Pages;

public partial class QrPage : ContentPage
{
    private bool _isScanning = true;

    public QrPage()
    {
        InitializeComponent();
        barcodeReader.Options = new BarcodeReaderOptions {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
    }

    private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (!_isScanning) return;
        _isScanning = false;

        var result = e.Results.FirstOrDefault();
        if (result == null) { _isScanning = true; return; }

        MainThread.BeginInvokeOnMainThread(async () => {
            await FetchAndNavigate(result.Value);
        });
    }

    private async Task FetchAndNavigate(string id)
    {
        using var client = new HttpClient();
        string cleanId = id.Trim();
        string url = $"{Constants.BaseApiUrl}/api/poi/{cleanId}?lang={AppState.CurrentLanguage}";

        try {
            var poi = await client.GetFromJsonAsync<Poi>(url);
            if (poi != null) {
                // GỌI API GHI NHẬN QUÉT QR CHẠY NGẦM
                _ = TrackInteractionAsync(cleanId, "scan-qr");

                await Navigation.PushAsync(new PoiDetailPage(poi));
            } else {
                await DisplayAlertAsync("Lỗi", "Không tìm thấy địa điểm", "OK");
                _isScanning = true; 
            }
        }
        catch (Exception ex) {
            await DisplayAlertAsync("Lỗi", $"Lỗi kết nối API: {ex.Message}", "OK");
            _isScanning = true; 
        }
    }

    // 🔥 HÀM GỌI API LƯU LÊN MONGODB
    private async Task TrackInteractionAsync(string poiId, string action)
    {
        try {
            using var client = new HttpClient();
            string url = $"{Constants.BaseApiUrl}/api/poi/{poiId}/{action}";
            var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");
            await client.PostAsync(url, content);
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"[Tracking Error] {action}: {ex.Message}");
        }
    }

    protected override void OnAppearing() {
        base.OnAppearing();

        // 🔥 KÍCH HOẠT NHỊP TIM ONLINE
        project_csharp_sgu.Services.HeartbeatService.StartHeartbeat();

        _isScanning = true;
        barcodeReader.IsDetecting = true;
    }

    protected override void OnDisappearing() {
        base.OnDisappearing();
        _isScanning = false;
        barcodeReader.IsDetecting = false;
    }
}