using Android.App;
using Android.Runtime;

namespace project_csharp_sgu;
[Application(UsesCleartextTraffic = true)] // Thêm đoạn này
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
