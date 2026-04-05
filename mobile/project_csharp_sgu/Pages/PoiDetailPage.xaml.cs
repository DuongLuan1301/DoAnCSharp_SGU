using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using project_csharp_sgu.Models;
using project_csharp_sgu.Services;

namespace project_csharp_sgu.Pages;

public partial class PoiDetailPage : ContentPage
{
    private Poi _poi;
    private Poi selectedPoi;

    // private string _translatedDescription = null;

    //constructor
<<<<<<< Updated upstream
    public PoiDetailPage(Poi poi, bool v)
    {
        InitializeComponent();
=======
   public PoiDetailPage(Poi poi, bool isFromQr = false) // Mặc định là false nếu không truyền
{
    InitializeComponent();
>>>>>>> Stashed changes

    _poi = poi;

<<<<<<< Updated upstream
        // bind dữ liệu sang UI
        BindingContext = _poi;
    }
=======
    // Lấy AudioService (Tài nhớ kiểm tra đã đăng ký trong MauiProgram chưa nhé)
    _audioService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAudioService>();

    // Bind dữ liệu
    BindingContext = _poi;
}
>>>>>>> Stashed changes

    public PoiDetailPage(Poi selectedPoi)
    {
        this.selectedPoi = selectedPoi;
    }

    //lấy ngôn ngữ hiện tại và format sang định dạng khác để gọi voice phù hợp
    private static string GetSpeechLocale(string lang)
    {
        return lang switch
        {
            "en" => "en-US",
            "ja" => "ja-JP",
            "zh" => "zh-CN",
            _ => "en-US"
        };
    }

    // PLAY AUDIO (đọc description)
    private async void OnPlayAudioClicked(object sender, EventArgs e)
    {
        //kiểm tra poi tồn tại
        if (_poi == null || string.IsNullOrWhiteSpace(_poi.description))
            return;
        //lấy ngôn ngữ hiện tại trên app
        string lang = AppState.CurrentLanguage;

        //convert sang locale của TTS
        string localeCode = GetSpeechLocale(lang);

        // 🔥 lấy danh sách locale từ device
        var locales = await TextToSpeech.GetLocalesAsync();

        // 🔥 tìm locale phù hợp
        var selectedLocale = locales.FirstOrDefault(l =>
            l.Language.StartsWith(localeCode.Substring(0, 2))
        );

        // 🔥 fallback nếu không có
        selectedLocale ??= locales.FirstOrDefault();

        //đọc đúng ngôn ngữ được backend trả về
        await TextToSpeech.SpeakAsync(_poi.description, new SpeechOptions
        {
            Locale = selectedLocale
        });
    }

    // ĐÓNG POPUP
    private async void OnCloseClicked(object sender, EventArgs e)
    {
        _ = await Navigation.PopAsync();
    }
}