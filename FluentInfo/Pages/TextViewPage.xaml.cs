using FluentInfoCommon;
using MediaInfoLib;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Navigation;

namespace FluentInfo.Pages;

public sealed partial class TextViewPage
{
    private readonly SettingsHolder settings = SettingsHolder.Instance;

    public TextViewPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var mediaInfo = (e.Parameter as MediaInfo)!;
        mediaInfo.Option("Inform", "Text");
        var info = mediaInfo.Inform()!;

        var inlines = InfoTextBlock.Inlines;
        inlines.Clear();

        var lines = Utils.SplitToLines(info);

        foreach (var line in lines)
        {
            inlines.Add(new Run { Text = line });
            inlines.Add(new LineBreak());
        }

        base.OnNavigatedTo(e);
    }
}
