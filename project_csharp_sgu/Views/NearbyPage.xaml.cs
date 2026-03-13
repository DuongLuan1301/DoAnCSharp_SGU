namespace project_csharp_sgu.Views;
using project_csharp_sgu.Models;

public partial class NearbyPage : ContentPage
{
    public NearbyPage()
    {
        InitializeComponent();

        var pois = new List<Poi>
        {
            new Poi { Name="Bún bò Huế", Distance="12m"},
            new Poi { Name="Bánh xèo", Distance="18m"},
            new Poi { Name="Phở bò", Distance="25m"}
        };

        nearbyCollection.ItemsSource = pois;
    }
}