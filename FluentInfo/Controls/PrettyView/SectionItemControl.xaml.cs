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
    public sealed partial class SectionItemControl : UserControl
    {
        public SectionItemControl()
        {
            this.InitializeComponent();
        }

        public string Field
        {
            get { return (string)GetValue(FieldProperty); }
            set { SetValue(FieldProperty, value); }
        }

        public static readonly DependencyProperty FieldProperty =
            DependencyProperty.Register("Field", typeof(string), typeof(SectionItemControl), new PropertyMetadata(null));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(SectionItemControl), new PropertyMetadata(null));
    }
}
