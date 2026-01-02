using FluentInfo.Controls.PrettyView;
using FluentInfoCommon;
using Microsoft.UI.Xaml.Navigation;

namespace FluentInfo.Pages;

public sealed partial class PrettyViewPage
{
    public PrettyViewPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var model = (e.Parameter as AppModel)!;
        var info = model.InfoText!;

        var children = ContentStaggeredPanel.Children;
        children.Clear();

        var sections = MediaInfoTextParser.Parse(info);

        foreach (var section in sections)
        {
            var title = section.Title != null
                ? new TitleControl(section.Type, section.Title, section.SubTitle, section.Chips)
                : null;
            children.Add(new SectionControl(title, section.Properties));
        }

        base.OnNavigatedTo(e);
    }
}