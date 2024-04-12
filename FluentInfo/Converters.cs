using FluentInfo.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInfo
{
    class Converters
    {
        public static TextWrapping BooleanToWrapping(bool value)
        {
            return value ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }

        public static ScrollMode BooleanToScrollMode(bool value)
        {
            return value ? ScrollMode.Disabled : ScrollMode.Auto;
        }

        public static ScrollBarVisibility BooleanToScrollBarVisibility(bool value)
        {
            return value ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
        }

        public static bool SelectedViewToBool(SelectedView selectedView, string name)
        {
            return selectedView == Enum.Parse<SelectedView>(name);
        }

        public static System.Type SelectedViewToPage(SelectedView selectedView)
        {
            return selectedView switch
            {
                SelectedView.TextView => typeof(TextViewPage),
                SelectedView.PrettyView => typeof(PrettyViewPage),
                _ => typeof(FailedPage),
            };
        }
    }
}
