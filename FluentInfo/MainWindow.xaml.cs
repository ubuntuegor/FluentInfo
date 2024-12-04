using FluentInfo.Pages;
using MediaInfoLib;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.IO;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinUIEx;

namespace FluentInfo;

public sealed partial class MainWindow
{
    private readonly string appName = (Application.Current.Resources["AppTitleName"] as string)!;
    private readonly MediaInfo mediaInfo = new();
    private readonly SettingsHolder settings = SettingsHolder.Instance;
    private bool fileOpened;

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
        ReactToSettings();

        SizeChanged += MainWindow_SizeChanged;
    }

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        settings.WindowWidth = args.Size.Width;
        settings.WindowHeight = args.Size.Height;
    }

    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        ReactToSettings();

        if (e.PropertyName == nameof(settings.SelectedView))
        {
            RefreshPage();
        }
    }

    private void ReactToSettings()
    {
        WrapTextOptionButton.IsEnabled = settings.SelectedView == SelectedView.TEXT_VIEW;
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
        if (!fileOpened) return;

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
            fileOpened = true;
            RefreshPage();
        }
        else
        {
            fileOpened = false;
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
}
