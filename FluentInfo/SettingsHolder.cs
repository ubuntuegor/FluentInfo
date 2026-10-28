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

    private static readonly Lazy<SettingsHolder> LazyInstance = new(() => new SettingsHolder());

    private readonly ApplicationDataContainer _localSettings =
        ApplicationData.Current.LocalSettings;

    private SettingsHolder()
    {
        if (_localSettings.Values[SelectedViewProperty] is int selectedView) SelectedView = (ViewOption)selectedView;
        if (_localSettings.Values[TextWrapEnabledProperty] is bool textWrapEnabled) TextWrapEnabled = textWrapEnabled;
    }

    [ObservableProperty] public partial ViewOption SelectedView { get; set; } = ViewOption.PrettyView;
    [ObservableProperty] public partial bool TextWrapEnabled { get; set; } = false;

    public static SettingsHolder Instance => LazyInstance.Value;

    partial void OnSelectedViewChanged(ViewOption value)
    {
        _localSettings.Values[SelectedViewProperty] = (int)value;
    }

    partial void OnTextWrapEnabledChanged(bool value)
    {
        _localSettings.Values[TextWrapEnabledProperty] = value;
    }
}