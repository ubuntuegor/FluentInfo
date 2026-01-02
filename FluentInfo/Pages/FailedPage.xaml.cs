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
        var model = (e.Parameter as AppModel)!;
        ErrorTextBlock.Text = model.InfoText == null
            ? $"Could not open file: {model.CurrentFilePath}"
            : "Unknown view format selected";
        base.OnNavigatedTo(e);
    }
}