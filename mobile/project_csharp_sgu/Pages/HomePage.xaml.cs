using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;   // 🔹 dùng Geolocation, Geocoding
using Microsoft.Maui.ApplicationModel; // 🔹 dùng Permissions, MainThread
using Mapsui;
using Mapsui.Tiling;
using Mapsui.UI.Maui;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Projections;
using System.Net.Http.Json;
using project_csharp_sgu.Models;
using MapsuiColor = Mapsui.Styles.Color;
using MapsuiBrush = Mapsui.Styles.Brush;

#nullable enable

namespace project_csharp_sgu.Pages;

public partial class HomePage : ContentPage
{
    // 🔹 dùng để dừng timer khi rời trang (tránh leak)
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private bool isFirst = true;
    private bool _mapInitialized = false;
    private MemoryLayer? _poiLayer;
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

        InitMap(); // 🔥 load map ngay lập tức

        // 🔥 chạy nền, KHÔNG block UI
        _ = Task.Run(async () =>
        {
            await LoadPoisToMap();
        });

        _ = Task.Run(async () =>
        {
            await RequestLocationPermission();
            StartLocationTracking();
        });
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
        if (_mapInitialized) return;

        var map = new Mapsui.Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        MyMap.Map = map;

        _mapInitialized = true;
    }
    private async Task<List<Poi>> GetPoisAsync()
    {
        using var client = new HttpClient();

        var pois = await client.GetFromJsonAsync<List<Poi>>(
            "http://10.0.2.2:5188/admin/poi"
        );

        return pois ?? new List<Poi>();
    }
    private async Task LoadPoisToMap()
    {
        if (MyMap?.Map == null) return;

        var pois = await GetPoisAsync();

        var features = new List<IFeature>();

        foreach (var poi in pois)
        {
            var (x, y) = SphericalMercator.FromLonLat(poi.Lng, poi.Lat);

            var feature = new PointFeature(new MPoint(x, y));

            // style marker cho POI
            feature.Styles.Add(new SymbolStyle
            {
                SymbolScale = 0.6,
                Fill = new Mapsui.Styles.Brush(MapsuiColor.FromArgb(255, 255, 87, 34)),
                Outline = new Pen(MapsuiColor.Black, 1)
            });

            features.Add(feature);
        }

        _poiLayer = new MemoryLayer
        {
            Features = features
        };

        MyMap.Map.Layers.Add(_poiLayer);
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
    // 🔥 2. TIMER LẤY GPS MỖI 20 GIÂY
    // =====================================================
    private void StartLocationTracking()
    {
        _cts = new CancellationTokenSource();

        Dispatcher.StartTimer(TimeSpan.FromSeconds(20), () =>
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

        // ===== phần marker của user location =====
        var feature = new PointFeature(point)
        {
            Styles = new[]{new SymbolStyle{
                SymbolScale = 0.6,
                Fill = new MapsuiBrush(MapsuiColor.FromArgb(255, 33, 150, 243)),
            Outline = new Pen(MapsuiColor.Black, 1)
            }
            }
        };

        var layer = new MemoryLayer
        {
            Features = new[] { feature }
        };

        var oldLayers = MyMap.Map.Layers
            .Where(l => l is MemoryLayer && l != _poiLayer)
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