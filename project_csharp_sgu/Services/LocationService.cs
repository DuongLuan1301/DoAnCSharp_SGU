namespace project_csharp_sgu.Services
{
    public class LocationService
    {
        public async Task<Location?> GetCurrentLocation()
        {
#if WINDOWS
            return new Location(10.785948, 106.800758);
#else
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best);
                return await Geolocation.GetLocationAsync(request);
            }
            catch
            {
                return null;
            }
#endif
        }
    }
}