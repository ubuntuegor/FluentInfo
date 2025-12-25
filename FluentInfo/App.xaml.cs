using System;
using ManagedCommon;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace FluentInfo;

public partial class App
{
    private Window _mainWindow = null!;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var cmdargs = Environment.GetCommandLineArgs();
        _mainWindow = new MainWindow(cmdargs);
        _mainWindow.CenterOnScreen();
        _mainWindow.Activate();
        WindowHelpers.BringToForeground(_mainWindow.GetWindowHandle());
    }
}