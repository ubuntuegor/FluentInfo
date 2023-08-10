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

        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(appTitleBar);

            navigationFrame.Navigate(typeof(NoFileOpenPage));
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

        private void UpdateInfoForFile(string path)
        {
            var success = mediaInfo.Open(path);

            if (success)
            {
                string info = mediaInfo.Inform();
                navigationFrame.Navigate(typeof(TextViewPage), info, new EntranceNavigationTransitionInfo());
            }
            else
            {
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

        private async void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var appName = Application.Current.Resources["AppTitleName"];
            var appVersion = Application.Current.Resources["AppVersion"];

            var dialog = new ContentDialog()
            {
                XamlRoot = rootPanel.XamlRoot,
                Title = String.Format("{0} {1}", appName, appVersion),
                Content = new AboutContentPage(),
                CloseButtonText = "Close",
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            };

            await dialog.ShowAsync();
        }
    }
}
