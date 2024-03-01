using FluentInfo.Pages;
using MediaInfoLib;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentInfo
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WinUIEx.WindowEx
    {
        private readonly MediaInfo mediaInfo = new();
        private readonly SettingsHolder settings = SettingsHolder.Instance;
        private bool fileOpened = false;

        public MainWindow(string[] cmdargs)
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(appTitleBar);
            this.SetIcon("Assets/fluentinfo.ico");

            if (cmdargs.Length > 1)
            {
                var path = cmdargs[1];
                UpdateInfoForFile(path);
            }
            else
            {
                navigationFrame.Navigate(typeof(NoFileOpenPage));
            }

            settings.PropertyChanged += Settings_PropertyChanged;
            ReactToSettings();
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ReactToSettings();

            if (e.PropertyName == nameof(settings.SelectedView))
            {
                RefreshPage();
            }
        }

        private void ReactToSettings()
        {
            wrapTextOptionButton.IsEnabled = settings.SelectedView == SelectedView.TextView;
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
            navigationFrame.Navigate(selectedPage, mediaInfo, new EntranceNavigationTransitionInfo());
        }

        private void UpdateInfoForFile(string path)
        {
            var success = mediaInfo.Open(path);

            if (success)
            {
                fileOpened = true;
                RefreshPage();
            }
            else
            {
                fileOpened = false;
                navigationFrame.Navigate(typeof(FailedPage), path, new EntranceNavigationTransitionInfo());
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
            var appName = Application.Current.Resources["AppTitleName"] as string;

            var dialog = new ContentDialog()
            {
                XamlRoot = rootPanel.XamlRoot,
                Title = appName,
                Content = new AboutContentPage(),
                CloseButtonText = "Close",
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            };

            await dialog.ShowAsync();
        }

        ~MainWindow()
        {
            settings.PropertyChanged -= Settings_PropertyChanged;
        }

        private void SelectPrettyView(object sender, RoutedEventArgs e)
        {
            settings.SelectedView = SelectedView.PrettyView;
        }

        private void SelectTextView(object sender, RoutedEventArgs e)
        {
            settings.SelectedView = SelectedView.TextView;
        }
    }
}
