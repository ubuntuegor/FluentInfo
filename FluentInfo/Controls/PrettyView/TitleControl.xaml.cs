using System.Collections.Generic;
using FluentInfoCommon;
using Microsoft.UI.Xaml;

namespace FluentInfo.Controls.PrettyView;

public sealed partial class TitleControl
{
    public TitleControl(SectionType type, string title, string? subtitle, List<string> chips)
    {
        InitializeComponent();
        TitleTextBlock.Text = title;

        if (type == SectionType.Other)
            TypeIcon.Visibility = Visibility.Collapsed;
        else
            TypeIcon.Glyph = SectionTypeToGlyph(type);

        if (subtitle != null)
            SubtitleTextBlock.Text = subtitle;
        else
            SubtitleTextBlock.Visibility = Visibility.Collapsed;

        foreach (var chip in chips) ChipsWrapPanel.Children.Add(new ChipControl(chip));
    }

    private static string SectionTypeToGlyph(SectionType type)
    {
        return type switch
        {
            SectionType.General => "\xE7C3",
            SectionType.Video => "\xE714",
            SectionType.Audio => "\xE8D6",
            SectionType.Text => "\xED1E",
            SectionType.Menu => "\xE8FD",
            SectionType.Image => "\uE8B9",
            _ => ""
        };
    }
}