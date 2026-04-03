using project_csharp_sgu.Models;
using project_csharp_sgu.Services;
using ZXing.Net.Maui;

namespace project_csharp_sgu.Pages;

public partial class QrPage : ContentPage
{
    private bool _isProcessing = false;
    private readonly ApiService _apiService = new ApiService();

    public QrPage()
    {
        InitializeComponent();

        barcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false 
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _isProcessing = false;

        try
        {
            var status = await Permissions.RequestAsync<Permissions.Camera>();

            if (status == PermissionStatus.Granted)
            {
                await Task.Delay(500); 
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    barcodeReader.IsDetecting = true;
                });
            }
            else
            {
                await DisplayAlert("Thông báo", "Bạn cần cấp quyền Camera trong Cài đặt máy.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi khởi động Camera: {ex.Message}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        barcodeReader.IsDetecting = false;
    }

    private void BarcodeReader_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessing) return;

        var firstResult = e.Results.FirstOrDefault();
        if (firstResult == null) return;

        _isProcessing = true;

        Dispatcher.Dispatch(async () =>
        {
            string scannedId = firstResult.Value.Trim();

            try 
            {
                // 1. Gọi Backend lấy thông tin gian hàng (bao gồm Lat, Lng)
                var poi = await _apiService.GetPoiByIdFromApiAsync(scannedId, AppState.CurrentLanguage);

                if (poi != null)
                {
                    // 2. Kiểm tra nếu có tọa độ thì hỏi người dùng
                    if (poi.Lat != 0 && poi.Lng != 0)
                    {
                        string action = await DisplayActionSheet($"Tìm thấy: {poi.Name}", "Hủy", null, "Dẫn đường đến đây", "Xem thông tin chi tiết");

                        if (action == "Dẫn đường đến đây")
                        {
                            await OpenMapAsync(poi);
                            _isProcessing = false; // Reset để có thể quét mã khác sau khi quay lại
                        }
                        else if (action == "Xem thông tin chi tiết")
                        {
                            await Navigation.PushAsync(new PoiDetailPage(poi, true));
                        }
                        else
                        {
                            _isProcessing = false; // Nhấn Hủy thì cho quét lại
                        }
                    }
                    else
                    {
                        // Nếu không có tọa độ thì vào thẳng trang chi tiết
                        await Navigation.PushAsync(new PoiDetailPage(poi, true));
                    }
                }
                else
                {
                    await DisplayAlert("Thông báo", $"Mã QR '{scannedId}' không tồn tại.", "OK");
                    _isProcessing = false;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi kết nối", "Không thể lấy dữ liệu tọa độ. Kiểm tra API!", "OK");
                _isProcessing = false;
            }
        });
    }

    // Hàm mở ứng dụng bản đồ (Google Maps) trên điện thoại
    private async Task OpenMapAsync(Poi poi)
    {
        try
        {
            var location = new Location(poi.Lat, poi.Lng);
            var options = new MapLaunchOptions { Name = poi.Name, NavigationMode = NavigationMode.Driving };

            await Map.Default.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", "Không thể mở ứng dụng bản đồ: " + ex.Message, "OK");
        }
    }

    private void OnGenerateClicked(object sender, EventArgs e)
    {
        if (IdGeneratorEntry != null && !string.IsNullOrEmpty(IdGeneratorEntry.Text))
        {
            qrGenerator.Value = IdGeneratorEntry.Text.Trim();
        }
    }
}