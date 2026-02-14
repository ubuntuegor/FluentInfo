using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentInfoCommon;
using MediaInfoLib;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.AppLifecycle;

namespace FluentInfo;

public partial class AppModel : ObservableObject
{
    private static readonly Lazy<AppModel> LazyInstance = new(() => new AppModel());

    private static readonly Lazy<List<AppLanguage>> LazyAppLanguages = new(() =>
    {
        var languageCodes = Microsoft.Windows.Globalization.ApplicationLanguages.ManifestLanguages;
        return [.. languageCodes.Select(code => new AppLanguage(code, GetLanguageNameByLanguageCode(code)))];
    });

    private readonly MediaInfo _mediaInfoLib = new();
    public readonly MediaInfoTextParser InfoTextParser = new();

    private AppModel()
    {
        UpdateMediaInfoLanguage();
    }

    public static AppModel Instance => LazyInstance.Value;
    public static List<AppLanguage> AppLanguages => LazyAppLanguages.Value;

    public string? MediaInfoVersion => _mediaInfoLib.Option("info_version", string.Empty);
    [ObservableProperty] public partial string? CurrentFilePath { get; private set; }
    [ObservableProperty] public partial string? InfoText { get; private set; }

    [ObservableProperty]
    public partial string Language { get; private set; } = ApplicationLanguages.PrimaryLanguageOverride;

    public void OpenFile(string path)
    {
        CurrentFilePath = path;
        var success = _mediaInfoLib.Open(path);

        if (success)
        {
            _mediaInfoLib.Option("Inform", "Text");
            InfoText = _mediaInfoLib.Inform()!;
        }
        else
        {
            InfoText = null;
        }
    }

    [RelayCommand]
    private void ChangeLanguage(string langCode)
    {
        if (langCode != "")
        {
            Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = langCode;
        }
        else
        {
            // workaround for https://github.com/microsoft/WindowsAppSDK/issues/5335
            ApplicationLanguages.PrimaryLanguageOverride = langCode;
            AppInstance.Restart(CurrentFilePath != null ? '"' + CurrentFilePath + '"' : "");
        }

        UpdateMediaInfoLanguage();
        if (CurrentFilePath != null) OpenFile(CurrentFilePath);
        Language = langCode;
    }

    private void UpdateMediaInfoLanguage()
    {
        var languageText = new ResourceLoader().GetString("MediaInfoLanguage");
        InfoTextParser.UpdateLanguage(languageText);
        _mediaInfoLib.Option("Language", languageText);
    }

    private static string GetLanguageNameByLanguageCode(string languageCode)
    {
        return GetStringWithLanguageCode(languageCode, "LanguageName");
    }

    private static string GetStringWithLanguageCode(string languageCode, string resourceKey)
    {
        var resourceManager = new ResourceManager();
        var resourceContext = resourceManager.CreateResourceContext();
        resourceContext.QualifierValues["Language"] = languageCode;
        var resourceMap = resourceManager.MainResourceMap.GetSubtree("Resources");
        return resourceMap.GetValue(resourceKey, resourceContext).ValueAsString;
    }

    public record AppLanguage(string LanguageCode, string Name);
}