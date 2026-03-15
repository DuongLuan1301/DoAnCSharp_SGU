using project_csharp_sgu.Models;
using project_csharp_sgu.Services;

namespace project_csharp_sgu.Views;

public partial class NearbyPage : ContentPage
{
    private readonly POIService _poiService;
    private readonly LocationService _locationService;

    public NearbyPage(POIService poiService, LocationService locationService)
    {
        InitializeComponent();
        _poiService = poiService;
        _locationService = locationService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Load POI if empty
        if (_poiService.POIs == null || _poiService.POIs.Count == 0)
            await _poiService.LoadPOIs();

        Console.WriteLine(">>> NearbyPage POI Count = " + _poiService.POIs.Count);

        if (_poiService.POIs.Count == 0)
        {
            await DisplayAlert("Error", "Không có dữ liệu POI (pois.json)", "OK");
            return;
        }

        var current = await _locationService.GetCurrentLocation();
        if (current == null)
        {
            await DisplayAlert("Error", "Không lấy được vị trí GPS", "OK");
            return;
        }

        double userLat = current.Latitude;
        double userLng = current.Longitude;

        foreach (var poi in _poiService.POIs)
        {
            double dist = _poiService.CalculateDistance(userLat, userLng, poi.Latitude, poi.Longitude);

            poi.DistanceText = dist < 1000
                ? $"{dist:F0} m"
                : $"{dist / 1000:F1} km";
        }

        nearbyCollection.ItemsSource = null;
        nearbyCollection.ItemsSource = _poiService.POIs;
    }

    private async void OnListenClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.BindingContext is not POI poi) return;

        await Navigation.PushAsync(new POIDetailPage(poi));
    }
}