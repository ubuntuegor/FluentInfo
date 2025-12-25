using System;
using Windows.Storage;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FluentInfo;

internal enum SelectedView
{
    TextView,
    PrettyView
}

internal partial class SettingsHolder : ObservableObject
{
    private const string TextWrapEnabledProperty = "textWrapEnabledProperty";
    private const string SelectedViewProperty = "selectedViewProperty";
    private const string WindowWidthProperty = "windowWidthProperty";
    private const string WindowHeightProperty = "windowHeightProperty";

    private static readonly Lazy<SettingsHolder> LazyInstance = new(() => new SettingsHolder());

    private readonly ApplicationDataContainer _localSettings =
        ApplicationData.Current.LocalSettings;

    [ObservableProperty] private SelectedView _selectedView = SelectedView.PrettyView;

    [ObservableProperty] private bool _textWrapEnabled;

    [ObservableProperty] private double _windowHeight = 600;

    [ObservableProperty] private double _windowWidth = 600;

    private SettingsHolder()
    {
        if (_localSettings.Values[SelectedViewProperty] is int selectedView) _selectedView = (SelectedView)selectedView;
        if (_localSettings.Values[TextWrapEnabledProperty] is bool textWrapEnabled) _textWrapEnabled = textWrapEnabled;
        if (_localSettings.Values[WindowHeightProperty] is double windowHeight) _windowHeight = windowHeight;
        if (_localSettings.Values[WindowWidthProperty] is double windowWidth) _windowWidth = windowWidth;
    }

    public static SettingsHolder Instance => LazyInstance.Value;

    partial void OnSelectedViewChanged(SelectedView value)
    {
        _localSettings.Values[SelectedViewProperty] = (int)value;
    }

    partial void OnTextWrapEnabledChanged(bool value)
    {
        _localSettings.Values[TextWrapEnabledProperty] = value;
    }

    partial void OnWindowWidthChanged(double value)
    {
        _localSettings.Values[WindowWidthProperty] = value;
    }

    partial void OnWindowHeightChanged(double value)
    {
        _localSettings.Values[WindowHeightProperty] = value;
    }
}