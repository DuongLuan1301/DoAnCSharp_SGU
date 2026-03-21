using project_csharp_sgu.Pages;

namespace project_csharp_sgu;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}