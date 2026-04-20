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

        // Hiển thị trạng thái loading
        Pois.Clear();
        Pois.Add(new Poi { Name = "Đang tìm gian hàng gần..." });

        var pois = await LoadPoisAsync(AppState.CurrentLanguage);
        _allPois = pois.ToList();

        if (_locationService == null)
            return;

        // ❗ tránh subscribe 2 lần
        _locationService.LocationUpdated -= OnLocationUpdated;
        _locationService.LocationUpdated += OnLocationUpdated;

        _locationService.Start();

        // nếu đã có GPS sẵn → chạy luôn
        if (_locationService.CurrentLocation != null)
        {
            OnLocationUpdated(_locationService);
        }
    }

    public async Task<ObservableCollection<Poi>> LoadPoisAsync(string lang)
    {
        using var client = new HttpClient();

        // Tách Base URL ra để dùng chung
        string baseUrl = "http://192.168.31.34:5188";
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

        var updatedPois = new List<Poi>();

        foreach (var poi in _allPois)
        {
            double distance = Location.CalculateDistance(
                userLat, userLng,
                poi.Lat, poi.Lng,
                DistanceUnits.Kilometers);

            poi.DistanceValue = distance;

            if (distance < 1)
            {
                var meters = distance * 1000;
                poi.Distance = $"{meters:F0} m";
            }
            else
            {
                poi.Distance = $"{distance:F2} km";
            }

            if (distance <= 2)
            {
                updatedPois.Add(poi);
            }
        }

        // sort gần nhất
        updatedPois = updatedPois
            .OrderBy(p => p.DistanceValue)
            .ToList();

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
        try
        {
            using var client = new HttpClient();
            string url = $"http://10.0.2.2:5188/api/poi/{poiId}/{action}";
            var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");
            await client.PostAsync(url, content);
        }
        catch (Exception ex)
        {
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