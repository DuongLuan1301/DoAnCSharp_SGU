using Microsoft.Maui.Devices.Sensors;

#nullable enable

namespace project_csharp_sgu.Services;

public class LocationService
{
    // vòng đời của timer (dừng / chạy)
    private CancellationTokenSource? _cts;

    // vị trí hiện tại của user
    public Location? CurrentLocation { get; private set; }

    // vị trí "root" để so sánh DistanceFromAnrchor (anchor)
    public Location? AnchorLocation { get; private set; }

    // khoảng cách từ CurrentLocation → AnchorLocation (km)
    public double DistanceFromAnchor { get; private set; }

    // event để notify cho các page khi có GPS mới
    public event Action<LocationService>? LocationUpdated;

    public LocationService()
    {
        _cts = null;
        CurrentLocation = null;
        AnchorLocation = null;
        LocationUpdated = null;
    }

    // 1. BẮT ĐẦU GPS (GLOBAL)
    public void Start()
    {
        // nếu timer đã chạy rồi → không chạy lại (tránh duplicate)
        if (_cts != null && !_cts.IsCancellationRequested)
            return;

        // tạo token mới để điều khiển timer
        _cts = new CancellationTokenSource();

        // tạo timer chạy mỗi 10 giây
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher != null)
        {
            dispatcher.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                // gọi async GPS (không await để tránh block UI)
                _ = UpdateLocationAsync();

                // nếu chưa bị cancel → tiếp tục timer
                // nếu bị cancel → dừng timer
                return !_cts.IsCancellationRequested;
            });
        }
    }

    // 2. LẤY GPS + TÍNH KHOẢNG CÁCH
    private async Task UpdateLocationAsync()
    {
        //lấy location hiện tại
        var location = await Geolocation.Default.GetLocationAsync(
            new GeolocationRequest(GeolocationAccuracy.High));

        // nếu location đã có → kiểm tra khoảng cách
        if (CurrentLocation != null && location != null)
        {
            //tính khoảng cách: movedDistance = Distance(anchorLocation, currentLocation)
            double movedDistance = Location.CalculateDistance(
                CurrentLocation,
                location,
                DistanceUnits.Kilometers);

            // nếu di chuyển < 50m → bỏ qua
            if (movedDistance < 0.05)
                return;
        }

        // nếu di chuyển > 50m -> cập nhật vị trí
        CurrentLocation = location;

        // anchor logic giữ nguyên (dùng cho 1km nếu cần)
        if (AnchorLocation == null && location != null)
        {
            AnchorLocation = location;
            DistanceFromAnchor = 0;
        }
        else if (location != null)
        {
            DistanceFromAnchor = Location.CalculateDistance(
                location,
                AnchorLocation!,
                DistanceUnits.Kilometers);
        }

        // chỉ gọi khi di chuyển đủ 50m 
        LocationUpdated?.Invoke(this);
    }

    // 3. RESET ANCHOR (SAU KHI GỌI API)
    public void ResetAnchor()
    {
        AnchorLocation = CurrentLocation;
        DistanceFromAnchor = 0;
    }
}