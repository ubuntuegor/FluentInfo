using FluentInfo.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace FluentInfo;

internal static class Converters
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

    public static Type SelectedViewToPage(SelectedView selectedView)
    {
        return selectedView switch
        {
            SelectedView.TEXT_VIEW => typeof(TextViewPage),
            SelectedView.PRETTY_VIEW => typeof(PrettyViewPage),
            _ => typeof(FailedPage)
        };
    }
}
