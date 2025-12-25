using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using FluentInfo.Pages;
using MediaInfoLib;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using WinRT.Interop;
using WinUIEx;

namespace FluentInfo;

public sealed partial class MainWindow : INotifyPropertyChanged
{
    private readonly string _appName = (Application.Current.Resources["AppTitleName"] as string)!;
    private readonly MediaInfo _mediaInfo = new();
    private readonly SettingsHolder _settings = SettingsHolder.Instance;

    private bool _isFileOpened;

    public MainWindow(string[] cmdargs)
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        this.SetIcon(@"Assets\fluentinfo.ico");
        SetTitle(_appName);

        Width = _settings.WindowWidth;
        Height = _settings.WindowHeight;

        if (cmdargs.Length > 1)
        {
            var path = cmdargs[1];
            UpdateInfoForFile(path);
        }
        else
        {
            NavigationFrame.Navigate(typeof(NoFileOpenPage));
        }

        _settings.PropertyChanged += Settings_PropertyChanged;
        SizeChanged += MainWindow_SizeChanged;
    }

    private bool IsFileOpened
    {
        get => _isFileOpened;
        set
        {
            if (_isFileOpened == value) return;
            _isFileOpened = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFileOpened)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        _settings.WindowWidth = args.Size.Width;
        _settings.WindowHeight = args.Size.Height;
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settings.SelectedView)) RefreshPage();
    }

    private async void OpenFilePicker(object sender, RoutedEventArgs e)
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, hwnd);

        picker.FileTypeFilter.Add("*");
        picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
        var file = await picker.PickSingleFileAsync();

        if (file != null) UpdateInfoForFile(file.Path);
    }

    private void RefreshPage()
    {
        if (!IsFileOpened) return;

        var selectedPage = Converters.SelectedViewToPage(_settings.SelectedView);
        NavigationFrame.Navigate(selectedPage, _mediaInfo, new EntranceNavigationTransitionInfo());
    }

    private void UpdateInfoForFile(string path)
    {
        var success = _mediaInfo.Open(path);
        var fileName = Path.GetFileName(path);

        SetTitle(fileName + " - " + _appName);

        if (success)
        {
            IsFileOpened = true;
            RefreshPage();
        }
        else
        {
            IsFileOpened = false;
            NavigationFrame.Navigate(typeof(FailedPage), path, new EntranceNavigationTransitionInfo());
        }
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Open";
        }
        else
        {
            e.AcceptedOperation = DataPackageOperation.None;
        }
    }

    private async void Window_Drop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;

        var items = await e.DataView.GetStorageItemsAsync();

        if (items.Count == 0) return;

        var storageFile = items[0];

        if (!storageFile.IsOfType(StorageItemTypes.File)) return;

        UpdateInfoForFile(storageFile.Path);
    }

    private async void OpenAboutWindow(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = RootPanel.XamlRoot,
            Title = _appName,
            Content = new AboutContentPage(_mediaInfo),
            CloseButtonText = "Close",
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style
        };

        await dialog.ShowAsync();
    }

    private void SetTitle(string title)
    {
        Title = title;
        TitleTextBlock.Text = title;
    }

    ~MainWindow()
    {
        _settings.PropertyChanged -= Settings_PropertyChanged;
        SizeChanged -= MainWindow_SizeChanged;
    }

    private void SelectPrettyView(object sender, RoutedEventArgs e)
    {
        _settings.SelectedView = SelectedView.PrettyView;
    }

    private void SelectTextView(object sender, RoutedEventArgs e)
    {
        _settings.SelectedView = SelectedView.TextView;
    }

    private async void CopyText(object sender, RoutedEventArgs e)
    {
        _mediaInfo.Option("Inform", "Text");
        var info = _mediaInfo.Inform()!;

        var package = new DataPackage();
        package.SetText(info);
        Clipboard.SetContent(package);

        if (!PopupInfoBar.IsOpen)
        {
            PopupInfoBar.IsOpen = true;
            await Task.Delay(TimeSpan.FromSeconds(3));
            PopupInfoBar.IsOpen = false;
        }
    }
}