using System.Collections.ObjectModel;
using project_csharp_sgu.Models;

namespace project_csharp_sgu.Pages;

public partial class PoiListPage : ContentPage
{
    // Danh sách các hàng quán để hiển thị lên giao diện
    public ObservableCollection<Poi> Pois { get; set; }

    public PoiListPage()
    {
        InitializeComponent();

        // Khởi tạo dữ liệu mẫu cho demo thuyết minh tự động
        Pois = new ObservableCollection<Poi>
        {
            new Poi 
            { 
                Id = "POI_1",
                Name = "Bún bò Huế", 
                Distance = "0.5 km", 
                Address = "123 Nguyễn Huệ, Q1", 
                Description = "Món bún bò truyền thống với hương vị đậm đà, là đặc sản nổi tiếng của miền Trung Việt Nam." 
            },
            new Poi 
            { 
                Id = "POI_2",
                Name = "Cà phê sữa đá", 
                Distance = "0.2 km", 
                Address = "45 Lê Lợi, Q1", 
                Description = "Trải nghiệm văn hóa cà phê đường phố đặc trưng của Sài Gòn với vị đắng của cà phê và vị ngọt của sữa." 
            }
        };

        PoiList.ItemsSource = Pois;
    }

   
    private async void OnDetailClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Poi selectedPoi)
        {
            // Điều hướng xem thông tin (không tự động phát âm thanh ngay)
            await Navigation.PushAsync(new PoiDetailPage(selectedPoi, false));
        }
    }
}