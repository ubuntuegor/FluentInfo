using FluentInfo.Pages;
using MediaInfoLib;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinUIEx;

namespace FluentInfo;

public sealed partial class MainWindow : INotifyPropertyChanged
{
    private readonly string appName = (Application.Current.Resources["AppTitleName"] as string)!;
    private readonly MediaInfo mediaInfo = new();
    private readonly SettingsHolder settings = SettingsHolder.Instance;

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isFileOpened = false;
    private bool IsFileOpened
    {
        get => _isFileOpened;
        set
        {
            if (_isFileOpened != value)
            {
                _isFileOpened = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFileOpened)));
            }
        }
    }

    public MainWindow(string[] cmdargs)
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        this.SetIcon(@"Assets\fluentinfo.ico");
        SetTitle(appName);

        Width = settings.WindowWidth;
        Height = settings.WindowHeight;

        if (cmdargs.Length > 1)
        {
            var path = cmdargs[1];
            UpdateInfoForFile(path);
        }
        else
        {
            NavigationFrame.Navigate(typeof(NoFileOpenPage));
        }

        settings.PropertyChanged += Settings_PropertyChanged;
        SizeChanged += MainWindow_SizeChanged;
    }

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        settings.WindowWidth = args.Size.Width;
        settings.WindowHeight = args.Size.Height;
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(settings.SelectedView))
        {
            RefreshPage();
        }
    }

    private async void OpenFilePicker(object sender, RoutedEventArgs e)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var picker = new FileOpenPicker();
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        picker.FileTypeFilter.Add("*");
        picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
        var file = await picker.PickSingleFileAsync();

        if (file != null)
        {
            UpdateInfoForFile(file.Path);
        }
    }

    private void RefreshPage()
    {
        if (!IsFileOpened) return;

        var selectedPage = Converters.SelectedViewToPage(settings.SelectedView);
        NavigationFrame.Navigate(selectedPage, mediaInfo, new EntranceNavigationTransitionInfo());
    }

    private void UpdateInfoForFile(string path)
    {
        var success = mediaInfo.Open(path);
        var fileName = Path.GetFileName(path);

        SetTitle(fileName + " - " + appName);

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
        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            return;
        }

        var items = await e.DataView.GetStorageItemsAsync();

        if (items.Count == 0)
        {
            return;
        }

        var storageFile = items[0];

        if (!storageFile.IsOfType(StorageItemTypes.File))
        {
            return;
        }

        UpdateInfoForFile(storageFile.Path);
    }

    private async void OpenAboutWindow(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = RootPanel.XamlRoot,
            Title = appName,
            Content = new AboutContentPage(mediaInfo),
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
        settings.PropertyChanged -= Settings_PropertyChanged;
        SizeChanged -= MainWindow_SizeChanged;
    }

    private void SelectPrettyView(object sender, RoutedEventArgs e)
    {
        settings.SelectedView = SelectedView.PRETTY_VIEW;
    }

    private void SelectTextView(object sender, RoutedEventArgs e)
    {
        settings.SelectedView = SelectedView.TEXT_VIEW;
    }

    private async void CopyText(object sender, RoutedEventArgs e)
    {
        mediaInfo.Option("Inform", "Text");
        var info = mediaInfo.Inform()!;

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
