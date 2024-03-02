using FluentInfo.Controls.PrettyView;
using MediaInfoLib;
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentInfo.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PrettyViewPage : Page
    {
        private const string Separator = " : ";

        public PrettyViewPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MediaInfo mediaInfo = e.Parameter as MediaInfo;
            mediaInfo.Option("Inform", "Text");
            string info = mediaInfo.Inform();

            var children = content.Children;
            children.Clear();

            var lines = Utils.SplitToLines(info, StringSplitOptions.RemoveEmptyEntries);

            var sectionItems = new List<(string, string)>();

            foreach (string line in lines)
            {
                var index = line.IndexOf(Separator);

                if (index == -1)
                {
                    // current line is a title
                    if (sectionItems.Count > 0)
                    {
                        children.Add(new SectionControl(sectionItems));
                        sectionItems = [];
                    }

                    var title = line.Trim();
                    children.Add(new TitleControl { Title = title });
                }
                else
                {
                    // current line is a field
                    var name = line[..index].Trim();
                    var value = line[(index + Separator.Length)..].Trim();

                    sectionItems.Add((name, value));
                }
            }

            if (sectionItems.Count > 0)
            {
                children.Add(new SectionControl(sectionItems));
            }

            base.OnNavigatedTo(e);
        }
    }
}
