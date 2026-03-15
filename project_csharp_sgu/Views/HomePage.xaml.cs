using Microsoft.Maui.Devices.Sensors;
using project_csharp_sgu.Services;

namespace project_csharp_sgu.Views;

public partial class HomePage : ContentPage
{
    //Biến lưu tọa độ lấy từ LocationService.cs
    private readonly LocationService _locationService;

    public HomePage(LocationService locationService)
    {
        InitializeComponent();

        _locationService = locationService;

        _locationService.StartTracking();

        StartUIUpdate();
    }

    //Mỗi 10s chạy một lần
    void StartUIUpdate()
    {
        Dispatcher.StartTimer(TimeSpan.FromSeconds(10), () =>
        {
            _ = UpdateLocationUI();
            return true;
        });
    }

    //Cập nhật location cho homepage
    async Task UpdateLocationUI()
    {
        //Biến lưu location hiện tại 
        var location = _locationService.CurrentLocation;

        if (location == null)
            return;

        //Lưu tọa độ
        var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);

        var place = placemarks?.FirstOrDefault();

        if (place != null)
        {
            string street = place.Thoroughfare;
            locationNameLabel.IsVisible = false;
            locationDetailLabel.Text =
                $"Latitude: {location.Latitude}\n" +
                $"Longitude: {location.Longitude}\n" +
                $"{(string.IsNullOrEmpty(street) ? "" : street + "\n")}" +
                $"{place.SubAdminArea}, {place.AdminArea}";
        }

        lastUpdateLabel.Text =
            $"Update time: {DateTime.Now:HH:mm:ss}";
    }
}