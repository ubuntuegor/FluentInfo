using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentInfo
{
    class SettingsHolder : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private readonly Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private const string TEXT_WRAP_ENABLED_PROPERTY = "textWrapEnabledProperty";

        private bool textWrapEnabled = false;

        private SettingsHolder() {
            if (localSettings.Values[TEXT_WRAP_ENABLED_PROPERTY] is bool result)
            {
                textWrapEnabled = result;
            }
        }

        public bool TextWrapEnabled
        {
            get { return textWrapEnabled; }
            set
            {
                textWrapEnabled = value;
                localSettings.Values[TEXT_WRAP_ENABLED_PROPERTY] = value;
                OnPropertyChanged();
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static readonly Lazy<SettingsHolder> instance = new(() => new SettingsHolder());
        public static SettingsHolder Instance { get { return instance.Value; } }
    }
}
