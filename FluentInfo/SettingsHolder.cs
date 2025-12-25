using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace FluentInfo;

internal enum SelectedView
{
    TextView,
    PrettyView
}

internal partial class SettingsHolder : INotifyPropertyChanged
{
    private const string TextWrapEnabledProperty = "textWrapEnabledProperty";
    private const string SelectedViewProperty = "selectedViewProperty";
    private const string WindowWidthProperty = "windowWidthProperty";
    private const string WindowHeightProperty = "windowHeightProperty";

    private static readonly Lazy<SettingsHolder> LazyInstance = new(() => new SettingsHolder());

    private readonly ApplicationDataContainer _localSettings =
        ApplicationData.Current.LocalSettings;

    private SettingsHolder()
    {
    }

    public bool TextWrapEnabled
    {
        get => _localSettings.Values[TextWrapEnabledProperty] as bool? ?? false;
        set
        {
            _localSettings.Values[TextWrapEnabledProperty] = value;
            OnPropertyChanged();
        }
    }

    public SelectedView SelectedView
    {
        get =>
            _localSettings.Values[SelectedViewProperty] is int selectedView
                ? (SelectedView)selectedView
                : SelectedView.PrettyView;
        set
        {
            _localSettings.Values[SelectedViewProperty] = (int)value;
            OnPropertyChanged();
        }
    }

    public double WindowWidth
    {
        get => _localSettings.Values[WindowWidthProperty] as double? ?? 600;
        set
        {
            _localSettings.Values[WindowWidthProperty] = value;
            OnPropertyChanged();
        }
    }

    public double WindowHeight
    {
        get => _localSettings.Values[WindowHeightProperty] as double? ?? 600;
        set
        {
            _localSettings.Values[WindowHeightProperty] = value;
            OnPropertyChanged();
        }
    }

    public static SettingsHolder Instance => LazyInstance.Value;
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}