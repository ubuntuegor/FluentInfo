using FluentInfoCommon;
using MediaInfoLib;
using SharpYaml;
using System.Text;

namespace FluentInfoTest;

[TestClass]
public class SectionParserTest
{
    private static readonly string MediaFolder = Path.Combine(GetTestSourceDirectory(), @"Assets\media");
    private static readonly string LanguagesFolder = Path.Combine(GetTestSourceDirectory(), @"Assets\langs");
    private static readonly string SnapshotsFolder = Path.Combine(GetTestSourceDirectory(), @"Snapshots");

    public TestContext TestContext { get; set; }

    private readonly MediaInfo _mediaInfo = new();
    private readonly MediaInfoTextParser _mediaInfoTextParser = new();

    private static string GetTestSourceDirectory()
    {
        return Path.GetDirectoryName(new System.Diagnostics.StackFrame(true).GetFileName())!;
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

    private string GetSnapshotFileName()
    {
        var result = new StringBuilder(TestContext.TestName);
        var testData = TestContext.TestData;

        if (testData != null)
        {
            result.Append('(');
            for (var i = 0; i < testData.Length; i++)
            {
                if (i != 0)
                {
                    result.Append(", ");
                }

                result.Append(testData[i]);
            }
            result.Append(')');
        }

        result.Append(".txt");
        return result.ToString();
    }

    private void CheckAgainstSnapshot(string data)
    {
        var snapshotsForClassPath = Path.Combine(SnapshotsFolder, TestContext.FullyQualifiedTestClassName);
        var snapshotFileName = GetSnapshotFileName();
        var snapshotPath = Path.Combine(snapshotsForClassPath, snapshotFileName);
        var expectedData = File.Exists(snapshotPath) ? File.ReadAllText(snapshotPath) : null;
        Directory.CreateDirectory(snapshotsForClassPath);
        File.WriteAllText(snapshotPath, data);
        Assert.AreEqual(expectedData, data);
    }

    [TestMethod]
    [DynamicData(nameof(ParserTestData))]
    public void TestParser(string fileName, string language)
    {
        var filePath = Path.Combine(MediaFolder, fileName);
        var languagePath = Path.Combine(LanguagesFolder, language);
        Assert.IsTrue(File.Exists(filePath), $"Media file {filePath} does not exist");
        Assert.IsTrue(File.Exists(languagePath), $"Language file {languagePath} does not exist");
        var sections = RunParser(filePath, File.ReadAllText(languagePath));

        var data = YamlSerializer.Serialize(sections);
        CheckAgainstSnapshot(data);
    }
}