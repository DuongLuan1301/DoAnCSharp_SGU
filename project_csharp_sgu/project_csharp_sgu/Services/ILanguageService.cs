namespace project_csharp_sgu.Services;

public interface ILanguageService
{
    void SetLanguage(string langCode);
    string GetLanguage();
}