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
            //Gọi hàm LoadPOIs() trong PoiService.cs
            await _poiService.LoadPOIs();

        //Lưu tọa độ hiện tại lấy trong LocationService.cs
        var current = _locationService.CurrentLocation;

        //lấy tọa độ từ User
        double userLat = current.Latitude;
        double userLong = current.Longitude;
        
        //Biến lưu danh sách POIs hợp lệ (nằm trong bán kính 1km của User)
        var nearby = new List<POI>();

        //Duyệt từng POIs trong danh sách POIs
        foreach (var poi in _poiService.POIs)
        {
            //Tính khoảng cách từ User đến một POI
            double dist = _poiService.CalculateDistance(userLat, userLong, poi.Latitude, poi.Longitude);

            //Nếu khoảng cách nhỏ hơn 1000(m) ~ 1(km)
            if(dist < 1000)
            {
                poi.Distance = dist;
                poi.DistanceText = $"{dist:F0} m";
                nearby.Add(poi);
            }
            //Biến đổi đơn vị giữa m và km
            //poi.DistanceText = dist < 1000 ? $"{dist:F0} m" : $"{dist / 1000:F1} km";
        }

            // sort các POI ở gần nhất -> xa nhất
            nearby = nearby.OrderBy(p => p.Distance).ToList();

        nearbyCollection.ItemsSource = null;
        nearbyCollection.ItemsSource = nearby;
    }

    private async void OnListenClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.BindingContext is not POI poi) return;

        await Navigation.PushAsync(new POIDetailPage(poi));
    }
}