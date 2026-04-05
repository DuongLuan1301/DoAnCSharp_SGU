using project_csharp_sgu.Models;
using project_csharp_sgu.Services;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using System.Diagnostics;
using System.Net.Http.Json;

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
            Debug.WriteLine($"Lỗi khởi động Camera: {ex.Message}");
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

        // Sử dụng Dispatcher để chạy tác vụ UI và Async
        _ = Dispatcher.Dispatch(async () =>
        {
            string scannedId = firstResult.Value.Trim();

            try
            {
                // 1. Gọi Backend lấy thông tin (Biến lang lấy từ AppState của bạn)
                var poi = await _apiService.GetPoiByIdFromApiAsync(scannedId, AppState.CurrentLanguage);

                if (poi != null)
                {
                    // 2. Kiểm tra nếu có tọa độ (sử dụng tên biến viết thường lat, lng)
                    if (poi.lat != 0 && poi.lng != 0)
                    {
                        string action = await DisplayActionSheet($"Tìm thấy: {poi.name}", "Hủy", null, "Dẫn đường đến đây", "Xem thông tin chi tiết");

                        if (action == "Dẫn đường đến đây")
                        {
                            await OpenMapAsync(poi);
                            _isProcessing = false;
                        }
                        else if (action == "Xem thông tin chi tiết")
                        {
                            await Navigation.PushAsync(new PoiDetailPage(poi, true));
                            _isProcessing = false;
                        }
                        else
                        {
                            _isProcessing = false; // Nhấn Hủy hoặc bấm ra ngoài
                        }
                    }
                    else
                    {
                        // Nếu không có tọa độ thì vào thẳng trang chi tiết
                        await Navigation.PushAsync(new PoiDetailPage(poi, true));
                        _isProcessing = false;
                    }
                }
                else
                {
                    await DisplayAlert("Thông báo", $"Mã QR '{scannedId}' không tồn tại trong hệ thống.", "OK");
                    _isProcessing = false;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi kết nối", "Không thể lấy dữ liệu từ API. Vui lòng kiểm tra IP và Server!", "OK");
                Debug.WriteLine($"Lỗi: {ex.Message}");
                _isProcessing = false;
            }
        });
    }

    private async Task OpenMapAsync(Poi poi)
    {
        try
        {
            // Sử dụng biến viết thường poi.lat, poi.lng và poi.name
            var location = new Location(poi.lat, poi.lng);
            var options = new MapLaunchOptions { Name = poi.name, NavigationMode = NavigationMode.Driving };

            await Map.Default.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", "Không thể mở ứng dụng bản đồ: " + ex.Message, "OK");
        }
    }

   private void OnGenerateClicked(object? sender, EventArgs e) // Thêm dấu ? ở đây
    {
        if (IdGeneratorEntry != null && !string.IsNullOrEmpty(IdGeneratorEntry.Text))
        {
            qrGenerator.Value = IdGeneratorEntry.Text.Trim();
        }
    }
}

// TRIỂN KHAI LỚP APISERVICE NGAY TẠI ĐÂY (Hoặc để trong file ApiService.cs riêng)
public class ApiService
{
    private readonly HttpClient _httpClient;
    // 1. SỬA LẠI IP CHUẨN CỦA TÀI TẠI ĐÂY
    private const string BaseUrl = "http://192.168.100.75:5188/api/poi";

    public ApiService()
    {
        _httpClient = new HttpClient();
        // Set timeout ngắn để Tài dễ test lỗi kết nối
        _httpClient.Timeout = TimeSpan.FromSeconds(5); 
    }

    public async Task<Poi> GetPoiByIdFromApiAsync(string id, string lang)
    {
        // 2. KHÔNG DÙNG try-catch Ở ĐÂY để lỗi "văng" ra ngoài QrPage xử lý
        // Như vậy QrPage mới biết là lỗi Mạng hay là lỗi 404 (Không tìm thấy)
        
        var response = await _httpClient.GetAsync($"{BaseUrl}/{id}?lang={lang}");
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Poi>();
        }
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null; // Trường hợp này mới thực sự là không tồn tại
        }

        throw new Exception("Lỗi hệ thống: " + response.StatusCode);
    }
}