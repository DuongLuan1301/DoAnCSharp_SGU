using Plugin.Maui.Audio;

namespace project_csharp_sgu.Services
{
    public class AudioService
    {
        private readonly IAudioManager _audioManager;
        private IAudioPlayer? _player;

        public AudioService()
        {
            _audioManager = AudioManager.Current;
        }

        public async Task Play(string audioFileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(audioFileName))
                {
                    Console.WriteLine("Audio filename is empty!");
                    return;
                }

                // Stop player cũ nếu có
                _player?.Stop();
                _player?.Dispose();
                _player = null;

                // Lấy tên file (không đường dẫn)
                string fileNameOnly = Path.GetFileName(audioFileName);

                Console.WriteLine(">>> Loading audio from app package: " + fileNameOnly);

                // Load file từ Resources/Raw
                Stream stream = await FileSystem.OpenAppPackageFileAsync(fileNameOnly);

                // Tạo player
                _player = _audioManager.CreatePlayer(stream);

                if (_player == null)
                {
                    Console.WriteLine("Cannot create audio player!");
                    return;
                }

                Console.WriteLine(">>> Playing audio: " + fileNameOnly);
                _player.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> AUDIO ERROR: " + ex.Message);
            }
        }
    }
}
