using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using project_csharp_sgu.Models;

namespace project_csharp_sgu.Pages;

public partial class PoiListPage : ContentPage
{
    public ObservableCollection<Poi> Pois { get; set; }

    public PoiListPage()
    {
        InitializeComponent();

        // Data mẫu
        Pois = new ObservableCollection<Poi>
        {
            new Poi
            {
                Name = "Bún bò Huế",
                Distance = "0.5 km",
                Address = "123 Nguyễn Huệ, Q1",
                Description = "Món ăn truyền thống Việt Nam, vị đậm đà."
            },
            new Poi
            {
                Name = "Cà phê sữa đá",
                Distance = "0.2 km",
                Address = "45 Lê Lợi, Q1",
                Description = "Thức uống nổi tiếng Việt Nam."
            },
            new Poi
            {
                Name = "Bánh mì",
                Distance = "0.8 km",
                Address = "78 Trần Hưng Đạo, Q1",
                Description = "Bánh mì giòn, nhân phong phú."
            }
        };

        PoiList.ItemsSource = Pois;
    }

    private async void OnDetailClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Poi selectedPoi)
        {
            await Navigation.PushAsync(new PoiDetailPage(selectedPoi));
        }
    }
}
