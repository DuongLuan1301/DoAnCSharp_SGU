using System.Net.Http.Json;
using System.Text.Json;
using project_csharp_sgu.Models;

namespace project_csharp_sgu.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    
    // CẬP NHẬT: Đã thay localhost bằng IP 192.168.100.75 của Tài
    private const string BaseUrl = "http://192.168.100.75:5188/api/poi";

    public ApiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<Poi?> GetPoiByIdFromApiAsync(string id, string lang = "vi")
    {
        try
        {
            // URL chuẩn: http://192.168.100.75:5188/api/poi/SGU_01?lang=vi
            string url = $"{BaseUrl}/{id}?lang={lang}";

            // Dùng cấu hình CaseInsensitive để không bị lỗi chữ hoa/thường giữa C# và JSON
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            var response = await _httpClient.GetFromJsonAsync<Poi>(url, options);
            return response;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"---> Lỗi gọi API tại {id}: {ex.Message}");
            return null;
        }
    }
}