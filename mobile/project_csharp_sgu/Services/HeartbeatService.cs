using System.Net.Http;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace project_csharp_sgu.Services
{
    public static class HeartbeatService
    {
        private static bool _isRunning = false;
        // Tạo 1 ID duy nhất cho thiết bị dùng chung cho toàn bộ App
        private static string _deviceId = Guid.NewGuid().ToString();

        public static void StartHeartbeat()
        {
            // 🔥 Chốt chặn an toàn: Nếu timer đang chạy rồi thì KHÔNG tạo thêm nữa
            if (_isRunning) return; 
            _isRunning = true;

            _ = PingServerAsync(); // Bắn tín hiệu ngay lần đầu gọi

            // Tạo bộ đếm 5 giây chạy ngầm xuyên suốt các trang
            Application.Current.Dispatcher.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                _ = PingServerAsync();
                return true; // Luôn trả về true để timer chạy mãi mãi chừng nào app còn mở
            });
        }

        private static async Task PingServerAsync()
        {
            try 
            {
                using var client = new HttpClient();
                string url = $"{Constants.BaseApiUrl}/api/tracking/ping?deviceId={_deviceId}";
                // Gửi gói dữ liệu rỗng chuẩn JSON như đã thống nhất
                var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");
                await client.PostAsync(url, content);
            } 
            catch 
            { 
                // Rớt mạng thì bỏ qua, không làm crash app
            }
        }
    }
}