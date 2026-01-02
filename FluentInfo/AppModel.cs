using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MediaInfoLib;

namespace FluentInfo;

public partial class AppModel : ObservableObject
{
    private static readonly Lazy<AppModel> LazyInstance = new(() => new AppModel());

    private readonly MediaInfo _mediaInfoLib = new();

    private AppModel()
    {
    }

    public static AppModel Instance => LazyInstance.Value;

    public string? MediaInfoVersion => _mediaInfoLib.Option("info_version", string.Empty);
    [ObservableProperty] public partial string? CurrentFilePath { get; private set; }
    [ObservableProperty] public partial string? InfoText { get; private set; }

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
}