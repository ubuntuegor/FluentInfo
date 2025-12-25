using FluentInfoCommon;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;

namespace FluentInfo.Controls.PrettyView;

public sealed partial class SectionControl
{
    public SectionControl(TitleControl? title, OrderedProperties items)
    {
        InitializeComponent();

        if (title != null) RootStackPanel.Children.Insert(0, title);

        foreach (var (field, value) in items.GetPairs())
        {
            var paragraph = new Paragraph
            {
                Margin = new Thickness(0, 10, 0, 0)
            };
            paragraph.Inlines.Add(new Run { Text = field + ":", FontWeight = FontWeights.SemiBold });
            paragraph.Inlines.Add(new Run { Text = " " + value });
            PropertiesTextBlock.Blocks.Add(paragraph);
        }
    }
}