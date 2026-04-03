using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using project_csharp_sgu.Models;
using project_csharp_sgu.Services;

namespace project_csharp_sgu.Pages;

public partial class PoiListPage : ContentPage
{

    private readonly LocationService _locationService;

    //Biến lưu danh sách các Pois hợp lệ
    private List<Poi> _allPois = new();

    //Biến lưu toàn bộ Pois từ db
    public ObservableCollection<Poi> Pois { get; set; } = new ObservableCollection<Poi>();

    private readonly IAudioService _audioService;

    public PoiListPage()
    {
        InitializeComponent();

        _audioService = Application.Current
            .Handler
            .MauiContext
            .Services
            .GetService<IAudioService>();

        _locationService = Application.Current
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
            await DisplayAlert("Error", $"Không thể tải POI: {ex.Message}", "OK");
        }
    }

    public async Task<ObservableCollection<Poi>> LoadPoisAsync(string lang)
    {
        using var client = new HttpClient();
        string url = $"http://10.0.2.2:5188/api/poi?lang={lang}";

        try
        {
            var pois = await client.GetFromJsonAsync<List<Poi>>(url);
            if (pois == null) return new ObservableCollection<Poi>();
            return new ObservableCollection<Poi>(pois);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Lỗi kết nối API: {ex.Message}", "OK");
            return new ObservableCollection<Poi>();
        }
    }

    private void OnLocationUpdated(LocationService service)
    {
        if (service.CurrentLocation == null) return;

        var userLat = service.CurrentLocation.Latitude;
        var userLng = service.CurrentLocation.Longitude;

        var filtered = new List<Poi>();
        //Duyệt poi trong danh sách pois
        foreach (var poi in _allPois)
        {   //tính khoảng cách từ location hiện tại đến location của poi
            double distance = Location.CalculateDistance(
                userLat, userLng,
                poi.lat, poi.lng,
                DistanceUnits.Kilometers);

            // nếu khoảng cách <= 1km, thêm poi vào biến filtered
            if (distance <= 1)
            {
                poi.distance = $"{distance:F2} km";
                filtered.Add(poi);
            }
        }
        // filtered lưu các pois theo thứ tự tăng dần
        filtered = filtered.OrderBy(p =>
            double.Parse(p.distance.Replace(" km", ""))
        ).ToList();
        // update UI
        MainThread.BeginInvokeOnMainThread(() =>
        {
            //dọn danh sách cũ chứa toàn bộ pois
            Pois.Clear();
            //thêm poi hợp lệ vào danh sách
            foreach (var p in filtered)
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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (_locationService != null)
            _locationService.LocationUpdated -= OnLocationUpdated;
    }
}