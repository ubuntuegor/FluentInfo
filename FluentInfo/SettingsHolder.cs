using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FluentInfo;

internal enum SelectedView
{
    TEXT_VIEW,
    PRETTY_VIEW
}

internal partial class SettingsHolder : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged = delegate { };

    private readonly Windows.Storage.ApplicationDataContainer localSettings =
        Windows.Storage.ApplicationData.Current.LocalSettings;

    private const string TextWrapEnabledProperty = "textWrapEnabledProperty";
    private const string SelectedViewProperty = "selectedViewProperty";
    private const string WindowWidthProperty = "windowWidthProperty";
    private const string WindowHeightProperty = "windowHeightProperty";

    private SettingsHolder()
    {
    }

    public bool TextWrapEnabled
    {
        get => localSettings.Values[TextWrapEnabledProperty] as bool? ?? false;
        set
        {
            localSettings.Values[TextWrapEnabledProperty] = value;
            OnPropertyChanged();
        }
    }

    public SelectedView SelectedView
    {
        get =>
            localSettings.Values[SelectedViewProperty] is int selectedView
                ? (SelectedView)selectedView
                : SelectedView.PRETTY_VIEW;
        set
        {
            localSettings.Values[SelectedViewProperty] = (int)value;
            OnPropertyChanged();
        }
    }

    public double WindowWidth
    {
        get => localSettings.Values[WindowWidthProperty] as double? ?? 600;
        set
        {
            localSettings.Values[WindowWidthProperty] = value;
            OnPropertyChanged();
        }
    }

    public double WindowHeight
    {
        get => localSettings.Values[WindowHeightProperty] as double? ?? 600;
        set
        {
            localSettings.Values[WindowHeightProperty] = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged!.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static readonly Lazy<SettingsHolder> LazyInstance = new(() => new SettingsHolder());
    public static SettingsHolder Instance => LazyInstance.Value;
}
