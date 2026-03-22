using Microsoft.Maui.Storage;

namespace project_csharp_sgu.Services;

public class LanguageService : ILanguageService
{
    private const string LANG_KEY = "app_language";

    public void SetLanguage(string langCode)
    {
        Preferences.Set(LANG_KEY, langCode);
    }

    public string GetLanguage()
    {
        return Preferences.Get(LANG_KEY, "en"); // default English
    }
}