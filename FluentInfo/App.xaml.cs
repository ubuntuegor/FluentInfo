using ManagedCommon;
using Microsoft.UI.Xaml;
using System;
using WinUIEx;

namespace FluentInfo;

public partial class App
{
    private Window mWindow = null!;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var cmdargs = Environment.GetCommandLineArgs();
        mWindow = new MainWindow(cmdargs);
        mWindow.CenterOnScreen();
        mWindow.Activate();
        WindowHelpers.BringToForeground(mWindow.GetWindowHandle());
    }
}
