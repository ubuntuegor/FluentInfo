using MediaInfoLib;
using Microsoft.UI.Xaml.Navigation;

namespace FluentInfo.Pages;

public sealed partial class FailedPage
{
    public FailedPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ErrorTextBlock.Text = e.Parameter is MediaInfo
            ? "Unknown view format selected"
            : $"Could not open file: {e.Parameter}";
        base.OnNavigatedTo(e);
    }
}