namespace project_csharp_sgu.Services;

/// <summary>
/// Triển khai quản lý ngôn ngữ sử dụng Preferences (bộ nhớ thiết bị)
/// Mặc định luôn là tiếng Anh ("en")
/// </summary>
public class LanguageService : ILanguageService
{
    // Khóa để lưu ngôn ngữ vào Preferences
    private const string LanguageKey = "app_language";

    /// <summary>
    /// Lấy ngôn ngữ đã lưu trước đó
    /// Mặc định: "en" (tiếng Anh)
    /// </summary>
    public async Task<string> GetSavedLanguageAsync()
    {
        // Lấy ngôn ngữ từ Preferences
        // Nếu chưa bao giờ lưu, mặc định = "en" (tiếng Anh)
        var language = Preferences.Default.Get(LanguageKey, "en");
        
        // Trả về kết quả
        return await Task.FromResult(language);
    }

    /// <summary>
    /// Lưu ngôn ngữ được chọn từ LanguagePage
    /// </summary>
    public async Task SaveLanguageAsync(string languageCode)
    {
        // Lưu mã ngôn ngữ vào Preferences
        // Ví dụ: "en", "de", "zh", "ja"
        Preferences.Default.Set(LanguageKey, languageCode);
        
        // Hoàn thành task
        await Task.CompletedTask;
    }
}