using project_csharp_sgu.Models;

namespace project_csharp_sgu.Services;

public class AudioService : IAudioService
{
    private CancellationTokenSource _cts;
    private bool _isPlaying;
    private Poi _currentPoi;

    public bool IsPlaying => _isPlaying;

    public async Task PlayAsync(Poi poi, string lang)
    {
        // 1. Nếu đang phát POI khác → dừng cái cũ
        if (_currentPoi != null && _currentPoi != poi)
        {
            Stop();
            await Task.Delay(150); // Nghỉ một chút để hệ thống giải phóng resource
        }

        // 2. Nếu cùng POI và đang phát → nhấn lần nữa là dừng
        if (_currentPoi == poi && _isPlaying)
        {
            Stop();
            return;
        }

        _cts = new CancellationTokenSource();
        _isPlaying = true;
        _currentPoi = poi;

        try
        {
            // Sửa logic chọn LocaleCode
            string localeCode = lang.ToLower() switch
            {
                "vi" => "vi-VN",
                "en" => "en-US",
                "ja" => "ja-JP",
                "zh" => "zh-CN",
                _ => "vi-VN"
            };

            var locales = await TextToSpeech.Default.GetLocalesAsync();
            var selectedLocale = locales.FirstOrDefault(l => 
                l.Language.ToLower().StartsWith(localeCode.Substring(0, 2))) 
                ?? locales.FirstOrDefault();

            await TextToSpeech.Default.SpeakAsync(poi.description, new SpeechOptions
            {
                Locale = selectedLocale,
                Pitch = 1.0f,
                Volume = 1.0f
            }, _cts.Token);
        }
        catch (OperationCanceledException) { /* Bỏ qua khi bị Cancel */ }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Audio Error: {ex.Message}"); }
        finally
        {
            _isPlaying = false;
            _currentPoi = null;
        }
    }

    public void Stop()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        _isPlaying = false;
        _currentPoi = null;
    }
}