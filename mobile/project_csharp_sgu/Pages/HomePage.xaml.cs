using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;   // 🔹 dùng Geolocation, Geocoding
using Microsoft.Maui.ApplicationModel; // 🔹 dùng Permissions, MainThread
using Mapsui;
using Mapsui.Tiling;
using Mapsui.UI.Maui;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Projections;

#nullable enable

namespace project_csharp_sgu.Pages;

public partial class HomePage : ContentPage
{
    // 🔹 dùng để dừng timer khi rời trang (tránh leak)
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private bool isFirst = true;

    public HomePage()
    {
        InitializeComponent();
        LoadLanguage();
    }

    // 🔹 nút chuyển sang trang chọn ngôn ngữ
    private async void OnLanguageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LanguagePage());
    }

    // 🔹 khi trang xuất hiện
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        isFirst = true;

        LoadLanguage();
        // 🔥 đảm bảo UI luôn đúng ngôn ngữ

        InitMap();

        await RequestLocationPermission();
        // 🔥 xin quyền GPS

        StartLocationTracking();
        // 🔥 bắt đầu lấy GPS mỗi 10 giây
    }

    // 🔹 khi rời trang → dừng GPS
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _cts?.Cancel();
        // 🔥 dừng timer → tránh chạy ngầm
    }

    private void InitMap()
    {
        var map = new Mapsui.Map();

        // load map từ OpenStreetMap
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        MyMap.Map = map;
    }

    // =====================================================
    // 🔥 1. XIN QUYỀN GPS
    // =====================================================
    private async Task RequestLocationPermission()
    {
        // 🔹 kiểm tra đã có quyền chưa
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            // 🔹 nếu chưa có → yêu cầu user cấp quyền
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (status != PermissionStatus.Granted)
        {
            // 🔹 nếu user từ chối
            await DisplayAlertAsync("Error", "Location permission denied", "OK");
        }
    }

    // =====================================================
    // 🔥 2. TIMER LẤY GPS MỖI 10 GIÂY
    // =====================================================
    private void StartLocationTracking()
    {
        _cts = new CancellationTokenSource();

        Dispatcher.StartTimer(TimeSpan.FromSeconds(10), () =>
        {
            // 🔹 gọi hàm async (không await trực tiếp)
            _ = GetLocationAsync();

            // 🔹 nếu true → timer tiếp tục
            // 🔹 nếu false → timer dừng
            return !_cts.IsCancellationRequested;
        });
    }

    // =====================================================
    // 🔥 3. LẤY GPS + ĐỊA CHỈ
    // =====================================================
    private async Task GetLocationAsync()
    {
        try
        {
            // 🔹 yêu cầu GPS với độ chính xác cao
            var request = new GeolocationRequest(GeolocationAccuracy.High);

            // 🔹 lấy tọa độ hiện tại
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                double lat = location.Latitude;
                double lon = location.Longitude;

                MainThread.BeginInvokeOnMainThread(() => { UpdateMapLocation(lat, lon); });

                // 🔥 REVERSE GEOCODING → từ tọa độ ra địa chỉ
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(lat, lon);

                var place = placemarks?.FirstOrDefault();

                string address = "Unknown";

                if (place != null)
                {
                    // 🔹 lấy từng thành phần (có fallback)
                    string street = place.Thoroughfare ?? "";

                    // 🔥 phường (ưu tiên SubLocality → fallback FeatureName)
                    string ward =
                        place.SubLocality ??      // thường là phường
                        place.FeatureName ??
                        "";

                    // 🔥 thành phố
                    string city =
                        place.AdminArea ??        // TP.HCM
                        place.Locality ??         // fallback
                        "";

                    // 🔹 build address tránh trùng lặp
                    var parts = new List<string>();

                    if (!string.IsNullOrWhiteSpace(ward))
                        parts.Add(ward);

                    if (!string.IsNullOrWhiteSpace(street))
                        parts.Add(street);

                    if (!string.IsNullOrWhiteSpace(city))
                        parts.Add(city);

                    address = string.Join(", ", parts);
                }
                // 🔥 update UI phải chạy trên main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateLocationUI(lat, lon, address);
                });
            }
        }
        catch (Exception ex)
        {
            // 🔹 debug lỗi
            Console.WriteLine($"GPS Error: {ex.Message}");
        }
    }

    private void UpdateMapLocation(double lat, double lon)
    {
        if (MyMap == null || MyMap.Map == null)
            return;

        var (x, y) = SphericalMercator.FromLonLat(lon, lat);
        var point = new MPoint(x, y);

        // 🔥 chỉ zoom lần đầu
        if (isFirst)
        {
            MyMap.Map.Navigator.CenterOn(point);
            MyMap.Map.Navigator.ZoomTo(1);
            isFirst = false;
        }
        else
        {
            MyMap.Map.Navigator.CenterOn(point);
        }

        // ===== giữ nguyên phần marker của bạn =====
        var feature = new PointFeature(point)
        {
            Styles = new[]
            {
            new SymbolStyle { SymbolScale = 0.8 }
        }
        };

        var layer = new MemoryLayer
        {
            Features = new[] { feature }
        };

        var oldLayers = MyMap.Map.Layers
            .Where(l => l is MemoryLayer)
            .ToList();

        foreach (var l in oldLayers)
            MyMap.Map.Layers.Remove(l);

        MyMap.Map.Layers.Add(layer);

        MyMap.Refresh();
    }
    // =====================================================
    // 🔥 4. UPDATE UI THEO NGÔN NGỮ
    // =====================================================
    private void UpdateLocationUI(double lat, double lon, string address)
    {
        string lang = AppState.CurrentLanguage;

        if (lang == "ja")
        {
            LocationLabel.Text = $"📍 位置: {lat:F6}, {lon:F6}";
            AddressLabel.Text = $"📌 住所: {address}";
        }
        else if (lang == "zh")
        {
            LocationLabel.Text = $"📍 位置: {lat:F6}, {lon:F6}";
            AddressLabel.Text = $"📌 地址: {address}";
        }
        else
        {
            // 🔹 mặc định English
            LocationLabel.Text = $"📍 Location: {lat:F6}, {lon:F6}";
            AddressLabel.Text = $"📌 Address: {address}";
        }
    }

    // =====================================================
    // 🔥 5. LOAD NGÔN NGỮ BAN ĐẦU (KHI CHƯA CÓ GPS)
    // =====================================================
    private void LoadLanguage()
    {
        string lang = AppState.CurrentLanguage;

        // 🔹 fallback (khi chưa có GPS)
        string latlng;
        string address;

        if (lang == "ja")
        {
            latlng = "読み込み中...";
            address = "読み込み中...";
            LocationLabel.Text = $"📍 位置: {latlng}";
            AddressLabel.Text = $"📌 住所: {address}";
        }
        else if (lang == "zh")
        {
            latlng = "加载中...";
            address = "加载中...";
            LocationLabel.Text = $"📍 位置: {latlng}";
            AddressLabel.Text = $"📌 地址: {address}";
        }
        else
        {
            latlng = "loading...";
            address = "loading...";
            LocationLabel.Text = $"📍 Location: {latlng}";
            AddressLabel.Text = $"📌 Address: {address}";
        }
    }
}