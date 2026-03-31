using project_csharp_sgu.Models; // Sử dụng đúng namespace của Model
using ZXing.Net.Maui;

namespace project_csharp_sgu.Pages;

public partial class QrPage : ContentPage
{
    public QrPage()
    {
        InitializeComponent();

        barcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
    }

    private void BarcodeReader_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        var firstResult = e.Results.FirstOrDefault();
        if (firstResult == null) return;

        Dispatcher.Dispatch(async () =>
        {
            string poiId = firstResult.Value;
            // FindPoiById trả về kiểu project_csharp_sgu.Models.Poi
            var poi = FindPoiById(poiId);

            if (poi != null)
            {
                // Bây giờ lệnh này sẽ chạy vì PoiDetailPage đã hiểu Poi là gì
                await Navigation.PushAsync(new PoiDetailPage(poi, true));
            }
        });
    }

    
    private void OnGenerateClicked(object sender, EventArgs e)
    {
        if (IdGeneratorEntry != null && !string.IsNullOrEmpty(IdGeneratorEntry.Text))
        {
            qrGenerator.Value = IdGeneratorEntry.Text.Trim();
        }
    }

    
    private Poi? FindPoiById(string id)
    {
        var mockData = new List<Poi>
        {
            new Poi { Id = "POI_1", Name = "Bún bò Huế", Description = "Bún bò Huế chuẩn vị truyền thống..." },
            new Poi { Id = "POI_2", Name = "Cà phê sữa đá", Description = "Hương vị cà phê đặc trưng Sài Gòn..." }
        };

        return mockData.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }
}