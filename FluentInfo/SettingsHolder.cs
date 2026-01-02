using System;
using Windows.Storage;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FluentInfo;

internal enum ViewOption
{
    TextView,
    PrettyView
}

internal partial class SettingsHolder : ObservableObject
{
    private const string SelectedViewProperty = "selectedViewProperty";
    private const string TextWrapEnabledProperty = "textWrapEnabledProperty";
    private const string WindowWidthProperty = "windowWidthProperty";
    private const string WindowHeightProperty = "windowHeightProperty";

    private static readonly Lazy<SettingsHolder> LazyInstance = new(() => new SettingsHolder());

    private readonly ApplicationDataContainer _localSettings =
        ApplicationData.Current.LocalSettings;

    private SettingsHolder()
    {
        if (_localSettings.Values[SelectedViewProperty] is int selectedView) SelectedView = (ViewOption)selectedView;
        if (_localSettings.Values[TextWrapEnabledProperty] is bool textWrapEnabled) TextWrapEnabled = textWrapEnabled;
        if (_localSettings.Values[WindowHeightProperty] is double windowHeight) WindowHeight = windowHeight;
        if (_localSettings.Values[WindowWidthProperty] is double windowWidth) WindowWidth = windowWidth;
    }

    [ObservableProperty] public partial ViewOption SelectedView { get; set; } = ViewOption.PrettyView;
    [ObservableProperty] public partial bool TextWrapEnabled { get; set; } = false;
    [ObservableProperty] public partial double WindowWidth { get; set; } = 600;
    [ObservableProperty] public partial double WindowHeight { get; set; } = 600;

    public static SettingsHolder Instance => LazyInstance.Value;

    partial void OnSelectedViewChanged(ViewOption value)
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