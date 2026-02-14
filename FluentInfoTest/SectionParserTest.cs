using FluentInfoCommon;
using MediaInfoLib;

namespace FluentInfoTest;

[TestClass]
[UsesVerify]
public partial class SectionParserTest
{
    private const string MediaFolder = @"Assets\media";
    private const string LanguagesFolder = @"Assets\langs";

    private readonly MediaInfo _mediaInfo = new();
    private readonly MediaInfoTextParser _mediaInfoTextParser = new();
    private readonly VerifySettings _settings = new();

    public SectionParserTest()
    {
        _settings.UseDirectory("Snapshots");
    }

    public static IEnumerable<(string FilePath, string LanguagePath)> ParserTestData
    {
        get
        {
            var files = Directory.GetFiles(MediaFolder).Select(Path.GetFileName);
            var languages = Directory.GetFiles(LanguagesFolder).Select(Path.GetFileName);
            return files.SelectMany(f => languages.Select(l => (f!, l!)));
        }
    }

    private List<Section> RunParser(string filePath, string languageText)
    {
        _mediaInfoTextParser.UpdateLanguage(languageText);
        _mediaInfo.Option("Language", languageText);
        _mediaInfo.Option("Inform", "Text");
        var mediaInfoText = _mediaInfo.Open(filePath) ? _mediaInfo.Inform() : null;
        return _mediaInfoTextParser.Parse(mediaInfoText!);
    }

    [TestMethod]
    [DynamicData(nameof(ParserTestData))]
    public Task TestParser(string fileName, string language)
    {
        var filePath = Path.Combine(MediaFolder, fileName);
        var languagePath = Path.Combine(LanguagesFolder, language);
        Assert.IsTrue(File.Exists(filePath), $"Media file {filePath} does not exist");
        Assert.IsTrue(File.Exists(languagePath), $"Language file {languagePath} does not exist");
        var sections = RunParser(filePath, File.ReadAllText(languagePath));
        return Verify(sections, _settings);
    }
}