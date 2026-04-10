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

        // Chạy trên MainThread để tránh văng app
        MainThread.BeginInvokeOnMainThread(async () => {
            await FetchAndNavigate(result.Value);
        });
    }

    private async Task FetchAndNavigate(string id)
    {
        using var client = new HttpClient();
        string url = $"http://10.0.2.2:5188/api/poi/{id.Trim()}?lang={AppState.CurrentLanguage}";

        try {
            var poi = await client.GetFromJsonAsync<Poi>(url);
            if (poi != null) {
                await Navigation.PushAsync(new PoiDetailPage(poi));
                // Không set lại _isScanning vì trang đã thay đổi
            } else {
                await DisplayAlertAsync("Lỗi", "Không tìm thấy địa điểm", "OK");
                _isScanning = true; // Cho phép quét lại
            }
        }
        catch (Exception ex) {
            await DisplayAlertAsync("Lỗi", $"Lỗi kết nối API: {ex.Message}", "OK");
            _isScanning = true; // Cho phép quét lại
        }
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        _isScanning = true;
        barcodeReader.IsDetecting = true;
    }

    protected override void OnDisappearing() {
        base.OnDisappearing();
        _isScanning = false;
        barcodeReader.IsDetecting = false;
    }}