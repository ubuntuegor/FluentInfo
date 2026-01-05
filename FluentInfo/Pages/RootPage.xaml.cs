using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage.Pickers;

namespace FluentInfo.Pages;

public sealed partial class RootPage
{
    private readonly SettingsHolder _settings = SettingsHolder.Instance;
    private AppModel _model = null!;

    public RootPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _model = (e.Parameter as AppModel)!;

        RefreshPage();

        _model.PropertyChanged += ModelOnPropertyChanged;
        _settings.PropertyChanged += Settings_PropertyChanged;

        base.OnNavigatedTo(e);
    }

    private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_model.InfoText)) RefreshPage();
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settings.SelectedView)) RefreshPage();
    }

    private async void OpenFilePicker(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker(XamlRoot.ContentIslandEnvironment.AppWindowId);

        picker.FileTypeFilter.Add("*");
        picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
        var file = await picker.PickSingleFileAsync();

        if (file != null) _model.OpenFile(file.Path);
    }

    private void RefreshPage()
    {
        if (_model.CurrentFilePath == null)
        {
            NavigationFrame.Navigate(typeof(NoFileOpenPage));
            return;
        }

        if (_model.InfoText == null)
        {
            NavigationFrame.Navigate(typeof(FailedPage), _model, new EntranceNavigationTransitionInfo());
            return;
        }

        var selectedPage = Converters.SelectedViewToPage(_settings.SelectedView);
        NavigationFrame.Navigate(selectedPage, _model, new EntranceNavigationTransitionInfo());
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

        _model.OpenFile(storageFile.Path);
    }

    private async void OpenAboutWindow(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = (Application.Current.Resources["AppTitleName"] as string)!,
            Content = new AboutContentPage(_model),
            CloseButtonText = "Close",
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style
        };

        await dialog.ShowAsync();
    }

    private void SelectPrettyView(object sender, RoutedEventArgs e)
    {
        _settings.SelectedView = ViewOption.PrettyView;
    }

    private void SelectTextView(object sender, RoutedEventArgs e)
    {
        _settings.SelectedView = ViewOption.TextView;
    }

    private async void CopyText(object sender, RoutedEventArgs e)
    {
        var package = new DataPackage();
        package.SetText(_model.InfoText);
        Clipboard.SetContent(package);

        if (!PopupInfoBar.IsOpen)
        {
            PopupInfoBar.IsOpen = true;
            await Task.Delay(TimeSpan.FromSeconds(3));
            PopupInfoBar.IsOpen = false;
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _model.PropertyChanged -= ModelOnPropertyChanged;
        _settings.PropertyChanged -= Settings_PropertyChanged;

        base.OnNavigatedFrom(e);
    }
}