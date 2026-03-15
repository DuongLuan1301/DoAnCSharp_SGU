using Microsoft.Maui.Devices.Sensors;

namespace project_csharp_sgu.Services;

//Service xử lý GPS
public class LocationService
{
    //Biến lưu GPS (Location: class trong MAUI)
    public Location? CurrentLocation { get; private set; }

    //Tránh tạo nhiều timer cùng lúc
    bool _isTracking = false;

    //Hàm theo dõi, cập nhật GPS mỗi 10 giây
    public void StartTracking()
    {
        //Kiểm tra tracking đã được chạy chưa
        if (_isTracking)
            return;

        _isTracking = true;

        //Tạo timer chạy mỗi 10 giây
        Application.Current.Dispatcher.StartTimer(TimeSpan.FromSeconds(10), () =>
        {
            //Gọi UpdateLocation(), lấy GPS mới
            _ = UpdateLocation();
            return true;
        });
    }

    //Hàm lấy GPS
    private async Task UpdateLocation()
    {   
        //Biến lấy GPS
        var location = await Geolocation.GetLocationAsync(
            //Request GPS
            new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.High,
                Timeout = TimeSpan.FromSeconds(10) //Timeout 10s
            });
        //Lấy GPS thành công
        if (location != null)
        {   
            //Cập nhật Tọa độ hiện tại
            CurrentLocation = location;
        }

    }
}