using System.ComponentModel;
using System.IO;
using FluentInfo.Pages;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace FluentInfo;

public sealed partial class MainWindow
{
    private readonly AppModel _model;
    private readonly SettingsHolder _settings = SettingsHolder.Instance;

    public MainWindow(AppModel model)
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        this.SetIcon(@"Assets\fluentinfo.ico");

        Width = _settings.WindowWidth;
        Height = _settings.WindowHeight;

        _model = model;
        UpdateTitle();
        RootFrame.Navigate(typeof(RootPage), _model);

        _model.PropertyChanged += ModelOnPropertyChanged;
        SizeChanged += MainWindow_SizeChanged;
    }

    private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_model.CurrentFilePath)) UpdateTitle();
    }

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        _settings.WindowWidth = args.Size.Width;
        _settings.WindowHeight = args.Size.Height;
    }

    private void UpdateTitle()
    {
        var appName = (Application.Current.Resources["AppTitleName"] as string)!;
        var title = appName;

        if (_model.CurrentFilePath != null)
        {
            var fileName = Path.GetFileName(_model.CurrentFilePath);
            title = fileName + " - " + appName;
        }

        Title = title;
        TitleTextBlock.Text = title;
    }

    ~MainWindow()
    {
        _model.PropertyChanged -= ModelOnPropertyChanged;
        SizeChanged -= MainWindow_SizeChanged;
    }
}