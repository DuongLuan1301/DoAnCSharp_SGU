using System.Drawing;
using Microsoft.Maui.Media;
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
        //nếu đang phát POI khác → stop
        if (_currentPoi != null && _currentPoi != poi)
        {
            Stop();
            await Task.Delay(150);
        }

        //nếu cùng POI → toggle stop
        if (_currentPoi == poi && _isPlaying)
        {
            Stop();
            _currentPoi = null;
            return;
        }

        _cts = new CancellationTokenSource();
        _isPlaying = true;
        _currentPoi = poi;

        try
        {
            string localeCode = lang switch
            {
                "en" => "en-US",
                "ja" => "ja-JP",
                "zh" => "zh-CN",
                _ => "en-US"
            };

            var locales = await TextToSpeech.GetLocalesAsync();

            var selectedLocale = locales.FirstOrDefault(l =>
                l.Language.StartsWith(localeCode.Substring(0, 2))
            ) ?? locales.FirstOrDefault();

            await TextToSpeech.SpeakAsync(poi.description, new SpeechOptions
            {
                Locale = selectedLocale
            }, _cts.Token);
        }
        catch (OperationCanceledException)
        {
        }
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
            try
            {
                _cts.Cancel();
            }
            catch { }

            _cts.Dispose();
            _cts = null;
        }

        _isPlaying = false;
        _currentPoi = null;
    }
}