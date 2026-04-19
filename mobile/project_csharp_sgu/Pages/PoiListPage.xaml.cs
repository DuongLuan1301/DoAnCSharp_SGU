using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using project_csharp_sgu.Models;
using project_csharp_sgu.Services;

#nullable enable

namespace project_csharp_sgu.Pages;

public partial class PoiListPage : ContentPage
{

    private LocationService? _locationService;

    //Biến lưu danh sách các Pois hợp lệ
    private List<Poi> _allPois = new();

    //Biến lưu toàn bộ Pois từ db
    public ObservableCollection<Poi> Pois { get; set; } = new ObservableCollection<Poi>();

    private IAudioService? _audioService;

    public PoiListPage()
    {
        InitializeComponent();

        _audioService = null;
        _locationService = null;

        _audioService = Application.Current?
            .Handler
            .MauiContext
            .Services
            .GetService<IAudioService>();

        _locationService = Application.Current?
            .Handler
            .MauiContext
            .Services
            .GetService<LocationService>();

        PoiList.ItemsSource = Pois;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            //Lấy dữ liệu Pois theo ngôn ngữ được chọn
            var pois = await LoadPoisAsync(AppState.CurrentLanguage);

            _allPois = pois.ToList(); // lưu toàn bộ pois

            // Hiển thị toàn bộ POI trước
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Pois.Clear();
                foreach (var p in _allPois)
                {
                    p.Distance = "Đang tính...";
                    Pois.Add(p);
                }
            });

            // subscribe GPS (nhận event mỗi khi locationService gửi đến)
            _locationService.LocationUpdated += OnLocationUpdated;

            _locationService.Start(); // đảm bảo GPS chạy

            if (_locationService.CurrentLocation != null)
            {
                OnLocationUpdated(_locationService);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Không thể tải POI: {ex.Message}", "OK");
        }
    }

  public async Task<ObservableCollection<Poi>> LoadPoisAsync(string lang)
{
    using var client = new HttpClient();
    
    // Tách Base URL ra để dùng chung
    string baseUrl = "http://10.0.2.2:5188"; 
    string url = $"{baseUrl}/api/poi?lang={lang}";

    try
    {
        System.Diagnostics.Debug.WriteLine($"[PoiListPage] Loading POIs from: {url}");
        
        var pois = await client.GetFromJsonAsync<List<Poi>>(url);
        
        if (pois == null) 
        {
            System.Diagnostics.Debug.WriteLine("[PoiListPage] API returned null");
            return new ObservableCollection<Poi>();
        }

        System.Diagnostics.Debug.WriteLine($"[PoiListPage] Loaded {pois.Count} POIs:");
       // Kiểm tra chắc chắn _locationService không bị null trước khi dùng
if (_locationService != null)
{
    // subscribe GPS (nhận event mỗi khi locationService gửi đến)
    _locationService.LocationUpdated += OnLocationUpdated;

    _locationService.Start(); // đảm bảo GPS chạy

    if (_locationService.CurrentLocation != null)
    {
        OnLocationUpdated(_locationService);
    }
}
else
{
    System.Diagnostics.Debug.WriteLine("[PoiListPage] Cảnh báo: _locationService bị null!");
}
        return new ObservableCollection<Poi>(pois);
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"[PoiListPage] ERROR: {ex.Message}\n{ex.StackTrace}");
        
        // Chạy DisplayAlert trên MainThread để tránh lỗi crash app
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await DisplayAlertAsync("Error", $"Lỗi kết nối API: {ex.Message}", "OK");
        });
        
        return new ObservableCollection<Poi>();
    }
}

    private void OnLocationUpdated(LocationService service)
    {
        if (service.CurrentLocation == null) return;

        var userLat = service.CurrentLocation.Latitude;
        var userLng = service.CurrentLocation.Longitude;

        // Tính khoảng cách và filter chỉ POI trong khoảng 1-2km
        var updatedPois = new List<Poi>();
        foreach (var poi in _allPois)
        {
            double distance = Location.CalculateDistance(
                userLat, userLng,
                poi.Lat, poi.Lng,
                DistanceUnits.Kilometers);

            poi.Distance = $"{distance:F2} km";
            
            // Chỉ lấy POI có khoảng cách dưới 2km
            if (distance <= 2)
            {
                updatedPois.Add(poi);
            }
        }

        // Sắp xếp theo khoảng cách tăng dần
        updatedPois = updatedPois.OrderBy(p =>
        {
            if (string.IsNullOrEmpty(p.Distance))
                return double.MaxValue;
            return double.Parse(p.Distance.Replace(" km", ""));
        }).ToList();

        // Update UI
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Pois.Clear();
            foreach (var p in updatedPois)
                Pois.Add(p);
        });
    }

  private async void OnDetailClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Poi selectedPoi)
        {
            await Navigation.PushAsync(new PoiDetailPage(selectedPoi));
        }
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
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (_locationService != null)
            _locationService.LocationUpdated -= OnLocationUpdated;
    }
}