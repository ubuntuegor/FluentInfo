using FluentInfo.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentInfo.Controls.PrettyView
{
    public sealed partial class TitleControl : UserControl
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
                _ => "",
            };
        }

        public TitleControl(SectionType type, string title, string subtitle, List<string> chips)
        {
            this.InitializeComponent();
            titleText.Text = title;

            if (type == SectionType.OTHER)
            {
                typeIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                typeIcon.Glyph = SectionTypeToGlyph(type);
            }

            if (subtitle != null)
            {
                subtitleText.Text = subtitle;
            }
            else
            {
                subtitleText.Visibility = Visibility.Collapsed;
            }

            foreach (var chip in chips)
            {
                chipsPanel.Children.Add(new ChipControl(chip));
            }
        }
    }
}
