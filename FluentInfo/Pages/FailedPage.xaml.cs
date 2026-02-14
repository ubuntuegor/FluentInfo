using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.ApplicationModel.Resources;

namespace FluentInfo.Pages;

public sealed partial class FailedPage
{
    private readonly ResourceLoader _resourceLoader = new();

    public FailedPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var model = (e.Parameter as AppModel)!;
        ErrorTextBlock.Text = model.InfoText == null
            ? string.Format(_resourceLoader.GetString("CouldNotOpenFileErrorMessage"), model.CurrentFilePath)
            : _resourceLoader.GetString("UnknownViewFormatErrorMessage");
        base.OnNavigatedTo(e);
    }
}