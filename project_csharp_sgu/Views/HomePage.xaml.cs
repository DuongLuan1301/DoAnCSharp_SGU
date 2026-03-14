using Microsoft.Maui.Devices.Sensors;

namespace project_csharp_sgu.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        StartLocationTracking();
    }

    private void StartLocationTracking()
    {
        Dispatcher.StartTimer(TimeSpan.FromSeconds(10), () =>
        {
            _ = GetLocation();
            return true;
            // true = timer tiếp tục chạy
        });
    }

    private async Task GetLocation()
    {
        try
        {
            var location = await Geolocation.GetLocationAsync(
                new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.High,
                    Timeout = TimeSpan.FromSeconds(10)
                });

            if (location != null)
            {
                var placemarks = await Geocoding.GetPlacemarksAsync(
                    location.Latitude,
                    location.Longitude);

                var place = placemarks?.FirstOrDefault();

                if (place != null)
                {
                    locationNameLabel.Text = place.FeatureName ?? "Current location";
                    locationDetailLabel.Text =
                        $"{place.SubAdminArea}, {place.AdminArea}";
                }
                lastUpdateLabel.Text = $"Update time: {DateTime.Now:HH:mm:ss}";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}