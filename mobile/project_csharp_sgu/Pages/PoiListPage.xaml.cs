using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using project_csharp_sgu.Models;
using project_csharp_sgu.Services;

namespace project_csharp_sgu.Pages;

public partial class PoiListPage : ContentPage
{
    public ObservableCollection<Poi> Pois { get; set; } = new ObservableCollection<Poi>();

    private readonly IAudioService _audioService;

    public PoiListPage()
    {
        InitializeComponent();

        _audioService = Application.Current
            .Handler
            .MauiContext
            .Services
            .GetService<IAudioService>();

        PoiList.ItemsSource = Pois; // bind ngay từ đầu
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            Pois = await LoadPoisAsync(AppState.CurrentLanguage);
            PoiList.ItemsSource = Pois;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Không thể tải POI: {ex.Message}", "OK");
        }
    }

    public async Task<ObservableCollection<Poi>> LoadPoisAsync(string lang)
    {
        using var client = new HttpClient();
        string url = $"http://10.0.2.2:5188/api/poi?lang={lang}";

        try
        {
            var pois = await client.GetFromJsonAsync<List<Poi>>(url);
            if (pois == null) return new ObservableCollection<Poi>();
            return new ObservableCollection<Poi>(pois);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Lỗi kết nối API: {ex.Message}", "OK");
            return new ObservableCollection<Poi>();
        }
    }

    private async void OnDetailClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Poi selectedPoi)
        {
            await Navigation.PushAsync(new PoiDetailPage(selectedPoi));
        }
    }
}