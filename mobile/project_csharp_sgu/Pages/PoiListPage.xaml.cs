using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using project_csharp_sgu.Models;

namespace project_csharp_sgu.Pages;

public partial class PoiListPage : ContentPage
{
    // Sử dụng PascalCase cho thuộc tính Pois theo chuẩn C#
    public ObservableCollection<Poi> Pois { get; set; } = new ObservableCollection<Poi>();

    public PoiListPage()
    {
        InitializeComponent();
        PoiList.ItemsSource = Pois; 
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Lấy ngôn ngữ hiện tại từ AppState
            string lang = AppState.CurrentLanguage ?? "vi";
            var loadedPois = await LoadPoisAsync(lang);

            // Cập nhật danh sách một cách mượt mà
            Pois.Clear();
            foreach (var poi in loadedPois)
            {
                Pois.Add(poi);
            }
        }
        catch (Exception ex)
        {
            // Sử dụng DisplayAlert chuẩn (nếu máy vẫn báo obsolete, bạn có thể đổi thành DisplayAlertAsync)
            await DisplayAlert("Lỗi", $"Không thể tải danh sách địa điểm: {ex.Message}", "OK");
        }
    }

    public async Task<ObservableCollection<Poi>> LoadPoisAsync(string lang)
{
    using var client = new HttpClient();
    
    // THAY ĐỔI TẠI ĐÂY: 
    // Giả sử IP máy tính bạn là 192.168.1.15 và Port Backend là 5188
    string ipAddress = "192.168.1.15"; // <--- Thay địa chỉ IP của bạn vào đây
    string url = $"http://{ipAddress}:5188/api/poi?lang={lang}";

    try
    {
        var response = await client.GetFromJsonAsync<List<Poi>>(url);
        
        if (response == null) 
            return new ObservableCollection<Poi>();

        return new ObservableCollection<Poi>(response);
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
        return new ObservableCollection<Poi>();
    }
}

    private async void OnDetailClicked(object sender, EventArgs e)
    {
        try
        {
            // Kiểm tra BindingContext có đúng là đối tượng Poi không
            if (sender is Button button && button.BindingContext is Poi selectedPoi)
            {
                // SỬA LỖI TẠI ĐÂY: Truyền thêm tham số 'false'
                // false vì đây là bấm xem thủ công, không cần tự động phát âm thanh như khi quét QR
                await Navigation.PushAsync(new PoiDetailPage(selectedPoi, false));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi chuyển trang", ex.Message, "OK");
        }
    }
}