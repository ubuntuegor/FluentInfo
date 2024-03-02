using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentInfo.Controls.PrettyView
{
    public sealed partial class SectionControl : UserControl
    {
        public SectionControl(List<(string, string)> items)
        {
            this.InitializeComponent();

            foreach (var (field, value) in items)
            {
                var paragraph = new Paragraph
                {
                    Margin = new Thickness(0, 10, 0, 0)
                };
                paragraph.Inlines.Add(new Run { Text = field + ":", FontWeight = FontWeights.SemiBold });
                paragraph.Inlines.Add(new Run { Text = " " + value });
                textBlock.Blocks.Add(paragraph);
            }
        }
    }
}
