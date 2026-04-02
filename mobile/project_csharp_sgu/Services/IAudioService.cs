using project_csharp_sgu.Models;

namespace project_csharp_sgu.Services;

public interface IAudioService
{
    bool IsPlaying { get; }

    Task PlayAsync(Poi poi, string lang);

    void Stop();
}