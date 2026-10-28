using System.ComponentModel;
using System.IO;
using FluentInfo.Pages;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace FluentInfo;

public sealed partial class MainWindow
{
    private readonly AppModel _model;

    public MainWindow(AppModel model)
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        this.SetIcon(@"Assets\fluentinfo.ico");

        var manager = WindowManager.Get(this);
        manager.PersistenceId = "MainWindowId";

        _model = model;
        UpdateTitle();
        RefreshRootPage();

        _model.PropertyChanged += ModelOnPropertyChanged;
    }

    private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_model.CurrentFilePath):
                UpdateTitle();
                break;
            case nameof(_model.Language):
                RefreshRootPage();
                break;
        }
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

    private void RefreshRootPage()
    {
        RootFrame.Navigate(typeof(RootPage), _model);
    }

    ~MainWindow()
    {
        _model.PropertyChanged -= ModelOnPropertyChanged;
    }
}