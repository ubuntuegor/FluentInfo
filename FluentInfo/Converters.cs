using System;
using FluentInfo.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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

    public static Type SelectedViewToPage(SelectedView selectedView)
    {
        return selectedView switch
        {
            SelectedView.TextView => typeof(TextViewPage),
            SelectedView.PrettyView => typeof(PrettyViewPage),
            _ => typeof(FailedPage)
        };
    }
}