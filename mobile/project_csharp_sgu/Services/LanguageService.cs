using Microsoft.Maui.Storage;

namespace project_csharp_sgu.Services;

public class LanguageService : ILanguageService
{
    private const string LANG_KEY = "";

    public void SetLanguage(string langCode)
    {
        Preferences.Set(LANG_KEY, langCode);
    }

    public string GetLanguage()
    {
        //CurrentLanguage nếu chưa có thì gán mặc định là "en"
        return Preferences.Get(LANG_KEY, "en");
    }
}