using System.Text;
using System.Text.Json;
using project_csharp_sgu.Models;

namespace project_csharp_sgu.Services
{
    public class POIService //Quản lý dữ liệu POIs
    {
        //biến lưu danh sách POI
        public List<POI> POIs { get; private set; } = new();

        //Hàm mở file pois.json trong app package
        public async Task LoadPOIs()
        {
                using var stream = await FileSystem.OpenAppPackageFileAsync("pois.json");
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

                //Đọc nội dung JSON
                var json = await reader.ReadToEndAsync();
                //Chuyển định dạng JSON -> object
                POIs = JsonSerializer.Deserialize<List<POI>>(json) ?? new List<POI>();
        }
        //Công thức Haversine
        public double CalculateDistance(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            double R = 6371000;
            double distance_Latitude = (latitude2 - latitude1) * Math.PI / 180;
            double distance_Longitude = (longitude2 - longitude1) * Math.PI / 180;

            double a =
                Math.Sin(distance_Latitude / 2) * Math.Sin(distance_Latitude / 2) +
                Math.Cos(latitude1 * Math.PI / 180) *
                Math.Cos(latitude2 * Math.PI / 180) *
                Math.Sin(distance_Longitude / 2) * Math.Sin(distance_Longitude / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            //Output: meter (trả về khoảng cách giữa 2 tọa độ)
            return R * c;
        }
    }
}