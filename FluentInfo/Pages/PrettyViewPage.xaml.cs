using FluentInfo.Controls.PrettyView;
using FluentInfo.Data;
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

            var sections = MediaInfoTextParser.Parse(info);

            foreach (var section in sections)
            {
                if (section.Title != null)
                {
                    children.Add(new TitleControl(section.Type, section.Title, section.SubTitle, section.Chips));
                }

                children.Add(new SectionControl(section.Properties));
            }

            base.OnNavigatedTo(e);
        }
    }
}
