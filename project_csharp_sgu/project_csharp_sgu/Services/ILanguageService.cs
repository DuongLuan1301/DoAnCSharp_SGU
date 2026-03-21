namespace project_csharp_sgu.Services;

/// <summary>
/// Interface quản lý ngôn ngữ của ứng dụng
/// </summary>
public interface ILanguageService
{
    /// <summary>
    /// Lấy ngôn ngữ đã lưu trước đó
    /// </summary>
    /// <returns>Mã ngôn ngữ (ví dụ: "en", "de", "zh", "ja")</returns>
    Task<string> GetSavedLanguageAsync();

    /// <summary>
    /// Lưu ngôn ngữ được chọn vào thiết bị
    /// </summary>
    /// <param name="languageCode">Mã ngôn ngữ cần lưu</param>
    Task SaveLanguageAsync(string languageCode);
}