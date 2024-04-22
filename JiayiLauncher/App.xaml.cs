
namespace JiayiLauncher;

// do not use this class for any init work, use the one in Platforms/Windows instead
public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new MainPage();
    }
    
    // ??????
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Title = "Jiayi Launcher";
        window.Width = 900;
        window.Height = 550;

        return window;
    }
}