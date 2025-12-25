using System;
using Windows.System;
using MediaInfoLib;
using Microsoft.UI.Xaml;

namespace FluentInfo.Pages;

public sealed partial class AboutContentPage
{
    public AboutContentPage(MediaInfo mediaInfo)
    {
        InitializeComponent();
        var version = mediaInfo.Option("info_version", string.Empty) ?? "[unknown version]";
        var index = version.IndexOf(" - v", StringComparison.Ordinal);
        if (index != -1) version = version[(index + 3)..];

        VersionText.Text = version;
    }

    private async void SourceCode_Click(object sender, RoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri("https://github.com/ubuntuegor/FluentInfo"));
    }
}