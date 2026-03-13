namespace project_csharp_sgu.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        GetLocation();
    }
    private async void GetLocation()
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
                latLabel.Text = $"Latitude: {location.Latitude}";
                lngLabel.Text = $"Longitude: {location.Longitude}";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }
}