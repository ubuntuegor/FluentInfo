using System;
using ManagedCommon;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace FluentInfo;

public partial class App
{
    private readonly AppModel _model = AppModel.Instance;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var commandLineArgs = Environment.GetCommandLineArgs();
        if (commandLineArgs.Length > 1)
        {
            var path = commandLineArgs[1];
            _model.OpenFile(path);
        }

        var window = new MainWindow(_model);
        window.CenterOnScreen();
        window.Activate();
        WindowHelpers.BringToForeground(window.GetWindowHandle());
    }
}