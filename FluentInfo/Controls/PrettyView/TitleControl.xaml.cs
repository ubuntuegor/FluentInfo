using FluentInfoCommon;
using Microsoft.UI.Xaml;
using System.Collections.Generic;

namespace FluentInfo.Controls.PrettyView;

public sealed partial class TitleControl
{
    private static string SectionTypeToGlyph(SectionType type)
    {
        return type switch
        {
            SectionType.GENERAL => "\xE7C3",
            SectionType.VIDEO => "\xE714",
            SectionType.AUDIO => "\xE8D6",
            SectionType.TEXT => "\xED1E",
            SectionType.MENU => "\xE8FD",
            SectionType.IMAGE => "\uE8B9",
            _ => ""
        };
    }

    public TitleControl(SectionType type, string title, string? subtitle, List<string> chips)
    {
        InitializeComponent();
        TitleTextBlock.Text = title;

        if (type == SectionType.OTHER)
        {
            TypeIcon.Visibility = Visibility.Collapsed;
        }
        else
        {
            TypeIcon.Glyph = SectionTypeToGlyph(type);
        }

        if (subtitle != null)
        {
            SubtitleTextBlock.Text = subtitle;
        }
        else
        {
            SubtitleTextBlock.Visibility = Visibility.Collapsed;
        }

        foreach (var chip in chips)
        {
            ChipsWrapPanel.Children.Add(new ChipControl(chip));
        }
    }
}
