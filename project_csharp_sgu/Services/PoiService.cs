using System.Text;
using System.Text.Json;
using project_csharp_sgu.Models;

namespace project_csharp_sgu.Services
{
    public class POIService
    {
        public List<POI> POIs { get; private set; } = new();

        public async Task LoadPOIs()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("pois.json");
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

                var json = await reader.ReadToEndAsync();

                POIs = JsonSerializer.Deserialize<List<POI>>(json) ?? new List<POI>();

                Console.WriteLine(">>> POIService Loaded POIs = " + POIs.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> ERROR LOADING POIs: " + ex.Message);
                POIs = new List<POI>();
            }
        }

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371000;
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) *
                Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}